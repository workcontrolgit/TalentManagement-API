#nullable enable
namespace TalentManagementData.Application.Events
{
    public sealed record PositionChangedEvent(Guid PositionId) : IDomainEvent;

}
