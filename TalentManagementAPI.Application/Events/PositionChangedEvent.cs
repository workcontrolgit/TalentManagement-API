#nullable enable
namespace TalentManagementAPI.Application.Events
{
    public sealed record PositionChangedEvent(Guid PositionId) : IDomainEvent;

}
