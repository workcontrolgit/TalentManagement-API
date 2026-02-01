#nullable enable
using TalentManagementAPI.Application.Common.Caching;
using TalentManagementAPI.Application.Interfaces.Caching;

namespace TalentManagementAPI.Application.Events.Handlers
{
    public sealed class CacheInvalidationEventHandler :
        IDomainEventHandler<EmployeeChangedEvent>,
        IDomainEventHandler<DepartmentChangedEvent>,
        IDomainEventHandler<PositionChangedEvent>,
        IDomainEventHandler<SalaryRangeChangedEvent>
    {
        private readonly ICacheInvalidationService _cacheInvalidationService;

        public CacheInvalidationEventHandler(ICacheInvalidationService cacheInvalidationService)
        {
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task HandleAsync(EmployeeChangedEvent domainEvent, CancellationToken ct = default)
        {
            await _cacheInvalidationService.InvalidatePrefixAsync(CacheKeyPrefixes.EmployeesAll, ct);
            await _cacheInvalidationService.InvalidateKeyAsync(CacheKeyPrefixes.DashboardMetrics, ct);
        }

        public Task HandleAsync(DepartmentChangedEvent domainEvent, CancellationToken ct = default)
            => _cacheInvalidationService.InvalidateKeyAsync(CacheKeyPrefixes.DashboardMetrics, ct);

        public async Task HandleAsync(PositionChangedEvent domainEvent, CancellationToken ct = default)
        {
            await _cacheInvalidationService.InvalidatePrefixAsync(CacheKeyPrefixes.PositionsAll, ct);
            await _cacheInvalidationService.InvalidateKeyAsync(CacheKeyPrefixes.DashboardMetrics, ct);
        }

        public Task HandleAsync(SalaryRangeChangedEvent domainEvent, CancellationToken ct = default)
            => _cacheInvalidationService.InvalidateKeyAsync(CacheKeyPrefixes.DashboardMetrics, ct);
    }

}
