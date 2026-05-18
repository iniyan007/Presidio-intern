select * from members;
select * from book;
select * from borrow;
select * from fine_payments;
select * from book_copies;

CREATE TABLE category (
    id    SERIAL PRIMARY KEY,
    name  VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE membership_type (
    id                SERIAL PRIMARY KEY,
    type              INT  NOT NULL UNIQUE,  
    max_borrowings    INT  NOT NULL,
    max_borrow_days   INT  NOT NULL
);

INSERT INTO membership_type (type, max_borrowings, max_borrow_days) VALUES
    (0, 2,  7), 
    (1, 3, 10),  
    (2, 5, 15);  

CREATE TABLE members (
    id                  SERIAL PRIMARY KEY,
    name                VARCHAR(150) NOT NULL,
    phone               VARCHAR(15)  NOT NULL UNIQUE,
    email               VARCHAR(150) NOT NULL UNIQUE,
    membership_type_id  INT  NOT NULL REFERENCES membership_type(id),
    status              INT  NOT NULL DEFAULT 0,
    joined_date         DATE NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE book (
    id          SERIAL PRIMARY KEY,
    isbn        VARCHAR(20)  NOT NULL UNIQUE,
    title       VARCHAR(255) NOT NULL,
    author      VARCHAR(150) NOT NULL,
    category_id INT NOT NULL REFERENCES category(id)
);
CREATE TABLE book_copies (
    id       SERIAL PRIMARY KEY,
    book_id  INT          NOT NULL REFERENCES book(id),
    status   INT          NOT NULL DEFAULT 0,
    remarks  VARCHAR(300)
);

CREATE TABLE borrow (
    id              SERIAL PRIMARY KEY,
    member_id       INT            NOT NULL REFERENCES members(id),
    book_copy_id    INT            NOT NULL REFERENCES book_copies(id),
    date_of_borrow  DATE           NOT NULL DEFAULT CURRENT_DATE,
    due_date        DATE           NOT NULL,
    date_of_return  DATE,
    fine_amount     NUMERIC(10,2)  NOT NULL DEFAULT 0,
    status          INT            NOT NULL DEFAULT 0
);

CREATE TABLE fine_payments (
    id            SERIAL PRIMARY KEY,
    borrow_id     INT            NOT NULL REFERENCES borrow(id),
    member_id     INT            NOT NULL REFERENCES members(id),
    amount_paid   NUMERIC(10,2)  NOT NULL,
    payment_date  TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION calculate_member_fine(p_member_id INT)
RETURNS NUMERIC AS $$
DECLARE
    total_fine  NUMERIC(10,2);
    total_paid  NUMERIC(10,2);
BEGIN
    SELECT COALESCE(SUM(fine_amount), 0)
    INTO total_fine
    FROM borrow
    WHERE member_id = p_member_id;

    SELECT COALESCE(SUM(amount_paid), 0)
    INTO total_paid
    FROM fine_payments
    WHERE member_id = p_member_id;

    RETURN total_fine - total_paid;
END;
$$ LANGUAGE plpgsql;

INSERT INTO category (name) VALUES
    ('Fiction'),
    ('Science'),
    ('Technology'),
    ('History'),
    ('Self Help');

INSERT INTO membership_type (type, max_borrowings, max_borrow_days) VALUES
    (0, 2,  7),   -- Basic
    (1, 3, 10),   -- Student
    (2, 5, 15)    -- Premium
ON CONFLICT (type) DO NOTHING;

INSERT INTO book (isbn, title, author, category_id) VALUES
    ('978-0-06-112008-4', 'To Kill a Mockingbird',       'Harper Lee',         1),  
    ('978-0-7432-7356-5', 'The Great Gatsby',            'F. Scott Fitzgerald', 1),  
    ('978-0-14-028329-7', 'Of Mice and Men',             'John Steinbeck',      1),  
    ('978-0-06-093546-9', 'A Brief History of Time',     'Stephen Hawking',     2), 
    ('978-0-393-31753-8', 'The Selfish Gene',            'Richard Dawkins',     2), 
    ('978-0-13-468599-1', 'Clean Code',                  'Robert C. Martin',    3),  
    ('978-0-201-63361-0', 'The Pragmatic Programmer',    'Andrew Hunt',         3),  
    ('978-0-06-196436-0', 'Sapiens',                     'Yuval Noah Harari',   4),  
    ('978-0-14-303943-3', 'The Diary of a Young Girl',   'Anne Frank',          4),  
    ('978-1-59327-584-6', 'Atomic Habits',               'James Clear',         5),  
    ('978-0-06-251858-0', 'The 7 Habits',                'Stephen Covey',       5);  

INSERT INTO book_copies (book_id, status, remarks) VALUES

    (1, 0, 'Good condition'),
    (1, 0, 'Good condition'),
    (1, 2, 'Cover damaged'), 
    (2, 0, 'Good condition'),
    (2, 0, 'New copy'),
    (3, 0, 'Good condition'),
    (4, 0, 'Good condition'),
    (4, 0, 'Good condition'),
    (5, 0, 'Good condition'),
    (6, 0, 'Good condition'),
    (6, 0, 'New copy'),
    (6, 0, 'Good condition'),
    (7, 0, 'Good condition'),
    (7, 0, 'Good condition'),
    (8, 0, 'Good condition'),
    (8, 0, 'Good condition'),
    (9, 0, 'Good condition'),
    (10, 0, 'Good condition'),
    (10, 0, 'New copy'),
    (11, 0, 'Good condition'),
    (11, 3, 'Lost by member');

INSERT INTO members (name, phone, email, membership_type_id, status, joined_date) VALUES
    ('Iniyan',    '9876543210', 'iniyan@gmail.com',   1, 0, '2024-01-15'), 
    ('Priya',     '9876543211', 'priya@gmail.com',    2, 0, '2024-02-20'), 
    ('Karthik',   '9876543212', 'karthik@gmail.com',  3, 0, '2024-03-10'),  
    ('Divya',     '9876543213', 'divya@gmail.com',    1, 1, '2024-04-05'),  
    ('Rahul',     '9876543214', 'rahul@gmail.com',    2, 0, '2024-05-12'); 

INSERT INTO borrow (member_id, book_copy_id, date_of_borrow, due_date, date_of_return, fine_amount, status) VALUES
    (1, 1, '2026-04-01', '2026-04-08', NULL, 0, 0), 

    (2, 9, '2026-05-10', '2026-05-20', NULL, 0, 0),

    (3, 15, '2026-04-20', '2026-05-05', '2026-05-10', 50, 1); 

UPDATE book_copies SET status = 1 WHERE id = 1;   
UPDATE book_copies SET status = 1 WHERE id = 9;  





