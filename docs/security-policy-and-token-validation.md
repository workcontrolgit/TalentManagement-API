# Security Policy and Token Validation

## Overview
This document describes how authorization policies are configured in `TalentManagementAPI.WebApi`, and how JWT access tokens are validated against the configured Security Token Service (STS).

## Feature Flag Gate
- `FeatureManagement:AuthEnabled` controls whether authentication and authorization are actively enforced.
- When enabled, JWT authentication is registered and the default authorization policy requires an authenticated user.
- When disabled, authorization policies are configured to succeed for local development and troubleshooting.

## Authorization Policies
Policy names are defined in `TalentManagementAPI.WebApi/Extensions/AuthorizationConsts.cs`:
- `AdminPolicy`
- `ManagerPolicy`
- `EmployeePolicy`

Role values are loaded from configuration:
- `ApiRoles:AdminRole`
- `ApiRoles:ManagerRole`
- `ApiRoles:EmployeeRole`

Policy behavior (when `AuthEnabled=true`):
- `AdminPolicy`: requires admin role.
- `ManagerPolicy`: requires manager or admin role.
- `EmployeePolicy`: requires employee, manager, or admin role.

Controller endpoints apply either:
- `[Authorize]` for authenticated access, or
- `[Authorize(Policy = AuthorizationConsts.AdminPolicy)]` for admin-restricted operations.

## Role Claim Mapping From Access Token
After JWT validation succeeds, ASP.NET Core creates `HttpContext.User` from token claims.
Role-based policies use `RequireRole(...)`, which checks the user's role claims and matches role names.

In this API, role names are loaded from config and wired to policies in `Program.cs`:

```csharp
var adminRole = builder.Configuration["ApiRoles:AdminRole"];       // "HRAdmin"
var managerRole = builder.Configuration["ApiRoles:ManagerRole"];   // "Manager"
var employeeRole = builder.Configuration["ApiRoles:EmployeeRole"]; // "Employee"

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationConsts.AdminPolicy,
        policy => policy.RequireRole(adminRole));

    options.AddPolicy(AuthorizationConsts.ManagerPolicy,
        policy => policy.RequireRole(managerRole, adminRole));

    options.AddPolicy(AuthorizationConsts.EmployeePolicy,
        policy => policy.RequireRole(employeeRole, managerRole, adminRole));
});
```

Example controller usage:

```csharp
[HttpDelete("{id}")]
[Authorize(Policy = AuthorizationConsts.AdminPolicy)]
public async Task<IActionResult> Delete(Guid id) { ... }
```

If the token contains a matching role claim (for example `HRAdmin`), access is granted. Otherwise, authorization fails with `403 Forbidden`.

### Example Access Token Claims
Example payload from STS:

```json
{
  "sub": "9c8d7e6f-1234-4567-89ab-0f1e2d3c4b5a",
  "name": "Jane Doe",
  "role": ["Manager"],
  "iss": "https://localhost:44310",
  "aud": "app.api.talentmanagement",
  "exp": 1735689600
}
```

With this token:
- `[Authorize]` passes (authenticated user).
- `EmployeePolicy` passes (`Manager` is included in allowed roles).
- `ManagerPolicy` passes (`Manager` is allowed).
- `AdminPolicy` fails (token does not include `HRAdmin`).

## Default Authorization Requirement
`AuthEnabledRequirement` is the default policy requirement:
- If `AuthEnabled=false`: requirement succeeds immediately.
- If `AuthEnabled=true`: requirement succeeds only when `User.Identity.IsAuthenticated == true`.

This provides a single global gate for protected endpoints while still allowing anonymous endpoints explicitly marked with `[AllowAnonymous]`.

## STS Token Validation
JWT bearer authentication is configured in `AddJWTAuthentication` with:
- `Sts:ServerUrl` as `Authority`
- `Sts:Audience` as `Audience`

### How `Sts:ServerUrl` and `Sts:Audience` Are Read
Configuration source (`appsettings.json`):

```json
"Sts": {
  "ServerUrl": "https://localhost:44310",
  "Audience": "app.api.talentmanagement"
}
```

Read and applied in `AddJWTAuthentication`:

```csharp
var authority = configuration["Sts:ServerUrl"];
var audience = configuration["Sts:Audience"];

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
    });
```

What each value means:
- `Authority` (`Sts:ServerUrl`): trusted STS/issuer endpoint used by JWT bearer middleware to load issuer metadata and signing keys.
- `Audience` (`Sts:Audience`): required `aud` claim that identifies this API as the intended token recipient.

Example: token accepted for audience check

```json
{
  "iss": "https://localhost:44310",
  "aud": "app.api.talentmanagement"
}
```

Example: token rejected for audience mismatch

```json
{
  "iss": "https://localhost:44310",
  "aud": "some.other.api"
}
```

Validation settings:
- `ValidateIssuer = true`
- `ValidIssuer = Sts:ValidIssuer` when provided, otherwise `Sts:ServerUrl`
- `ValidateAudience = true`
- `ValidAudience = Sts:Audience`
- `ValidateIssuerSigningKey = true`
- `ValidateLifetime = true`
- `ClockSkew = 2 minutes`

### Result
Incoming bearer tokens are accepted only when they are:
- issued by the configured STS issuer,
- intended for the configured API audience,
- signed with a trusted key from the authority metadata,
- and not expired (within allowed clock skew).

## Request Pipeline Order
The middleware pipeline enforces security in this order:
1. `UseAuthentication()`
2. `UseAuthorization()`
3. Controller endpoint execution

Without a valid token and required role/policy, protected endpoints return `401 Unauthorized` or `403 Forbidden` as appropriate.
