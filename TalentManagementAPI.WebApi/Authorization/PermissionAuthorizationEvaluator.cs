using System.Security.Claims;

namespace TalentManagementAPI.WebApi.Authorization
{
    public static class PermissionAuthorizationEvaluator
    {
        public static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
        {
            return roles.Any(user.IsInRole);
        }

        public static bool HasPermission(ClaimsPrincipal user, string requiredPermission)
        {
            var scopeClaims = user.FindAll("scp").Select(c => c.Value)
                .Concat(user.FindAll("scope").Select(c => c.Value));

            var scopes = scopeClaims
                .SelectMany(value => value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var rolePermissions = user.FindAll("roles")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return scopes.Contains(requiredPermission) || rolePermissions.Contains(requiredPermission);
        }
    }
}
