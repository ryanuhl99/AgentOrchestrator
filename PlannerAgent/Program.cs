using Common.Models;
using Common.Utils;
using Services.Clients;
using Services;
using Services.Clients.Agents;
using Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    client.BaseAddress = new Uri("http://research-agent");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<CodeAgentClient>(client =>
{
    client.BaseAddress = new Uri("http://code-agent");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ReviewAgentClient>(client =>
{
    client.BaseAddress = new Uri("http://review-agent");
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