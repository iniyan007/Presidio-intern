namespace TravelTourManagement.DataAccess.Enums;

public enum BookingStatus
{
    Pending,
    DocumentUnderReview,
    Confirmed,
    Cancelled,
    Completed,
    Refunded
}

public enum PaymentStatus
{
    Unpaid,
    Partial,
    Paid,
    Refunded,
    Failed
}   
