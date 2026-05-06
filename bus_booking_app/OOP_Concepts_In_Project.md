# OOPS in Project
## BUS BOOKING APP
**Name:** Iniyan 

### 1. OOPS Based Explanation:

#### I. Classes & Inheritance:

**A. The API Controllers (Inheritance Example)**
The most prominent example of inheritance in our project is how the system handles different HTTP requests using controllers.
*   **ControllerBase (The Base Class):** Think of this as the master template for any API endpoint in the system. Instead of duplicating common HTTP features, this base class holds all shared behaviors (like returning `Ok()`, `NotFound()`, and accessing user claims).
*   **AdminController (Inherits from ControllerBase):** Represents the endpoints for platform managers. It adds specific administrative details, such as gathering platform stats and approving operators.
*   **OperatorController (Inherits from ControllerBase):** Represents endpoints for bus vendors. It adds business-specific details like managing buses, assigning trips, and calculating operator revenue.

*(Note: In our data model, the `User` class currently uses a unified structure with a `Role` property. Applying inheritance to the `User` model (`AdminUser : User`, `OperatorUser : User`) is a planned improvement for future iterations.)*

**B. Bus and Route Management Classes**
These classes represent the physical vehicles and the journeys they take. They do not inherit from other domain classes; they stand on their own.
*   **Bus:** Represents an actual, physical bus owned by a bus operator.
    *   *What it holds:* The bus's name, registration number (`BusNumber`), total seating capacity, and which operator owns it.
*   **BusRoute:** Represents the geographical path a bus takes.
    *   *What it holds:* The starting city (`SourceCityId`), the destination (`DestinationCityId`), and the total distance.
*   **Trip:** Represents a specific planned journey.
    *   *What it holds:* The exact departure and arrival dates and times, the specific bus assigned, the route taken, the base ticket price, and the platform fee.

**C. Location Management Classes**
These classes organize the real-world map into data the system can understand.
*   **City:** Represents a major geographical location.
    *   *What it holds:* The name of the city, and navigation links to routes acting as a source or destination.

**D. Booking and Reservation Classes**
These classes handle everything that happens when a customer tries to buy a ticket.
*   **Booking:** Represents the main reservation record.
    *   *What it holds:* The total amount, the status of the booking (Pending, Confirmed), a temporary `LockedUntil` timestamp to prevent double booking, the person who booked it, and the specific trip.
*   **BookingSeat:** Represents the individual seat assigned.
    *   *What it holds:* The specific seat number, passenger gender, and status.
*   **Payment:** Represents the actual financial transaction for a booking.
    *   *What it holds:* The amount paid, the payment method used, the transaction ID, and whether the payment was successful or failed.

**E. System and Configuration Classes**
*   **Expense:** Represents costs incurred during a trip.
    *   *What it holds:* The description of the expense, the cost amount, and the associated trip.

#### II. Interfaces and Method-Level Usage:
The application uses interfaces as strict behavioral contracts to separate "what needs to be done" from "how it is actually done." This is primarily handled through Dependency Injection in the controllers.

*   **IEmailService:**
    *   *Usage in Methods:* The `AuthController` requires this interface in its constructor. When a user registers, the method simply calls `_emailService.SendEmailAsync(...)`. The method does not know or care if the alert is being sent via an actual SMTP server or just mocked into the console for testing.

#### III. Polymorphism (Dynamic Execution):
*   **Service-Level Polymorphism:** Because the application depends on interfaces rather than concrete classes, methods that require `IEmailService` can polymorphically execute entirely different code depending on the environment. At runtime, the system injects either `SmtpEmailService` (for production) or `ConsoleEmailService` (for development). The calling controller method never changes.

#### IV. Encapsulation (State Protection) & Abstraction:
Encapsulation is implemented throughout the application to protect the internal state of data from being altered in unauthorized ways.

*   **Dependency Protection:** Classes like `AdminController` keep their database dependencies secure by using `private readonly ApplicationDbContext _context`. This prevents external code from maliciously altering the database connection.
*   **Data Transfer Objects (DTOs):** The system uses classes like `RegisterRequest`, `LoginRequest`, and `CreateTripRequest`. When a user interacts with the API, they use these DTOs, abstracting away the complex underlying database entities (`User`, `Bus`, `Trip`). The user sees a clean, simplified data structure, completely shielded from the database schema.
*   **Database Abstraction (ApplicationDbContext):** The Entity Framework Core `ApplicationDbContext` abstracts the entire database. Instead of writing raw SQL queries, developers interact with simple C# collections (`DbSet<Booking>`). The complexity of translating these actions into secure SQL is completely hidden behind the context class.

---

### 2. Improvements Performed:

