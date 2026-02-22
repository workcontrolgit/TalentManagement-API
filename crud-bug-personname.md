# CRUD Bug Fix: Employee `PersonName` Mapping

## Issue
`POST /api/v1/Employees` returned `500` with a runtime mapping error:

- source: `TalentManagementAPI.Application.Features.Employees.Commands.CreateEmployee.CreateEmployeeCommand`
- destination: `TalentManagementAPI.Domain.Entities.Employee`
- type: `Map`

The failure happened while compiling/executing the mapping expression for create employee.

## Root Cause
Employee creation relied on runtime mapping compilation for `CreateEmployeeCommand -> Employee`.  
`Employee.Name` is a value object (`PersonName`) and this path was susceptible to runtime mapping compile failures.

## Fix Implemented
Updated create mapping to explicit construction using `MapWith(...)` in:

- `TalentManagementAPI.Application/Mappings/GeneralProfile.cs`

### Before
```csharp
config.NewConfig<CreateEmployeeCommand, Employee>()
    .Map(dest => dest.Name, src => new PersonName(src.FirstName, src.MiddleName, src.LastName));
```

### After
```csharp
config.NewConfig<CreateEmployeeCommand, Employee>()
    .MapWith(src => new Employee
    {
        Name = new PersonName(src.FirstName, src.MiddleName, src.LastName),
        PositionId = src.PositionId,
        DepartmentId = src.DepartmentId,
        Salary = src.Salary,
        Birthday = src.Birthday,
        Email = src.Email,
        Gender = src.Gender,
        EmployeeNumber = src.EmployeeNumber,
        Prefix = src.Prefix,
        Phone = src.Phone
    });
```

This removes ambiguity in runtime mapping generation and ensures all fields required for create are explicitly mapped.

## Additional Hardening Done
1. Applied the same explicit mapping pattern to other create commands:
- `CreateDepartmentCommand -> Department`
- `CreatePositionCommand -> Position`
- `CreateSalaryRangeCommand -> SalaryRange`

2. Improved error diagnostics:
- `TalentManagementAPI.WebApi/Middlewares/ErrorHandlerMiddleware.cs` now logs full exception details:
  - from `_logger.LogError(error.Message);`
  - to `_logger.LogError(error, "Unhandled exception");`

## Regression Tests Added
Added mapping tests to prevent recurrence:

- `TalentManagementAPI.Application.Tests/Mappings/GeneralProfileCreateMappingsTests.cs`
  - verifies `CreateEmployeeCommand -> Employee` mapping including `PersonName`
  - verifies other create-command mappings

Also updated:

- `TalentManagementAPI.Application.Tests/Positions/UpdatePositionCommandHandlerTests.cs`
  - to validate recently fixed update fields (`PositionNumber`, `DepartmentId`, `SalaryRangeId`)

## Validation
Application tests were executed and passed:

- `TalentManagementAPI.Application.Tests`: Passed `50`, Failed `0`.
