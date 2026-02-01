#nullable enable
namespace TalentManagementAPI.Application.Events
{
    public sealed record EmployeeChangedEvent(Guid EmployeeId) : IDomainEvent;

}
