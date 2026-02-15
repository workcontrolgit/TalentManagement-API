namespace TalentManagementAPI.WebApi.Controllers.v2
{
    [ApiVersion("2.0")]
    public class PositionsController : BaseApiController
    {
        [HttpGet]
        [Authorize(Policy = AuthorizationConsts.ApiReadPolicy)]
        public async Task<IActionResult> Get([FromQuery] GetPositionsQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = AuthorizationConsts.ApiReadPolicy)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetPositionByIdQuery { Id = id }));
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationConsts.ApiWritePolicy)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(CreatePositionCommand command)
        {
            var resp = await Mediator.Send(command);
            return CreatedAtAction(nameof(Post), resp);
        }

        [HttpPost]
        [Route("AddMock")]
        [Authorize(Policy = AuthorizationConsts.ApiWritePolicy)]
        public async Task<IActionResult> AddMock(InsertMockPositionCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationConsts.ApiWritePolicy)]
        public async Task<IActionResult> Put(Guid id, UpdatePositionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationConsts.ApiAdminPolicy)]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeletePositionByIdCommand { Id = id }));
        }
    }
}
