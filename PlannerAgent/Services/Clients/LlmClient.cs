
namespace PlannerAgent.Services.Clients;

public class LlmClient(HttpClient client, IConfiguration config)
{
    private readonly HttpClient _client = client;
    private readonly string _apikey = config["OpenAi:ApiKey"]!;
}