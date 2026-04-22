using Microsoft.Extensions.AI;

namespace TalentManagementAPI.Infrastructure.Tests.Services
{
    public class OllamaAiServiceTests
    {
        [Fact]
        public async Task ChatAsync_WithSystemPrompt_SendsCorrectMessagesAndReturnsReply()
        {
            IList<ChatMessage>? capturedMessages = null;

            var client = new Mock<IChatClient>();
            client
                .Setup(x => x.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                    (msgs, _, _) => capturedMessages = msgs.ToList())
                .ReturnsAsync(new ChatResponse(new ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, "Hello world")));

            var service = new OllamaAiService(client.Object);

            var reply = await service.ChatAsync("Say hi", "You are concise.");

            reply.Should().Be("Hello world");
            capturedMessages.Should().NotBeNull();
            capturedMessages!.Should().HaveCount(2);
            capturedMessages[0].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.System);
            capturedMessages[0].Text.Should().Be("You are concise.");
            capturedMessages[1].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.User);
            capturedMessages[1].Text.Should().Be("Say hi");
        }

        [Fact]
        public async Task ChatAsync_WithoutSystemPrompt_SendsOnlyUserMessage()
        {
            IList<ChatMessage>? capturedMessages = null;

            var client = new Mock<IChatClient>();
            client
                .Setup(x => x.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                    (msgs, _, _) => capturedMessages = msgs.ToList())
                .ReturnsAsync(new ChatResponse(new ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, string.Empty)));

            var service = new OllamaAiService(client.Object);

            var reply = await service.ChatAsync("Say hi");

            reply.Should().BeEmpty();
            capturedMessages.Should().HaveCount(1);
            capturedMessages![0].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.User);
        }
    }
}
