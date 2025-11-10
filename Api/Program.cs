using Api;
using Api.Endpoints;
using Api.Middleware;
using Core;
using Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

// Configurar o Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicação");
    
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new ExpressionTemplate(
            // Include trace and span ids when present.
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
            theme: TemplateTheme.Code)));
    
    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        // Política para desenvolvimento
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

        // Política para produção (mais restritiva)
        options.AddPolicy("ProductionCors", policy =>
        {
            policy.WithOrigins() //COLOCAR O DOMINIO AQUI
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });


    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    builder.Services.AddApi(builder.Configuration);
    builder.Services.AddCore();
    builder.Services.AddInfrastructure();

    var app = builder.Build();
    
    // Adicionar logging de requisições
    app.UseSerilogRequestLogging();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "My API V1"); });

        app.MapScalarApiReference();
        app.UseDeveloperExceptionPage();
        app.UseCors("DevelopmentCors");

    }
    else
    {
        app.UseCors("ProductionCors");
        app.UseHttpsRedirection();
    }
    
    // Adicionar middleware de logging para webhooks
    app.UseMiddleware<WebhookLoggingMiddleware>();
    
    ClerkWebhookEndpoints.Map(app);
    ClientEndpoints.Map(app);

    Log.Information("Aplicação configurada com sucesso, iniciando servidor");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar a aplicação");
}
finally
{
    Log.CloseAndFlush();
}