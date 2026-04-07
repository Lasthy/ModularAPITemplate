using System.Text.Json;
using Cysharp.Serialization.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Modules;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ----- Infraestrutura compartilhada -----
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new UlidJsonConverter());
});

// ----- Registro de módulos -----
builder.Services.AddModules(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// ----- Pipeline HTTP -----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    var uiProvider = builder.Configuration.GetValue<string>("OpenApi:UI") ?? "Scalar";
    var tracker = app.Services.GetRequiredService<OpenApiModuleTracker>();

    if (uiProvider.Equals("Swagger", StringComparison.OrdinalIgnoreCase))
    {
        app.UseSwaggerUI(options =>
        {
            foreach (var name in tracker.DocumentNames)
                options.SwaggerEndpoint($"/openapi/{name}.json", name);
        });
    }
    else
    {
        app.MapScalarApiReference(options =>
        {
            options.Title = "ModularAPITemplate API Documentation";
            options.WithTheme(ScalarTheme.DeepSpace);
            options.AddDocuments(tracker.DocumentNames);
        });
    }
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (httpContext, report) =>
    {
        httpContext.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMilliseconds = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMilliseconds = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data,
                tags = entry.Value.Tags,
            }),
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload));
    },
});

// ----- Endpoints dos módulos -----
app.MapModuleEndpoints(builder.Configuration);

app.Run();
