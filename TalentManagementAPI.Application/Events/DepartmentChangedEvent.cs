#nullable enable
namespace TalentManagementAPI.Application.Events
{
    public sealed record DepartmentChangedEvent(Guid DepartmentId) : IDomainEvent;

}
