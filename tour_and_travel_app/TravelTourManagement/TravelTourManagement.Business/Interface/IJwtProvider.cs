using System;
using System.Collections.Generic;

namespace TravelTourManagement.Business.Providers;

public interface IJwtProvider
{
    string GenerateToken(Guid userId, string email, string role, bool isEmailVerified);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
