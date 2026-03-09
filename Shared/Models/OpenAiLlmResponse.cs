
using System.Text.Json.Serialization;

namespace Shared.Models;

public class OpenAiLlmResponse
{
    public List<Choice> Choices { get; set; } = [];
    public Usage Usage { get; set; } = new();
}

public class Choice
{
    public Message Message { get; set; } = new();
}

public class Message
{
    public string Content { get; set; } = "";
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}