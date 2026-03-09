using PlannerAgent.Common.Models;
using PlannerAgent.Common.Utils;
using PlannerAgent.Services.Clients;
using PlannerAgent.Services;
using PlannerAgent.Services.Clients.Agents;
using PlannerAgent.Logic;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Logging.ClearProviders();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}
else
{
    builder.Logging.AddConsole();
}

builder.Services.AddScoped<LlmService>();
builder.Services.AddScoped<PlannerLogic>();
builder.Services.AddScoped<IAgent, ResearchAgentClient>();
builder.Services.AddScoped<IAgent, CodeAgentClient>();
builder.Services.AddScoped<IAgent, ReviewAgentClient>();
builder.Services.AddScoped<AgentResolver>();
builder.Services.AddScoped<PlannerService>();

builder.Services.AddHttpClient<LlmClient>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ResearchAgentClient>(client =>
{
    client.BaseAddress = new Uri("http://ResearchAgent:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<CodeAgentClient>(client =>
{
    client.BaseAddress = new Uri("http://CodeAgent:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ReviewAgentClient>(client =>
{
    client.BaseAddress = new Uri("http://ReviewAgent:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddControllers();
//builder.Configuration.AddJsonFile();

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();