#### 1. Modularity:
Currently, portions of the application suffer from a lack of modularity. Distinct Data Transfer Objects (DTOs) are bundled together into single files. For example, `AuthDTOs.cs` acts as a container for `RegisterRequest`, `LoginRequest`, and `UpdateProfileRequest`.
While the computer reads this fine, it creates friction for developers. As the app grows, scrolling through hundreds of lines just to find specific properties becomes incredibly tedious.

**Original Code:**
```csharp
// File: backend/DTOs/AuthDTOs.cs
namespace backend.DTOs
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
    }
}


```

**After Changes:**
```csharp
// File 1 Name: backend/DTOs/RegisterRequest.cs
namespace backend.DTOs
{
    public class RegisterRequest { ... }
}

// File 2 Name: backend/DTOs/LoginRequest.cs
namespace backend.DTOs
{
    public class LoginRequest { ... }
}

// File 3 Name: backend/DTOs/UpdateProfileRequest.cs
namespace backend.DTOs
{
    public class UpdateProfileRequest { ... }
}
```

#### 2. Auditable Entity Interface:
Several classes (`User`, `Booking`, `Payment`, `Bus`, `Trip`, `City`) share identical properties, such as `CreatedAt`. Currently, the application treats these properties as completely unrelated, requiring duplicate logic.
By introducing a capability interface like `IAuditableEntity`, we can standardize this behavior. This interface acts as a contract. The system can then use polymorphism to treat all these different classes as one generic "Auditable" object, allowing the database context to automatically update the timestamps globally.

**BEFORE:**
```csharp
namespace backend.Models
{
    public class User
    {
        public int Id { get; set; }
        // Isolated audit field
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Booking
    {
        public int Id { get; set; }
        // Isolated duplicate logic
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

**AFTER:**
```csharp
// File: backend/Interfaces/IAuditableEntity.cs
namespace backend.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
    }
}

// Various Entity Files
using backend.Interfaces;
namespace backend.Models
{
    public class User : IAuditableEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Booking : IAuditableEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

#### 3. Payment Encapsulation:
Currently, classes like `Payment` use fully public properties (`public get; set;`). Any file in the application can instantly overwrite a payment's status or amount without permission, bypassing business rules and creating a risk of data corruption.
The solution is strict encapsulation. By changing the properties to `private set`, the object locks down its own data so it can only be read from the outside. We create a controlled method (`CompletePayment()`) to allow necessary updates safely.

**BEFORE:**
```csharp
// File: backend/Models/Payment.cs
namespace backend.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        // Anyone can change this amount anytime
        public decimal Amount { get; set; }
        // Anyone can change this status to anything
        public string Status { get; set; } = "Pending";
    }
}
```

**AFTER:**
```csharp
// File: backend/Models/Payment.cs
namespace backend.Models
{
    public class Payment
    {
        // Properties can be read publicly, but only changed from within
        public int Id { get; private set; }
        public int BookingId { get; private set; }
        public string TransactionId { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public string Status { get; private set; } = "Pending";

        public Payment(int bookingId, decimal amount)
        {
            BookingId = bookingId;
            Amount = amount;
        }

        // The ONLY way to mark successful
        public void CompletePayment(string transactionId)
        {
            if (Status == "Completed") 
            {
                 throw new InvalidOperationException("Payment is already completed.");
            }
            TransactionId = transactionId;
            Status = "Completed";
        }
    }
}
```

#### 4. Constructor Initialization (Guaranteeing Valid State):
Currently, the application allows objects to be created in a completely empty, invalid state. When a new entity is created, it relies entirely on the developer to manually set mandatory properties.
The OOP solution is Constructor Initialization. By adding a parameterized constructor, the application guarantees an object cannot exist in a "half-built" state. It forces mandatory data at creation, eliminating risks of corrupted data.

**BEFORE (Empty State Creation):**
```csharp
// File: backend/Models/Booking.cs
namespace backend.Models
{
    public class Booking
    {
        public int Id { get; set; }
        // Mandatory, but not enforced at creation
        public int TripId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
    }
}

// Usage elsewhere: (DANGER: Forgot to add UserId!)
var booking = new Booking();
booking.TripId = tripId;
booking.TotalAmount = 500.00m;
```

**AFTER (Constructor Initialization):**
```csharp
// File: backend/Models/Booking.cs
namespace backend.Models
{
    public class Booking
    {
        public int Id { get; private set; }
        public int TripId { get; private set; }
        public int UserId { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string Status { get; private set; } = "Pending";

        // Constructor forces all mandatory data upfront
        public Booking(int tripId, int userId, decimal totalAmount)
        {
            if (tripId <= 0) throw new ArgumentException("Trip required");
            if (userId <= 0) throw new ArgumentException("User required");
            if (totalAmount < 0) throw new ArgumentException("Amount cannot be negative");

            TripId = tripId;
            UserId = userId;
            TotalAmount = totalAmount;
        }
    }
}

// Code refuses to compile if mandatory data is missing:
var booking = new Booking(tripId, userId, 500.00m);
```

