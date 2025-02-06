using System.Text.Json.Serialization;
using FrameUp.ProcessService.Api.Configuration;
using FrameUp.ProcessService.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Configuration.GetSection("Settings").Get<Settings>()!;
builder.Services.AddSingleton(settings);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.AddLogBee(settings)
    .AddCustomHealthChecks(settings);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options
            .SwaggerDoc("v1", new OpenApiInfo { Title = "FrameUp.ProcessService.Api", Version = "v1" });
    });

// Add services to the container.
builder.Services
    .AddMassTransit(settings)
    .AddRepositories()
    .AddDatabaseContext(settings)
    .AddServices()
    .AddMinIO(settings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseLogBee();
app.UseCustomHealthChecks();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// We should get "Program" class from the main project, but it automatically references Microsoft.AspNetCore.Mvc.Testing.Program class
// https://stackoverflow.com/questions/55131379/integration-testing-asp-net-core-with-net-framework-cant-find-deps-json
public partial class Program
{
}