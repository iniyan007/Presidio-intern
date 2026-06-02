# Travel & Tour Management Platform (Backend)

Welcome to the Travel & Tour Management Platform! This is a robust, enterprise-grade RESTful API built with **ASP.NET Core 8**, designed to serve as the backend for a comprehensive travel booking ecosystem. 

## 🏗 Architecture & Backend Design

The application is engineered using **Clean Architecture (N-Tier)** principles, ensuring a highly decoupled, maintainable, and testable codebase.

* **API Layer** (`TravelTourManagement.API`): The entry point. Handles HTTP requests, routing, JWT authentication, model validation, global exception handling, and dependency injection registration.
* **Business Layer** (`TravelTourManagement.Business`): Contains all the core business logic, services (`IAuthService`, `IBookingService`, etc.), external providers (JWT, Email, OTP), and AutoMapper profiles.
* **Data Access Layer** (`TravelTourManagement.DataAccess`): Manages data persistence using **Entity Framework Core**. Implements a robust **Generic Repository Pattern** alongside specialized repositories to encapsulate all database interactions.

### Key Technical Patterns Used:
* **Repository Pattern**: A `GenericRepository` handles standard CRUD, while specialized repositories (e.g., `BookingRepository`) handle complex `Include` and domain-specific queries.
* **Dependency Injection (DI)**: Extensively used to decouple services and promote testability.
* **Cancellation Tokens**: Implemented pervasively across the entire stack (Controllers -> Services -> Repositories) to allow graceful connection termination and optimize resource usage if a client disconnects mid-request.
* **Background Jobs (Quartz.NET)**: Used for scheduling cron-like background tasks without blocking the main thread (e.g., automatically expiring unpaid bookings).
* **Distributed Caching**: Used for highly-performant, temporary data storage (OTPs, Reset Tokens) using `IDistributedCache` (MemoryCache or Redis).
* **AutoMapper**: Simplifies object-object mapping between Entities and Data Transfer Objects (DTOs).

---

## ✨ Features Implemented

### 1. Robust Authentication & Authorization
* **JWT Authentication**: Secure token generation using `HMAC-SHA256` signatures.
* **Role-Based Access Control (RBAC)**: Three distinct roles—`Admin`, `Packager`, and `Traveler`—enforced via `[Authorize(Roles = "...")]` attributes.
* **Refresh Tokens**: Long-lived refresh tokens stored securely in the database to acquire new short-lived access tokens seamlessly.
* **Email OTP Verification**: Registration requires email validation via a 6-digit OTP sent to the user's email, cached securely with a 10-minute expiration.
* **Highly Secure Two-Step Password Reset**: 
  1. User requests reset -> OTP sent to email (protects against email enumeration).
  2. User verifies OTP -> Receives a highly secure, 15-minute `ResetToken`.
  3. User submits `ResetToken` + New Password -> Password hashed via **BCrypt** and updated.
  4. Tokens are immediately and explicitly wiped from the cache upon successful reset to prevent race conditions.

### 2. Packager (Agency) Lifecycle
* **Application System**: Users can apply to become a "Packager" (`Status: Pending`).
* **Admin Moderation**: The `Admin` can review applications and Approve or Reject them. Once approved, the user gains the `Packager` role and can begin publishing packages.
* **Platform Configuration**: Admins manage global categories, tags, and settings used by Packagers to categorize their offerings.

### 3. Package Management
* **Dynamic Itineraries**: Packagers can create complex, multi-day itineraries for their packages.
* **Multipart Form Uploads**: Package creation supports uploading multiple high-quality media images directly via `multipart/form-data`, securely saving them to local storage.
* **Automated Seat Management**: Seat counts are strictly managed. When a booking is created, available seats are dynamically reduced (excluding Infants). 
* **Search & Discovery**: Rich search endpoints for Travelers to filter packages by destination, price, category, and travel dates.

### 4. Booking & Payment Ecosystem
* **Multi-Passenger Bookings**: A single booking supports multiple travelers categorized by Adults, Children, and Infants.
* **Real-time Seat Locking**: Booking immediately locks seats.
* **Payment Status Tracking**: Tracks `Unpaid`, `Partial`, and `Paid` states.
* **Automated Expirations (Quartz Job)**: A background worker continuously monitors `Pending` & `Unpaid` bookings. If a booking passes a configured time threshold without payment, the cron job cancels it and automatically restores the package's available seat count.
* **Traveler History**: Travelers can fetch their complete booking history (`/api/Bookings/my-bookings`), returning full details of their trips, passengers, and payment statuses.

### 5. Secure Travel Documents Management
* **Mandatory KYC**: Travelers must upload identity documents (e.g., Passport, Aadhar) associated with their bookings.
* **File Uploads**: Files are uploaded securely via `IFormFile`, sanitized, and stored locally with generated UUID filenames.
* **Document Verification Workflow**:
  * Packagers can review documents uploaded by travelers.
  * Packagers can mark documents as `Verified` or `Rejected` (providing a mandatory rejection reason).
  * Travelers can dynamically re-upload documents if rejected, restarting the verification flow.

### 6. Security & Attack Protection
* **Global Rate Limiting**: Implemented via ASP.NET Core native `Microsoft.AspNetCore.RateLimiting`. A strict global limit of 100 requests per minute per IP address protects the entire API from general DDoS attacks.
* **Auth Rate Limiting**: The `AuthController` (handling logins, registrations, and password resets) enforces an aggressively strict limit of 5 requests per minute to completely eliminate brute-force password cracking and email enumeration bots.
* **Duplicate Request Prevention (Idempotency)**: Custom Action Filters (`[IdempotentAttribute]`) intercept high-stakes operations (like `CreateBooking` and `CreatePackage`). By requiring clients to send a unique `X-Idempotency-Key` and validating it against a distributed cache, the backend safely catches and rejects accidental double-clicks (`409 Conflict`) without processing duplicate payments or records.

---

## 🛠 Tech Stack Details
* **Framework**: .NET 8 ASP.NET Core Web API
* **ORM**: Entity Framework Core (SQL Server)
* **Authentication**: JWT Bearer Authentication
* **Security**: BCrypt.Net-Next (Password Hashing)
* **Mapping**: AutoMapper
* **Scheduling**: Quartz.NET
* **Caching**: Microsoft.Extensions.Caching.Distributed
