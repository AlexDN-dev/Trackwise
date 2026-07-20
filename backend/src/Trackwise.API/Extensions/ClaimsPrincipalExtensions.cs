using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Trackwise.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Claim 'sub' absent ou invalide.");
        }
        return userId;
    }
}