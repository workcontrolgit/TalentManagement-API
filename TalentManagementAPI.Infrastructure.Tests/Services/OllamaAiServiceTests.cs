using OllamaSharp.Models.Chat;
using System.Linq;

namespace TalentManagementAPI.Infrastructure.Tests.Services
{
    public class OllamaAiServiceTests
    {
        [Fact]
        public async Task ChatAsync_WithSystemPrompt_SendsPromptAndReturnsCombinedReply()
        {
            var client = new Mock<IOllamaApiClient>();
            client.SetupProperty(x => x.SelectedModel, "llama3.2");

            ChatRequest? capturedRequest = null;

            client
                .Setup(x => x.ChatAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
                .Returns((ChatRequest request, CancellationToken _) =>
                {
                    capturedRequest = request;
                    return StreamResponses(
                        new ChatResponseStream
                        {
                            Message = new Message(new ChatRole("assistant"), "Hello")
                        },
                        new ChatResponseStream
                        {
                            Message = new Message(new ChatRole("assistant"), " world")
                        });
                });

            var service = new OllamaAiService(client.Object);

            var reply = await service.ChatAsync("Say hi", "You are concise.");

            reply.Should().Be("Hello world");
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Model.Should().Be("llama3.2");
            capturedRequest.Messages.Should().NotBeNull();
            var messages = capturedRequest.Messages!.ToList();
            messages.Should().HaveCount(2);
            messages[0].Role.Should().Be(new ChatRole("system"));
            messages[0].Content.Should().Be("You are concise.");
            messages[1].Role.Should().Be(new ChatRole("user"));
            messages[1].Content.Should().Be("Say hi");
        }

        [Fact]
        public async Task ChatAsync_WithoutChunks_ReturnsEmptyString()
        {
            var client = new Mock<IOllamaApiClient>();
            client.SetupProperty(x => x.SelectedModel, "llama3.2");
            client
                .Setup(x => x.ChatAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
                .Returns((ChatRequest _, CancellationToken _) => StreamResponses());

            var service = new OllamaAiService(client.Object);

            var reply = await service.ChatAsync("Say hi");

            reply.Should().BeEmpty();
        }

        private static async IAsyncEnumerable<ChatResponseStream?> StreamResponses(params ChatResponseStream[] responses)
        {
            foreach (var response in responses)
            {
                yield return response;
                await Task.Yield();
            }
        }
    }
}
