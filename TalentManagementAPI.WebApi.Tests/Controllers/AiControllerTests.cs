namespace TalentManagementAPI.WebApi.Tests.Controllers
{
    public class AiControllerTests
    {
        private readonly Mock<IAiChatService> _aiChatServiceMock = new();
        private readonly Mock<IFeatureManagerSnapshot> _featureManagerMock = new();
        private readonly Mock<IAiResponseMetadata> _aiMetadataMock = new();
        private readonly AiController _controller;

        public AiControllerTests()
        {
            _controller = new AiController(
                _aiChatServiceMock.Object,
                _featureManagerMock.Object,
                _aiMetadataMock.Object);
        }

        [Fact]
        public async Task Chat_AiDisabled_ReturnsServiceUnavailableProblemDetails()
        {
            _featureManagerMock
                .Setup(m => m.IsEnabledAsync("AiEnabled"))
                .ReturnsAsync(false);

            var result = await _controller.Chat(new AiChatRequest("hello"), CancellationToken.None);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

            var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Title.Should().Be("AI chat is disabled");
            problem.Detail.Should().Be("AI chat is disabled. Enable FeatureManagement:AiEnabled to use this endpoint.");

            _aiChatServiceMock.Verify(
                m => m.ChatAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Chat_AiEnabled_ReturnsOkWithReply()
        {
            _featureManagerMock
                .Setup(m => m.IsEnabledAsync("AiEnabled"))
                .ReturnsAsync(true);
            _aiChatServiceMock
                .Setup(m => m.ChatAsync("hello", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync("hi");

            var result = await _controller.Chat(new AiChatRequest("hello"), CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new AiChatResponse("hi"));
        }
    }
}
