# Security Review

## Scope
Review of current API authentication and authorization approach in `TalentManagementAPI.WebApi`, including policy setup, token validation configuration, and controller policy usage.

## Findings and Recommendations

### 1. Secrets committed in source configuration (High)
Observed:
- `MailSettings:SmtpPass` exists in `TalentManagementAPI.WebApi/appsettings.json`.
- `JWTSettings:Key` exists in `TalentManagementAPI.WebApi/appsettings.json`.

Risk:
- Credential leakage from repository history and downstream clones.

Recommendation:
- Remove secrets from committed config.
- Use environment variables, user-secrets for local dev, and a secret manager (for example Azure Key Vault) in deployed environments.
- Rotate exposed secrets immediately.

### 2. Write operations often require authentication only (High)
Observed:
- Multiple mutating endpoints use `[Authorize]` without role/scope policy checks.
- Deletes are admin-restricted, but create/update often are not.

Risk:
- Any authenticated principal can modify data if token is valid for issuer/audience.

Recommendation:
- Apply explicit authorization policies per operation capability.
- Suggested model:
  - read endpoints -> `ApiReadPolicy`
  - create/update endpoints -> `ApiWritePolicy`
  - delete/cache-admin endpoints -> `ApiAdminPolicy`

### 3. Global security bypass via feature flag (High operational risk)
Observed:
- `FeatureManagement:AuthEnabled` drives whether policies are strict or permissive.
- When disabled, default and named policies are configured to always succeed.

Risk:
- Misconfiguration can disable authorization broadly.

Recommendation:
- In non-development environments, fail startup if `AuthEnabled` is false.
- Restrict who can change feature flags and audit changes.

### 4. Role configuration is not validated on startup (Medium)
Observed:
- Role names are loaded from `ApiRoles` and used directly in policy registration.

Risk:
- Missing/empty role config can produce weak or broken authorization behavior.

Recommendation:
- Validate `ApiRoles:EmployeeRole`, `ApiRoles:ManagerRole`, and `ApiRoles:AdminRole` at startup and throw if missing.

### 5. Defined policies not consistently applied (Medium)
Observed:
- `ManagerPolicy` and `EmployeePolicy` are registered but not used by current controllers.

Risk:
- Security model exists in configuration but is not enforced at endpoint level.

Recommendation:
- Align controller attributes to policy intent and operation sensitivity.
- Replace generic `[Authorize]` on mutating endpoints with explicit policies.

### 6. Issuer matching can be brittle if STS issuer differs from authority URL (Low)
Observed:
- `ValidIssuer` defaults to `Sts:ServerUrl` when `Sts:ValidIssuer` is not set.

Risk:
- Tokens from valid STS can fail if `iss` format differs (for example slash/path differences).

Recommendation:
- Set `Sts:ValidIssuer` explicitly per environment.
- Consider `ValidIssuers` when multiple trusted issuer formats are required.

## Strengths in Current Implementation
- JWT validation enforces issuer, audience, lifetime, and signing key checks.
- STS authority and audience are required; app fails startup when missing.
- Admin policy is applied to destructive operations such as delete and cache management.

## Prioritized Action Plan
1. Remove and rotate exposed secrets.
2. Enforce capability/role policies for all mutating endpoints.
3. Prevent `AuthEnabled=false` in non-development.
4. Add startup validation for required role configuration.
5. Introduce scope-based policies for client credentials (`read`/`write`/`admin`).
6. Set explicit issuer configuration per environment.
