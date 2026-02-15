namespace TalentManagementAPI.WebApi.Controllers.v2
{
    [ApiVersion("2.0")]
    public class DepartmentsController : BaseApiController
    {
        [HttpGet]
        [Authorize(Policy = AuthorizationConsts.ApiReadPolicy)]
        public async Task<IActionResult> Get([FromQuery] GetDepartmentsQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = AuthorizationConsts.ApiReadPolicy)]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetDepartmentByIdQuery { Id = id }));
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationConsts.ApiWritePolicy)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(CreateDepartmentCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Value }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationConsts.ApiWritePolicy)]
        public async Task<IActionResult> Put(Guid id, UpdateDepartmentCommand command)
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
            return Ok(await Mediator.Send(new DeleteDepartmentByIdCommand { Id = id }));
        }
    }
}
