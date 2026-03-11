using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Modules;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ----- Infraestrutura compartilhada -----
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();

// ----- Registro de módulos -----

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

// ----- Endpoints dos módulos -----

app.Run();
