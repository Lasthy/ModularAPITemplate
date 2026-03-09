using ModularAPITemplate.Modules.Produtos;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Modules;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ----- Infraestrutura compartilhada -----
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();

// ----- Registro de módulos -----
builder.Services.AddModule<ProdutosModule>(builder.Configuration);

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
        app.MapScalarApiReference();
    }
}

app.UseHttpsRedirection();

// ----- Endpoints dos módulos -----
app.MapModuleEndpoints<ProdutosModule>();

app.Run();
