#nullable enable
namespace TalentManagementAPI.Application.Events
{
    public sealed record SalaryRangeChangedEvent(Guid SalaryRangeId) : IDomainEvent;

}
