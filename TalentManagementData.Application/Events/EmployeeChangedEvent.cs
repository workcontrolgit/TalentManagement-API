#nullable enable
namespace TalentManagementData.Application.Events
{
    public sealed record EmployeeChangedEvent(Guid EmployeeId) : IDomainEvent;

}
