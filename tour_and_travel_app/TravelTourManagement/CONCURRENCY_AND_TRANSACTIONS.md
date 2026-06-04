# ACID Transactions & Concurrency Control in BookingService

This document explains the critical architectural improvements made to the `BookingService` to ensure enterprise-grade resilience, data consistency, and protection against high-traffic race conditions (overbooking).

---

## 1. The Problem: Data Inconsistency & Race Conditions

Before the fix, the `BookingService` suffered from two severe enterprise flaws:

### Flaw A: Broken Unit of Work
When a user created a booking, the system executed three distinct database operations:
1. `_bookingRepository.AddAsync(booking)`
2. `_seasonalPricingRepository.UpdateAsync(pricing)` (Reduce available seats)
3. `_packageRepository.UpdateAsync(package)` (Increase current bookings)

Because the `GenericRepository` automatically called `_context.SaveChangesAsync()` after every single operation, these changes were committed to the database independently. If the application crashed or the network dropped exactly after step 1 but before step 2, the booking was permanently saved to the database, but the seats were never deducted. **This resulted in permanently corrupted data.**

### Flaw B: The Overbooking Race Condition
Seat availability was checked using a standard C# `if` statement:
```csharp
if (package.CurrentBookings + requestedSeats > package.MaxCapacity) 
    throw new Exception("Full");
```
If two users executed this code at the exact same millisecond for the last remaining seat, they would *both* read the database, *both* see that 1 seat is available, and *both* successfully bypass the `if` statement. The package would be overbooked.

---

## 2. The Solution: Execution Strategies & Serializable Transactions

To fix both issues simultaneously, we implemented strict **Execution Strategies** and **Pessimistic Concurrency via Serializable Transactions**.

### Fix A: Guaranteeing ACID Compliance
We injected the `ApplicationDbContext` directly into the `BookingService` to seize control over the database transaction boundaries. 

We now wrap the entire business flow inside:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
```
Now, when the repositories save their changes, the data is only written to a temporary, isolated transaction scope in PostgreSQL. The changes are **not** made permanent until the very end of the method when we explicitly call `await transaction.CommitAsync()`. 
If any line of code throws an exception (e.g., failing to upload a passport, or failing to schedule the Quartz job), the `catch` block calls `await transaction.RollbackAsync()`, erasing all temporary changes and keeping the database perfectly clean.

### Fix B: Preventing Race Conditions
We configured the database transaction to use the strictest possible isolation level:
```csharp
BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
```
When using `Serializable`, PostgreSQL guarantees mathematically that concurrent transactions behave as if they were executed one after the other in strict sequence. 

**How the Overbooking is prevented:**
1. User A and User B attempt to book the last seat at the exact same millisecond.
2. PostgreSQL locks the `Package` row for User A.
3. User A successfully commits the transaction and takes the seat.
4. Because the row was modified during User B's transaction, PostgreSQL actively rejects User B's transaction and throws a specific Database Exception (`40001` - serialization_failure).
5. Our C# code specifically catches this `40001` error and gracefully returns a friendly message to User B: *"Due to high traffic, this package was just updated. Please try booking again."*

---

## 3. The Solution: Transient Database Resilience

Cloud databases (like AWS RDS or Azure Postgres) occasionally drop connections for brief milliseconds due to network shifts. Previously, this would crash the API request.

To fix this, we added `.EnableRetryOnFailure(3)` to the PostgreSQL configuration in `DataAccessServiceRegistration.cs`.

Because EF Core cannot automatically retry explicit transactions, we wrapped the transaction logic inside an **Execution Strategy**:
```csharp
var strategy = _context.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () => {
    // Transaction logic here
});
```
If the database connection blips, the Execution Strategy automatically rebuilds the transaction and retries the entire booking process up to 3 times invisibly, ensuring a flawless user experience.
