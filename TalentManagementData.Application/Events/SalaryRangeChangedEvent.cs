#nullable enable
namespace TalentManagementData.Application.Events
{
    public sealed record SalaryRangeChangedEvent(Guid SalaryRangeId) : IDomainEvent;

}
