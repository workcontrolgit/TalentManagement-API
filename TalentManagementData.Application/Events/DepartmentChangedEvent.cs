#nullable enable
namespace TalentManagementData.Application.Events
{
    public sealed record DepartmentChangedEvent(Guid DepartmentId) : IDomainEvent;

}
