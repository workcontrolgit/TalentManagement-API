namespace TalentManagementAPI.Application.Events
{
    public interface IEventDispatcher
    {
        Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
            where TEvent : IDomainEvent;
    }

}
