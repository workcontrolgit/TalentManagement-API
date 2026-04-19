#nullable enable
using System.Diagnostics;
using System.Text.Json;
using TalentManagementAPI.Application.Common.Results;
using TalentManagementAPI.Application.Interfaces;
using TalentManagementAPI.Application.Messaging;

namespace TalentManagementAPI.Application.Features.AI.Queries.NlSearch
{
    public sealed class NlSearchQuery : IRequest<Result<NlEmployeeFilterDto>>
    {
        public string Query { get; init; } = string.Empty;

        public sealed class NlSearchQueryHandler
            : IRequestHandler<NlSearchQuery, Result<NlEmployeeFilterDto>>
        {
            private readonly IAiChatService _aiChatService;

            public NlSearchQueryHandler(IAiChatService aiChatService)
            {
                _aiChatService = aiChatService;
            }

            // Instructs the LLM to output structured JSON only.
            // Fields map 1:1 to GET /api/v1/employees query parameters.
            private const string SystemPrompt = """
                You are an employee search assistant. Convert the user's natural language query
                into a JSON object with exactly these fields:

                {
                  "firstName": "",
                  "lastName": "",
                  "email": "",
                  "employeeNumber": "",
                  "positionTitle": "",
                  "parsedExpression": ""
                }

                Rules:
                - Populate only fields mentioned or implied in the query; leave others as ""
                - All values are partial-match strings (e.g., "eng" matches "Engineering")
                - parsedExpression: a brief human-readable summary of what you extracted (e.g., "positionTitle contains 'engineer'")
                - Output ONLY the JSON object — no explanation, no markdown, no code fences
                """;

            public async Task<Result<NlEmployeeFilterDto>> Handle(
                NlSearchQuery request,
                CancellationToken cancellationToken)
            {
                var sw = Stopwatch.StartNew();

                string rawJson;
                try
                {
                    rawJson = await _aiChatService.ChatAsync(
                        request.Query,
                        SystemPrompt,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return Result<NlEmployeeFilterDto>.Failure($"AI service unavailable: {ex.Message}");
                }

                // Strip markdown code fences if the LLM wrapped the JSON (common with some models)
                var cleaned = StripCodeFences(rawJson);

                LlmFilterResponse? parsed;
                try
                {
                    parsed = JsonSerializer.Deserialize<LlmFilterResponse>(
                        cleaned,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    return Result<NlEmployeeFilterDto>.Failure(
                        $"AI returned non-JSON output. Raw response: {rawJson}");
                }

                if (parsed is null)
                    return Result<NlEmployeeFilterDto>.Failure("AI returned an empty response.");

                sw.Stop();

                return Result<NlEmployeeFilterDto>.Success(new NlEmployeeFilterDto
                {
                    OriginalQuery    = request.Query,
                    FirstName        = parsed.FirstName,
                    LastName         = parsed.LastName,
                    Email            = parsed.Email,
                    EmployeeNumber   = parsed.EmployeeNumber,
                    PositionTitle    = parsed.PositionTitle,
                    ParsedExpression = parsed.ParsedExpression,
                    ExecutionTimeMs  = sw.ElapsedMilliseconds,
                });
            }

            private static string StripCodeFences(string text)
            {
                var t = text.Trim();
                if (t.StartsWith("```"))
                {
                    var firstNewline = t.IndexOf('\n');
                    if (firstNewline >= 0) t = t[(firstNewline + 1)..];
                }
                if (t.EndsWith("```")) t = t[..^3];
                return t.Trim();
            }

            // Internal deserialization target — not exposed in the API response
            private sealed class LlmFilterResponse
            {
                public string FirstName        { get; set; } = string.Empty;
                public string LastName         { get; set; } = string.Empty;
                public string Email            { get; set; } = string.Empty;
                public string EmployeeNumber   { get; set; } = string.Empty;
                public string PositionTitle    { get; set; } = string.Empty;
                public string ParsedExpression { get; set; } = string.Empty;
            }
        }
    }
}
