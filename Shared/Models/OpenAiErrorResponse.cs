
namespace Shared.Models;

public class OpenAiErrorResponse
{
    public OpenAiError Error { get; set; } = new();
}

public class OpenAiError
{
    public string Message { get; set; } = "";
    public string Type { get; set; } = "";
    public string Param { get; set; } = "";
    public string Code { get; set; } = "";
}