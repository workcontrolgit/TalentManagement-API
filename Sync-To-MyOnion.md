# Sync-To-MyOnion

## Purpose
Track delta between recent fixes in:

- Source: `C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API` (commit `c3a485d`)
- Target: `C:\apps\devkit\ApiResources\MyOnion\MyOnion` (current includes commit `d2b33eb`)

## Source Delta (TalentManagement-API `c3a485d`)
Files changed in source commit:
1. `TalentManagementAPI.Application/Mappings/GeneralProfile.cs`
2. `TalentManagementAPI.Application/Features/Positions/Commands/UpdatePosition/UpdatePositionCommand.cs`
3. `TalentManagementAPI.Application.Tests/Mappings/GeneralProfileCreateMappingsTests.cs`
4. `TalentManagementAPI.Application.Tests/Positions/UpdatePositionCommandHandlerTests.cs`
5. `TalentManagementAPI.WebApi/Extensions/ServiceExtensions.cs`
6. `TalentManagementAPI.WebApi/Middlewares/ErrorHandlerMiddleware.cs`
7. `TalentManagementAPI.WebApi/appsettings.json`
8. `TalentManagementAPI.WebApi/appsettings.Development.json`

## Sync Status in MyOnion

### Already Synced
1. `MyOnion/src/MyOnion.Application/Mappings/GeneralProfile.cs`
- Employee create mapping now uses explicit `MapWith(...)` construction (PersonName + core fields).

2. `MyOnion/tests/MyOnion.Application.Tests/Mappings/GeneralProfileCreateEmployeeMappingTests.cs`
- Added regression test for `CreateEmployeeCommand -> Employee`.

### Not Yet Synced (Pending)
1. `MyOnion/src/MyOnion.Application/Features/Positions/Commands/UpdatePosition/UpdatePositionCommand.cs`
- Still only updates `PositionTitle` and `PositionDescription`.
- Missing updates for:
  - `PositionNumber`
  - `DepartmentId`
  - `SalaryRangeId`

2. `MyOnion/tests/MyOnion.Application.Tests/Positions/UpdatePositionCommandHandlerTests.cs`
- Does not assert the missing update fields above.

3. Broader create mapping hardening not yet synced
- `CreatePositionCommand -> Position` still partial map.
- `CreateDepartmentCommand -> Department` still partial map.
- `CreateSalaryRangeCommand -> SalaryRange` still implicit map.
- Source repo switched these to explicit `MapWith(...)`.

4. Web API diagnostics/security troubleshooting deltas not yet synced
- `MyOnion/src/MyOnion.WebApi/Extensions/ServiceExtensions.cs`
  - No temporary JWT diagnostics events (`OnAuthenticationFailed`, `OnChallenge`, `OnTokenValidated`)
  - No `IncludeErrorDetails = true`
  - No dev-only `Sts:IgnoreCertificateErrors` backchannel switch
- `MyOnion/src/MyOnion.WebApi/Middlewares/ErrorHandlerMiddleware.cs`
  - Still logs `error.Message` only (no full exception object)
- `MyOnion/src/MyOnion.WebApi/appsettings.json`
  - `Sts` block still uses `https://localhost:44310`
- `MyOnion/src/MyOnion.WebApi/appsettings.Development.json`
  - No `Sts:IgnoreCertificateErrors` flag

## MyOnion.sln Impact
For code/test compilation, **no `.sln` edit is required** for the new mapping test file because the test project is SDK-style and includes `*.cs` automatically.

If you want the new test file explicitly visible in Solution Items (not required for build), that would be a separate optional `.sln` edit.

## Recommended Next Sync Order
1. Sync `UpdatePositionCommand` field update fix + related test assertions.
2. Sync remaining create mapping hardening in `GeneralProfile`.
3. Sync WebApi troubleshooting deltas only if needed for local STS/JWKS debugging.
4. Keep temporary JWT diagnostics behind development-only conditions and remove after issue closure.

## Validation Suggestion After Sync
Run:

```powershell
dotnet test C:\apps\devkit\ApiResources\MyOnion\MyOnion\tests\MyOnion.Application.Tests\MyOnion.Application.Tests.csproj -c Debug
```

And if WebApi deltas are synced:

```powershell
dotnet run --project C:\apps\devkit\ApiResources\MyOnion\MyOnion\src\MyOnion.WebApi\MyOnion.WebApi.csproj
```
