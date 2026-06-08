using System;

namespace TravelTourManagement.DataAccess.Interface;

public interface IUserContextService
{
    Guid? UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
