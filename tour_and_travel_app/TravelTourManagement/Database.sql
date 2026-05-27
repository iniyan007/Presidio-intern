-- =============================================================================
-- Travel & Tour Package Management System
-- PostgreSQL DDL Script  — Version 2
-- Changes from v1:
--   + package_transport        (pickup/drop/transport details per day segment)
--   + package_accommodation    (hotel details per itinerary day)
--   + package_inclusions       (what's included / excluded in a package)
--   + package_highlights       (quick bullet points shown on listing card)
--   + platform_config          (admin-controlled platform fee %)
--   + revenue_summary VIEW     (packager + admin revenue per booking)
--   + messages                 (user ↔ packager in-app chat)
--   ~ itinerary_days           (added session: morning/afternoon/evening on activities)
--   ~ bookings                 (added platform_fee, packager_amount columns)
--   ~ packages                 (added package_type, min_age, cancellation_policy)
-- =============================================================================

-- -----------------------------------------------------------------------------
-- EXTENSIONS
-- -----------------------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";


-- =============================================================================
-- ENUMS
-- =============================================================================
CREATE TYPE user_role AS ENUM ('user', 'admin', 'packager');
CREATE TYPE packager_status AS ENUM ('pending', 'approved', 'suspended', 'deactivated');
CREATE TYPE package_status AS ENUM ('draft', 'pending_review', 'published', 'archived');
CREATE TYPE package_type AS ENUM ('group', 'private', 'honeymoon', 'family', 'adventure', 'pilgrimage');

CREATE TYPE booking_status AS ENUM (
    'pending', 'confirmed', 'cancelled', 'completed', 'refunded'
);
CREATE TYPE payment_status AS ENUM (
    'unpaid', 'partial', 'paid', 'refunded', 'failed'
);
CREATE TYPE review_status AS ENUM ('pending', 'published', 'flagged', 'removed');

CREATE TYPE media_category AS ENUM (
    'hotel', 'transport', 'food', 'destination', 'activity', 'cover'
);
CREATE TYPE document_status AS ENUM ('uploaded', 'verified', 'rejected');
CREATE TYPE notification_type AS ENUM (
    'booking', 'payment', 'review', 'approval', 'system', 'message'
);
CREATE TYPE transport_mode AS ENUM (
    'bus', 'train', 'flight', 'car', 'boat', 'van', 'walk', 'other'
);
CREATE TYPE meal_type AS ENUM (
    'breakfast', 'lunch', 'dinner', 'all_inclusive', 'none'
);
CREATE TYPE day_session AS ENUM ('morning', 'afternoon', 'evening', 'full_day');
CREATE TYPE message_sender_role AS ENUM ('user', 'packager');
CREATE TYPE inclusion_type AS ENUM ('included', 'excluded', 'optional');


-- =============================================================================
-- MODULE 1 — IDENTITY & ACCESS
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: users
-- Single auth table for all roles. Role determines what extra profile tables
-- exist (packagers is 1:1 for packager-role users).
-- -----------------------------------------------------------------------------
CREATE TABLE users (
    id                  UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
    full_name           VARCHAR(150) NOT NULL,
    email               VARCHAR(255) NOT NULL UNIQUE,
    password_hash       VARCHAR(255) NOT NULL,
    phone               VARCHAR(20),
    profile_picture     VARCHAR(500),           -- local path: /uploads/users/{id}/avatar.jpg
    role                user_role    NOT NULL DEFAULT 'user',
    is_active           BOOLEAN      NOT NULL DEFAULT TRUE,
    is_email_verified   BOOLEAN      NOT NULL DEFAULT FALSE,
    last_login_at       TIMESTAMP,
    created_at          TIMESTAMP    NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP    NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_users_email  ON users (email);
CREATE INDEX idx_users_role   ON users (role);
CREATE INDEX idx_users_active ON users (is_active);


-- -----------------------------------------------------------------------------
-- TABLE: packagers
-- Extended business profile for packager-role users. Admin approves/deactivates.
-- avg_rating is denormalised and kept in sync by trigger fn_recalculate_ratings.
-- -----------------------------------------------------------------------------
CREATE TABLE packagers (
    id                  UUID             PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id             UUID             NOT NULL UNIQUE REFERENCES users (id) ON DELETE CASCADE,
    company_name        VARCHAR(200)     NOT NULL,
    business_license_no VARCHAR(100),
    description         TEXT,
    contact_email       VARCHAR(255),
    contact_phone       VARCHAR(20),
    website_url         VARCHAR(500),
    status              packager_status  NOT NULL DEFAULT 'pending',
    approved_by         UUID             REFERENCES users (id) ON DELETE SET NULL,
    approved_at         TIMESTAMP,
    deactivated_at      TIMESTAMP,
    deactivation_reason TEXT,
    avg_rating          NUMERIC(3,2)     NOT NULL DEFAULT 0.00
        CONSTRAINT chk_packager_avg_rating CHECK (avg_rating BETWEEN 0 AND 5),
    total_reviews       INTEGER          NOT NULL DEFAULT 0,
    created_at          TIMESTAMP        NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP        NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_packagers_user_id ON packagers (user_id);
CREATE INDEX idx_packagers_status  ON packagers (status);
CREATE INDEX idx_packagers_rating  ON packagers (avg_rating DESC);


-- =============================================================================
-- MODULE 2 — PACKAGE MANAGEMENT
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: packages
-- Core catalogue entry. Published packages are visible to users for browsing
-- and booking. Follows draft → pending_review → published → archived workflow.
-- NEW: package_type, min_age, cancellation_policy
-- -----------------------------------------------------------------------------
CREATE TABLE packages (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    packager_id             UUID            NOT NULL REFERENCES packagers (id) ON DELETE RESTRICT,
    title                   VARCHAR(300)    NOT NULL,
    description             TEXT,
    package_type            package_type    NOT NULL DEFAULT 'group',
    destination             VARCHAR(200)    NOT NULL,
    country                 VARCHAR(100)    NOT NULL,
    city                    VARCHAR(100),
    duration_days           INTEGER         NOT NULL
        CONSTRAINT chk_package_duration CHECK (duration_days > 0),
    duration_nights         INTEGER         NOT NULL DEFAULT 0
        CONSTRAINT chk_package_nights CHECK (duration_nights >= 0),
    max_capacity            INTEGER         NOT NULL
        CONSTRAINT chk_package_capacity CHECK (max_capacity > 0),
    current_bookings        INTEGER         NOT NULL DEFAULT 0
        CONSTRAINT chk_package_bookings CHECK (current_bookings >= 0),
    min_age                 INTEGER         DEFAULT NULL,               -- NULL = no restriction
    cancellation_policy     TEXT,                                       -- free-text: e.g. "Free cancellation up to 7 days"
    status                  package_status  NOT NULL DEFAULT 'draft',
    is_featured             BOOLEAN         NOT NULL DEFAULT FALSE,
    avg_rating              NUMERIC(3,2)    NOT NULL DEFAULT 0.00
        CONSTRAINT chk_package_avg_rating CHECK (avg_rating BETWEEN 0 AND 5),
    total_reviews           INTEGER         NOT NULL DEFAULT 0,
    created_at              TIMESTAMP       NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMP       NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_package_bookings_capacity CHECK (current_bookings <= max_capacity)
);

CREATE INDEX idx_packages_packager_id  ON packages (packager_id);
CREATE INDEX idx_packages_status       ON packages (status);
CREATE INDEX idx_packages_destination  ON packages (destination);
CREATE INDEX idx_packages_country      ON packages (country);
CREATE INDEX idx_packages_type         ON packages (package_type);
CREATE INDEX idx_packages_featured     ON packages (is_featured) WHERE is_featured = TRUE;
CREATE INDEX idx_packages_rating       ON packages (avg_rating DESC);
CREATE INDEX idx_packages_title_trgm   ON packages USING GIN (title gin_trgm_ops);
CREATE INDEX idx_packages_dest_trgm    ON packages USING GIN (destination gin_trgm_ops);


-- -----------------------------------------------------------------------------
-- TABLE: package_highlights
-- NEW — Short bullet points shown on the package card/listing page.
-- e.g. "Return airfare included", "3-star hotel stay", "Expert guide"
-- -----------------------------------------------------------------------------
CREATE TABLE package_highlights (
    id              UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
    package_id      UUID         NOT NULL REFERENCES packages (id) ON DELETE CASCADE,
    highlight_text  VARCHAR(200) NOT NULL,
    display_order   INTEGER      NOT NULL DEFAULT 0
);

CREATE INDEX idx_highlights_package_id ON package_highlights (package_id);


-- -----------------------------------------------------------------------------
-- TABLE: package_inclusions
-- NEW — What is included, excluded, or optional in the package price.
-- e.g. Included: "Airport transfers", Excluded: "Travel insurance"
-- -----------------------------------------------------------------------------
CREATE TABLE package_inclusions (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    package_id      UUID            NOT NULL REFERENCES packages (id) ON DELETE CASCADE,
    type            inclusion_type  NOT NULL,
    description     VARCHAR(300)    NOT NULL,
    display_order   INTEGER         NOT NULL DEFAULT 0
);

CREATE INDEX idx_inclusions_package_id ON package_inclusions (package_id);


-- -----------------------------------------------------------------------------
-- TABLE: package_seasonal_pricing
-- A package can have multiple pricing windows (peak, off-season, holiday).
-- Bookings snapshot the seasonal_pricing_id to lock in price at booking time.
-- -----------------------------------------------------------------------------
CREATE TABLE package_seasonal_pricing (
    id               UUID          PRIMARY KEY DEFAULT uuid_generate_v4(),
    package_id       UUID          NOT NULL REFERENCES packages (id) ON DELETE CASCADE,
    season_name      VARCHAR(100)  NOT NULL,        -- e.g. 'Peak Summer 2026'
    start_date       DATE          NOT NULL,
    end_date         DATE          NOT NULL,
    base_price       NUMERIC(12,2) NOT NULL
        CONSTRAINT chk_pricing_base_price CHECK (base_price >= 0),
    child_price      NUMERIC(12,2) NOT NULL DEFAULT 0
        CONSTRAINT chk_pricing_child_price CHECK (child_price >= 0),
    discount_percent NUMERIC(5,2)  NOT NULL DEFAULT 0
        CONSTRAINT chk_pricing_discount CHECK (discount_percent BETWEEN 0 AND 100),
    available_slots  INTEGER       NOT NULL
        CONSTRAINT chk_pricing_slots CHECK (available_slots >= 0),
    is_active        BOOLEAN       NOT NULL DEFAULT TRUE,

    CONSTRAINT chk_pricing_date_range CHECK (end_date > start_date)
);

CREATE INDEX idx_pricing_package_id ON package_seasonal_pricing (package_id);
CREATE INDEX idx_pricing_dates      ON package_seasonal_pricing (start_date, end_date);
CREATE INDEX idx_pricing_active     ON package_seasonal_pricing (is_active) WHERE is_active = TRUE;


-- -----------------------------------------------------------------------------
-- TABLE: package_media
-- Images/videos uploaded by packagers. Stored locally in:
--   /uploads/packages/{package_id}/media/
-- Swap file_path → blob_url when migrating to Azure Blob Storage.
-- -----------------------------------------------------------------------------
CREATE TABLE package_media (
    id              UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
    package_id      UUID           NOT NULL REFERENCES packages (id) ON DELETE CASCADE,
    file_path       VARCHAR(500)   NOT NULL,     -- /uploads/packages/{id}/media/hotel1.jpg
    file_name       VARCHAR(255)   NOT NULL,
    category        media_category NOT NULL DEFAULT 'destination',
    caption         VARCHAR(300),
    display_order   INTEGER        NOT NULL DEFAULT 0,
    is_primary      BOOLEAN        NOT NULL DEFAULT FALSE,  -- hero image for listing card
    file_size_bytes BIGINT,
    mime_type       VARCHAR(100),
    uploaded_at     TIMESTAMP      NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_media_package_id ON package_media (package_id);
CREATE INDEX idx_media_category   ON package_media (category);
CREATE INDEX idx_media_primary    ON package_media (package_id, is_primary) WHERE is_primary = TRUE;


-- =============================================================================
-- MODULE 3 — ITINERARY (Day + Session level planning)
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: itinerary_days
-- One row per day of the package. Stores the day-level overview.
-- Accommodation and meals are in separate tables for structured querying.
-- -----------------------------------------------------------------------------
CREATE TABLE itinerary_days (
    id              UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
    package_id      UUID         NOT NULL REFERENCES packages (id) ON DELETE CASCADE,
    day_number      INTEGER      NOT NULL
        CONSTRAINT chk_itinerary_day_number CHECK (day_number > 0),
    title           VARCHAR(200) NOT NULL,   -- e.g. 'Arrival & Cochin City Tour'
    description     TEXT,
    location        VARCHAR(200),            -- primary location for the day
    created_at      TIMESTAMP    NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_itinerary_day UNIQUE (package_id, day_number)
);

CREATE INDEX idx_itinerary_days_package_id ON itinerary_days (package_id);


-- -----------------------------------------------------------------------------
-- TABLE: package_accommodation
-- NEW — Hotel/stay details per itinerary day.
-- Stores the hotel name, star rating, room type, and check-in/out for the day.
-- A packager can upload hotel photos via package_media (category = 'hotel').
-- -----------------------------------------------------------------------------
CREATE TABLE package_accommodation (
    id              UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
    itinerary_day_id UUID        NOT NULL REFERENCES itinerary_days (id) ON DELETE CASCADE,
    hotel_name      VARCHAR(200) NOT NULL,
    hotel_address   VARCHAR(400),
    star_rating     SMALLINT
        CONSTRAINT chk_hotel_star CHECK (star_rating BETWEEN 1 AND 5),
    room_type       VARCHAR(100),           -- 'Deluxe Twin', 'Standard Double', etc.
    check_in_time   TIME,
    check_out_time  TIME,
    amenities       TEXT,                   -- free-text: "WiFi, Pool, AC, Breakfast"
    notes           TEXT
);

CREATE INDEX idx_accommodation_day_id ON package_accommodation (itinerary_day_id);


-- -----------------------------------------------------------------------------
-- TABLE: package_transport
-- NEW — Transport leg details (pickup point → drop point) per day or segment.
-- Each row is one leg: e.g. Day 1 morning: Airport → Hotel by AC Bus.
-- -----------------------------------------------------------------------------
CREATE TABLE package_transport (
    id                  UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
    itinerary_day_id    UUID           NOT NULL REFERENCES itinerary_days (id) ON DELETE CASCADE,
    segment_order       INTEGER        NOT NULL DEFAULT 1,
    mode                transport_mode NOT NULL DEFAULT 'bus',
    vehicle_description VARCHAR(200),           -- e.g. 'AC Volvo Bus', 'IndiGo Flight'
    pickup_point        VARCHAR(300)   NOT NULL, -- e.g. 'Chennai Airport, Terminal 1'
    drop_point          VARCHAR(300)   NOT NULL, -- e.g. 'Hotel Radisson, Ooty'
    pickup_time         TIME,
    drop_time           TIME,
    distance_km         NUMERIC(8,2),
    notes               TEXT
);

CREATE INDEX idx_transport_day_id ON package_transport (itinerary_day_id);


-- -----------------------------------------------------------------------------
-- TABLE: itinerary_day_meals
-- NEW — Meal plan per session per day. Replaces the free-text meal_plan column.
-- e.g. Day 1 Breakfast: Complimentary at hotel | Day 1 Dinner: Local restaurant
-- -----------------------------------------------------------------------------
CREATE TABLE itinerary_day_meals (
    id              UUID       PRIMARY KEY DEFAULT uuid_generate_v4(),
    itinerary_day_id UUID      NOT NULL REFERENCES itinerary_days (id) ON DELETE CASCADE,
    meal            meal_type  NOT NULL,
    venue           VARCHAR(200),
    description     VARCHAR(300),
    is_included     BOOLEAN    NOT NULL DEFAULT TRUE   -- FALSE = own cost
);

CREATE INDEX idx_meals_day_id ON itinerary_day_meals (itinerary_day_id);


-- -----------------------------------------------------------------------------
-- TABLE: itinerary_activities
-- Activities planned per day, now with a day_session column (morning /
-- afternoon / evening / full_day) for session-wise planning.
-- is_optional + extra_cost supports upsell add-ons.
-- -----------------------------------------------------------------------------
CREATE TABLE itinerary_activities (
    id               UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
    itinerary_day_id UUID         NOT NULL REFERENCES itinerary_days (id) ON DELETE CASCADE,
    session          day_session  NOT NULL DEFAULT 'morning',
    sequence_order   INTEGER      NOT NULL DEFAULT 1,
    activity_title   VARCHAR(200) NOT NULL,
    description      TEXT,
    activity_type    VARCHAR(100),       -- 'Sightseeing', 'Adventure', 'Cultural', 'Shopping'
    location         VARCHAR(200),
    duration_minutes INTEGER
        CONSTRAINT chk_activity_duration CHECK (duration_minutes > 0),
    is_optional      BOOLEAN      NOT NULL DEFAULT FALSE,
    extra_cost       NUMERIC(10,2) NOT NULL DEFAULT 0
        CONSTRAINT chk_activity_extra_cost CHECK (extra_cost >= 0)
);

CREATE INDEX idx_activities_day_id ON itinerary_activities (itinerary_day_id);
CREATE INDEX idx_activities_session ON itinerary_activities (itinerary_day_id, session);


-- =============================================================================
-- MODULE 4 — BOOKINGS & PAYMENTS
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: platform_config
-- NEW — Admin-managed configuration. Stores the platform fee percentage that
-- is added on top of the packager price. Admin can update this at any time.
-- Only one active row should exist (enforced by is_active constraint pattern).
-- -----------------------------------------------------------------------------
CREATE TABLE platform_config (
    id                  UUID          PRIMARY KEY DEFAULT uuid_generate_v4(),
    platform_fee_percent NUMERIC(5,2) NOT NULL DEFAULT 5.00
        CONSTRAINT chk_platform_fee CHECK (platform_fee_percent BETWEEN 0 AND 100),
    gst_percent         NUMERIC(5,2)  NOT NULL DEFAULT 18.00
        CONSTRAINT chk_gst CHECK (gst_percent BETWEEN 0 AND 100),
    updated_by          UUID          REFERENCES users (id) ON DELETE SET NULL,
    updated_at          TIMESTAMP     NOT NULL DEFAULT NOW(),
    note                TEXT
);

-- Seed one default config row
INSERT INTO platform_config (platform_fee_percent, gst_percent)
VALUES (5.00, 18.00);


-- -----------------------------------------------------------------------------
-- TABLE: bookings
-- Core booking record. Snapshots pricing at time of booking.
-- NEW: platform_fee_percent, platform_fee_amount, packager_amount columns for
--      transparent revenue split. Admin earns platform_fee_amount, packager
--      earns packager_amount. total_amount = packager_amount + platform_fee_amount + tax_amount.
-- -----------------------------------------------------------------------------
CREATE TABLE bookings (
    id                   UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id              UUID           NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    package_id           UUID           NOT NULL REFERENCES packages (id) ON DELETE RESTRICT,
    seasonal_pricing_id  UUID           NOT NULL REFERENCES package_seasonal_pricing (id) ON DELETE RESTRICT,
    booking_reference    VARCHAR(30)    NOT NULL UNIQUE,     -- TRP-2026-00001
    adult_count          INTEGER        NOT NULL DEFAULT 1
        CONSTRAINT chk_booking_adults CHECK (adult_count > 0),
    child_count          INTEGER        NOT NULL DEFAULT 0
        CONSTRAINT chk_booking_children CHECK (child_count >= 0),

    -- Price breakdown (snapshotted at booking time)
    packager_base_amount NUMERIC(12,2)  NOT NULL
        CONSTRAINT chk_packager_base CHECK (packager_base_amount >= 0),
    platform_fee_percent NUMERIC(5,2)   NOT NULL,           -- snapshot of platform_config at booking time
    platform_fee_amount  NUMERIC(12,2)  NOT NULL
        CONSTRAINT chk_platform_fee_amt CHECK (platform_fee_amount >= 0),
    tax_amount           NUMERIC(12,2)  NOT NULL DEFAULT 0
        CONSTRAINT chk_tax_amount CHECK (tax_amount >= 0),
    total_amount         NUMERIC(12,2)  NOT NULL
        CONSTRAINT chk_total_amount CHECK (total_amount >= 0),
    paid_amount          NUMERIC(12,2)  NOT NULL DEFAULT 0
        CONSTRAINT chk_paid_amount CHECK (paid_amount >= 0),

    status               booking_status NOT NULL DEFAULT 'pending',
    payment_status       payment_status NOT NULL DEFAULT 'unpaid',
    travel_date          DATE           NOT NULL,
    return_date          DATE           NOT NULL,
    special_requests     TEXT,
    booked_at            TIMESTAMP      NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMP      NOT NULL DEFAULT NOW(),
    cancelled_at         TIMESTAMP,
    cancellation_reason  TEXT,

    CONSTRAINT chk_booking_dates        CHECK (return_date > travel_date),
    CONSTRAINT chk_paid_lte_total       CHECK (paid_amount <= total_amount),
    CONSTRAINT chk_total_amount_breakup CHECK (
        total_amount = packager_base_amount + platform_fee_amount + tax_amount
    )
);

CREATE INDEX idx_bookings_user_id      ON bookings (user_id);
CREATE INDEX idx_bookings_package_id   ON bookings (package_id);
CREATE INDEX idx_bookings_status       ON bookings (status);
CREATE INDEX idx_bookings_payment      ON bookings (payment_status);
CREATE INDEX idx_bookings_travel_date  ON bookings (travel_date);
CREATE INDEX idx_bookings_reference    ON bookings (booking_reference);


-- -----------------------------------------------------------------------------
-- TABLE: booking_travelers
-- Individual traveler details per booking (supports group bookings).
-- is_primary = TRUE marks the lead booker (same as the user who booked).
-- -----------------------------------------------------------------------------
CREATE TABLE booking_travelers (
    id              UUID         PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id      UUID         NOT NULL REFERENCES bookings (id) ON DELETE CASCADE,
    full_name       VARCHAR(150) NOT NULL,
    passport_number VARCHAR(50),
    date_of_birth   DATE,
    nationality     VARCHAR(100),
    is_primary      BOOLEAN      NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_travelers_booking_id ON booking_travelers (booking_id);


-- -----------------------------------------------------------------------------
-- TABLE: travel_documents
-- Documents per traveler (passport scans, visas, tickets).
-- Stored under /uploads/documents/{booking_id}/{traveler_id}/
-- -----------------------------------------------------------------------------
CREATE TABLE travel_documents (
    id                UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id        UUID            NOT NULL REFERENCES bookings (id) ON DELETE CASCADE,
    traveler_id       UUID            REFERENCES booking_travelers (id) ON DELETE SET NULL,
    document_type     VARCHAR(100)    NOT NULL,  -- 'passport', 'visa', 'ticket', 'insurance'
    file_path         VARCHAR(500)    NOT NULL,
    file_name         VARCHAR(255)    NOT NULL,
    original_filename VARCHAR(255),
    file_size_bytes   BIGINT,
    mime_type         VARCHAR(100),
    status            document_status NOT NULL DEFAULT 'uploaded',
    uploaded_at       TIMESTAMP       NOT NULL DEFAULT NOW(),
    verified_at       TIMESTAMP,
    verified_by       UUID            REFERENCES users (id) ON DELETE SET NULL
);

CREATE INDEX idx_docs_booking_id  ON travel_documents (booking_id);
CREATE INDEX idx_docs_traveler_id ON travel_documents (traveler_id);
CREATE INDEX idx_docs_status      ON travel_documents (status);


-- -----------------------------------------------------------------------------
-- TABLE: payments
-- One row per payment attempt. Supports partial payments, refunds, retries.
-- gateway_response stores raw JSON from the payment gateway for debugging.
-- -----------------------------------------------------------------------------
CREATE TABLE payments (
    id              UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id      UUID           NOT NULL REFERENCES bookings (id) ON DELETE RESTRICT,
    transaction_id  VARCHAR(200)   NOT NULL UNIQUE,
    amount          NUMERIC(12,2)  NOT NULL
        CONSTRAINT chk_payment_amount CHECK (amount > 0),
    currency        VARCHAR(10)    NOT NULL DEFAULT 'INR',
    payment_method  VARCHAR(100),           -- 'upi', 'card', 'netbanking', 'wallet'
    status          payment_status NOT NULL DEFAULT 'unpaid',
    gateway_response TEXT,
    paid_at         TIMESTAMP,
    refunded_at     TIMESTAMP,
    refund_amount   NUMERIC(12,2)  DEFAULT 0
        CONSTRAINT chk_refund_amount CHECK (refund_amount >= 0)
);

CREATE INDEX idx_payments_booking_id ON payments (booking_id);
CREATE INDEX idx_payments_status     ON payments (status);
CREATE INDEX idx_payments_paid_at    ON payments (paid_at);


-- =============================================================================
-- MODULE 5 — REVIEWS
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: reviews
-- Only allowed after booking.status = 'completed'. The booking_id UNIQUE
-- constraint enforces one review per trip. Admin moderates via status column.
-- Ratings stored in 6 dimensions; trigger keeps packager/package avg_rating
-- denormalized for fast listing queries.
-- -----------------------------------------------------------------------------
CREATE TABLE reviews (
    id                   UUID          PRIMARY KEY DEFAULT uuid_generate_v4(),
    booking_id           UUID          NOT NULL UNIQUE REFERENCES bookings (id) ON DELETE RESTRICT,
    user_id              UUID          NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    package_id           UUID          NOT NULL REFERENCES packages (id) ON DELETE RESTRICT,
    packager_id          UUID          NOT NULL REFERENCES packagers (id) ON DELETE RESTRICT,
    overall_rating       SMALLINT      NOT NULL
        CONSTRAINT chk_review_overall CHECK (overall_rating BETWEEN 1 AND 5),
    accommodation_rating SMALLINT
        CONSTRAINT chk_review_accomm CHECK (accommodation_rating BETWEEN 1 AND 5),
    transport_rating     SMALLINT
        CONSTRAINT chk_review_transport CHECK (transport_rating BETWEEN 1 AND 5),
    food_rating          SMALLINT
        CONSTRAINT chk_review_food CHECK (food_rating BETWEEN 1 AND 5),
    guide_rating         SMALLINT
        CONSTRAINT chk_review_guide CHECK (guide_rating BETWEEN 1 AND 5),
    value_rating         SMALLINT
        CONSTRAINT chk_review_value CHECK (value_rating BETWEEN 1 AND 5),
    comment              TEXT,
    is_verified_traveler BOOLEAN       NOT NULL DEFAULT TRUE,
    status               review_status NOT NULL DEFAULT 'pending',
    admin_note           TEXT,
    created_at           TIMESTAMP     NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMP     NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_reviews_package_id  ON reviews (package_id);
CREATE INDEX idx_reviews_packager_id ON reviews (packager_id);
CREATE INDEX idx_reviews_user_id     ON reviews (user_id);
CREATE INDEX idx_reviews_status      ON reviews (status);
CREATE INDEX idx_reviews_overall     ON reviews (overall_rating DESC);


-- -----------------------------------------------------------------------------
-- TABLE: review_media
-- Photos/videos uploaded by users alongside their review.
-- Stored under /uploads/reviews/{review_id}/
-- -----------------------------------------------------------------------------
CREATE TABLE review_media (
    id          UUID           PRIMARY KEY DEFAULT uuid_generate_v4(),
    review_id   UUID           NOT NULL REFERENCES reviews (id) ON DELETE CASCADE,
    file_path   VARCHAR(500)   NOT NULL,
    file_name   VARCHAR(255)   NOT NULL,
    category    media_category NOT NULL DEFAULT 'destination',
    uploaded_at TIMESTAMP      NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_review_media_review_id ON review_media (review_id);


-- =============================================================================
-- MODULE 6 — MESSAGING (User ↔ Packager)
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: message_threads
-- NEW — One thread per user-packager pair per package. A thread is opened
-- automatically when a user sends the first message from a package detail page.
-- -----------------------------------------------------------------------------
CREATE TABLE message_threads (
    id          UUID      PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id     UUID      NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    packager_id UUID      NOT NULL REFERENCES packagers (id) ON DELETE CASCADE,
    package_id  UUID      REFERENCES packages (id) ON DELETE SET NULL,
    created_at  TIMESTAMP NOT NULL DEFAULT NOW(),
    last_message_at TIMESTAMP,

    CONSTRAINT uq_thread UNIQUE (user_id, packager_id, package_id)
);

CREATE INDEX idx_threads_user_id     ON message_threads (user_id);
CREATE INDEX idx_threads_packager_id ON message_threads (packager_id);
CREATE INDEX idx_threads_package_id  ON message_threads (package_id);


-- -----------------------------------------------------------------------------
-- TABLE: messages
-- NEW — Individual messages within a thread. sender_role tells whether the
-- sender is the 'user' or the 'packager' side, sender_id is the users.id.
-- is_read tracks unread message count for notification badges.
-- -----------------------------------------------------------------------------
CREATE TABLE messages (
    id          UUID                PRIMARY KEY DEFAULT uuid_generate_v4(),
    thread_id   UUID                NOT NULL REFERENCES message_threads (id) ON DELETE CASCADE,
    sender_id   UUID                NOT NULL REFERENCES users (id) ON DELETE RESTRICT,
    sender_role message_sender_role NOT NULL,
    body        TEXT                NOT NULL,
    is_read     BOOLEAN             NOT NULL DEFAULT FALSE,
    sent_at     TIMESTAMP           NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_messages_thread_id ON messages (thread_id);
CREATE INDEX idx_messages_sender_id ON messages (sender_id);
CREATE INDEX idx_messages_unread    ON messages (thread_id, is_read) WHERE is_read = FALSE;


-- =============================================================================
-- MODULE 7 — SYSTEM TABLES
-- =============================================================================

-- -----------------------------------------------------------------------------
-- TABLE: notifications
-- In-app notifications for all user types. NEW: 'message' notification_type
-- added so users/packagers get a badge when a new message arrives.
-- reference_id is a generic pointer to the related entity.
-- -----------------------------------------------------------------------------
CREATE TABLE notifications (
    id          UUID              PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id     UUID              NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    title       VARCHAR(200)      NOT NULL,
    message     TEXT              NOT NULL,
    type        notification_type NOT NULL,
    reference_id UUID,
    is_read     BOOLEAN           NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMP         NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_notifications_user_id ON notifications (user_id);
CREATE INDEX idx_notifications_unread  ON notifications (user_id, is_read) WHERE is_read = FALSE;
CREATE INDEX idx_notifications_type    ON notifications (type);


-- -----------------------------------------------------------------------------
-- TABLE: audit_logs
-- Immutable log of all admin and packager actions. Used for the admin
-- dashboard activity feed and compliance. old_values / new_values are JSONB
-- so any entity's before-after diff can be stored without a schema change.
-- -----------------------------------------------------------------------------
CREATE TABLE audit_logs (
    id           UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    performed_by UUID        REFERENCES users (id) ON DELETE SET NULL,
    entity_type  VARCHAR(100) NOT NULL,   -- 'package', 'booking', 'packager', 'review'
    entity_id    UUID        NOT NULL,
    action       VARCHAR(50) NOT NULL,    -- 'CREATE', 'UPDATE', 'DELETE', 'APPROVE', 'DEACTIVATE'
    old_values   JSONB,
    new_values   JSONB,
    ip_address   VARCHAR(50),
    user_agent   VARCHAR(500),
    performed_at TIMESTAMP   NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_audit_entity       ON audit_logs (entity_type, entity_id);
CREATE INDEX idx_audit_performed_by ON audit_logs (performed_by);
CREATE INDEX idx_audit_performed_at ON audit_logs (performed_at DESC);
CREATE INDEX idx_audit_action       ON audit_logs (action);


-- =============================================================================
-- VIEWS
-- =============================================================================

-- -----------------------------------------------------------------------------
-- VIEW: v_booking_revenue
-- NEW — Joins bookings with package + packager for the revenue dashboards.
-- Packager dashboard: filter by packager_id for "revenue I earned".
-- Admin dashboard : aggregate across all rows for total platform revenue.
-- -----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_booking_revenue AS
SELECT
    b.id                    AS booking_id,
    b.booking_reference,
    b.booked_at,
    b.status                AS booking_status,
    b.payment_status,
    b.travel_date,
    b.return_date,

    -- Package info
    p.id                    AS package_id,
    p.title                 AS package_title,
    p.destination,

    -- Packager info
    pk.id                   AS packager_id,
    pk.company_name         AS packager_name,

    -- User (booker) info
    u.id                    AS user_id,
    u.full_name             AS user_name,
    u.email                 AS user_email,
    u.phone                 AS user_phone,

    -- Revenue breakdown
    b.adult_count,
    b.child_count,
    b.packager_base_amount,
    b.platform_fee_percent,
    b.platform_fee_amount,
    b.tax_amount,
    b.total_amount,
    b.paid_amount
FROM bookings b
JOIN packages  p  ON b.package_id   = p.id
JOIN packagers pk ON p.packager_id  = pk.id
JOIN users     u  ON b.user_id      = u.id;


-- -----------------------------------------------------------------------------
-- VIEW: v_packager_revenue_summary
-- NEW — One row per packager with total revenue and booking counts.
-- Admin can see all rows; packager queries their own packager_id.
-- -----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_packager_revenue_summary AS
SELECT
    pk.id                           AS packager_id,
    pk.company_name,
    COUNT(b.id)                     AS total_bookings,
    COUNT(b.id) FILTER (WHERE b.status = 'confirmed')   AS confirmed_bookings,
    COUNT(b.id) FILTER (WHERE b.status = 'completed')   AS completed_bookings,
    COUNT(b.id) FILTER (WHERE b.status = 'cancelled')   AS cancelled_bookings,
    COALESCE(SUM(b.packager_base_amount)
        FILTER (WHERE b.payment_status = 'paid'), 0)    AS total_earned,
    COALESCE(SUM(b.platform_fee_amount)
        FILTER (WHERE b.payment_status = 'paid'), 0)    AS total_platform_fee,
    COALESCE(SUM(b.total_amount)
        FILTER (WHERE b.payment_status = 'paid'), 0)    AS total_gmv
FROM packagers pk
LEFT JOIN packages  p ON p.packager_id = pk.id
LEFT JOIN bookings  b ON b.package_id  = p.id
GROUP BY pk.id, pk.company_name;


-- =============================================================================
-- TRIGGERS
-- =============================================================================

-- -----------------------------------------------------------------------------
-- fn_set_updated_at — auto-stamp updated_at on every UPDATE
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
CREATE TRIGGER trg_packagers_updated_at
    BEFORE UPDATE ON packagers FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
CREATE TRIGGER trg_packages_updated_at
    BEFORE UPDATE ON packages FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
CREATE TRIGGER trg_bookings_updated_at
    BEFORE UPDATE ON bookings FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
CREATE TRIGGER trg_reviews_updated_at
    BEFORE UPDATE ON reviews FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();


-- -----------------------------------------------------------------------------
-- fn_recalculate_ratings — keeps packager + package avg_rating in sync
-- Fires after any review INSERT, DELETE, or UPDATE of the status column.
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_recalculate_ratings()
RETURNS TRIGGER AS $$
DECLARE
    v_package_id  UUID := COALESCE(NEW.package_id,  OLD.package_id);
    v_packager_id UUID := COALESCE(NEW.packager_id, OLD.packager_id);
BEGIN
    UPDATE packages
    SET
        avg_rating    = COALESCE((
            SELECT ROUND(AVG(overall_rating)::NUMERIC, 2)
            FROM reviews WHERE package_id = v_package_id AND status = 'published'
        ), 0),
        total_reviews = (
            SELECT COUNT(*) FROM reviews
            WHERE package_id = v_package_id AND status = 'published'
        )
    WHERE id = v_package_id;

    UPDATE packagers
    SET
        avg_rating    = COALESCE((
            SELECT ROUND(AVG(overall_rating)::NUMERIC, 2)
            FROM reviews WHERE packager_id = v_packager_id AND status = 'published'
        ), 0),
        total_reviews = (
            SELECT COUNT(*) FROM reviews
            WHERE packager_id = v_packager_id AND status = 'published'
        )
    WHERE id = v_packager_id;

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_recalculate_ratings
    AFTER INSERT OR UPDATE OF status OR DELETE ON reviews
    FOR EACH ROW EXECUTE FUNCTION fn_recalculate_ratings();


-- -----------------------------------------------------------------------------
-- fn_generate_booking_reference — auto-generates TRP-2026-00001 on INSERT
-- -----------------------------------------------------------------------------
CREATE SEQUENCE IF NOT EXISTS booking_ref_seq START 1;

CREATE OR REPLACE FUNCTION fn_generate_booking_reference()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.booking_reference IS NULL OR NEW.booking_reference = '' THEN
        NEW.booking_reference :=
            'TRP-' || TO_CHAR(NOW(), 'YYYY') || '-' ||
            LPAD(NEXTVAL('booking_ref_seq')::TEXT, 5, '0');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_booking_reference
    BEFORE INSERT ON bookings FOR EACH ROW
    EXECUTE FUNCTION fn_generate_booking_reference();


-- -----------------------------------------------------------------------------
-- fn_update_current_bookings — increments / decrements packages.current_bookings
-- Prevents overbooking at DB level without relying on application logic alone.
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_update_current_bookings()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' AND NEW.status = 'confirmed' THEN
        UPDATE packages
        SET current_bookings = current_bookings + (NEW.adult_count + NEW.child_count)
        WHERE id = NEW.package_id;

    ELSIF TG_OP = 'UPDATE' AND OLD.status <> 'confirmed' AND NEW.status = 'confirmed' THEN
        UPDATE packages
        SET current_bookings = current_bookings + (NEW.adult_count + NEW.child_count)
        WHERE id = NEW.package_id;

    ELSIF TG_OP = 'UPDATE' AND OLD.status = 'confirmed'
          AND NEW.status IN ('cancelled', 'refunded') THEN
        UPDATE packages
        SET current_bookings = current_bookings - (OLD.adult_count + OLD.child_count)
        WHERE id = NEW.package_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_current_bookings
    AFTER INSERT OR UPDATE OF status ON bookings FOR EACH ROW
    EXECUTE FUNCTION fn_update_current_bookings();


-- -----------------------------------------------------------------------------
-- fn_update_thread_last_message — keeps message_threads.last_message_at current
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_update_thread_last_message()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE message_threads SET last_message_at = NEW.sent_at WHERE id = NEW.thread_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_thread_last_message
    AFTER INSERT ON messages FOR EACH ROW
    EXECUTE FUNCTION fn_update_thread_last_message();


-- =============================================================================
-- SEED: DEFAULT ADMIN USER
-- Replace the password_hash placeholder with a real bcrypt hash before deploy.
-- =============================================================================
INSERT INTO users (full_name, email, password_hash, role, is_active, is_email_verified)
VALUES (
    'System Admin',
    'admin@traveltour.com',
    '$2a$12$PLACEHOLDER_REPLACE_WITH_BCRYPT_HASH',
    'admin',
    TRUE,
    TRUE
);

-- =============================================================================
-- END OF SCHEMA — v2
-- =============================================================================