# Security Policy for Client Credential Tokens

## Goal
Support machine-to-machine callers (client credentials flow) where tokens may not contain user roles, while still enforcing least privilege by API permission.

## Current Gap
Current policies are role-based (`Employee`, `Manager`, `HRAdmin`). That works for user tokens, but client credential tokens can be issued without user identity and may not carry those business roles.

## Recommended Model
Use a mixed authorization model:
- User tokens: continue to support role-based policies.
- Client credential tokens: enforce API permissions via scopes/permissions such as:
  - `app.api.talentmanagement.read`
  - `app.api.talentmanagement.write`
  - optionally `app.api.talentmanagement.admin`

## Token Claim Strategy
Depending on STS, permissions may appear in:
- `scp` (space-separated scopes, common for delegated tokens),
- `scope` (some custom STS),
- `roles` (application permissions/app roles in some providers).

Policy checks should support all three claim shapes for compatibility.

## Example Policy Design
Define explicit policies by capability:
- `ApiReadPolicy`
- `ApiWritePolicy`
- `ApiAdminPolicy`

Example registration in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    // Existing user-role policies can remain
    options.AddPolicy("ApiReadPolicy", policy =>
        policy.RequireAssertion(ctx =>
            HasPermission(ctx.User, "app.api.talentmanagement.read") ||
            HasAnyRole(ctx.User, "Employee", "Manager", "HRAdmin")));

    options.AddPolicy("ApiWritePolicy", policy =>
        policy.RequireAssertion(ctx =>
            HasPermission(ctx.User, "app.api.talentmanagement.write") ||
            HasAnyRole(ctx.User, "Manager", "HRAdmin")));

    options.AddPolicy("ApiAdminPolicy", policy =>
        policy.RequireAssertion(ctx =>
            HasPermission(ctx.User, "app.api.talentmanagement.admin") ||
            HasAnyRole(ctx.User, "HRAdmin")));
});
```

Example helpers:

```csharp
static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
{
    return roles.Any(user.IsInRole);
}

static bool HasPermission(ClaimsPrincipal user, string required)
{
    var scopeClaims = user.FindAll("scp").Select(c => c.Value)
        .Concat(user.FindAll("scope").Select(c => c.Value));

    var scopes = scopeClaims
        .SelectMany(v => v.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var roleClaims = user.FindAll("roles").Select(c => c.Value)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return scopes.Contains(required) || roleClaims.Contains(required);
}
```

## Controller Usage
Apply capability policies instead of only role policies:

```csharp
[HttpGet]
[Authorize(Policy = "ApiReadPolicy")]
public async Task<IActionResult> Get(...) { ... }

[HttpPost]
[Authorize(Policy = "ApiWritePolicy")]
public async Task<IActionResult> Post(...) { ... }

[HttpDelete("{id}")]
[Authorize(Policy = "ApiAdminPolicy")]
public async Task<IActionResult> Delete(Guid id) { ... }
```

## STS Configuration Guidance
Configure API permissions in STS:
1. Define API scopes:
   - `app.api.talentmanagement.read`
   - `app.api.talentmanagement.write`
2. Grant client app only the minimum required permission.
3. Ensure issued tokens include expected claim (`scp`, `scope`, or `roles`).

## Validation Hardening
In addition to current issuer/audience validation:
- keep `ValidateIssuer = true`
- keep `ValidateAudience = true`
- keep `ValidateLifetime = true`
- validate signature keys from authority metadata
- consider reducing `ClockSkew` if infrastructure time is synchronized

## Migration Plan
1. Add new capability policies (`ApiReadPolicy`, `ApiWritePolicy`, `ApiAdminPolicy`).
2. Update controllers to use capability policies by operation type.
3. Keep legacy role policies temporarily for backward compatibility.
4. Remove legacy role dependency once all callers use scope/permission-based tokens.

## Expected Result
User-driven requests and machine-to-machine requests are both supported:
- users can be authorized by role,
- service clients can be authorized by API permission without requiring user roles.
