#nullable enable
using Microsoft.AspNetCore.Authorization;
using TalentManagementAPI.Application.Features.Dashboard.Queries.GetDashboardMetrics;

namespace TalentManagementAPI.WebApi.Controllers.v2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/dashboard")]
    public sealed class DashboardController : BaseApiController
    {
        [HttpGet("metrics")]
        [Authorize(Policy = AuthorizationConsts.ApiReadPolicy)]
        [ProducesResponseType(typeof(Result<DashboardMetricsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMetrics()
            => Ok(await Mediator.Send(new GetDashboardMetricsQuery()));
    }
}
