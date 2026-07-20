using Scalar.AspNetCore;
using Serilog;
using Trackwise.API.Extensions;
using Trackwise.Infrastructure;
using Trackwise.Application;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
    
    builder.Services.AddControllers();
    builder.Services.AddAuthorization();
    builder.Services.AddOpenApi();
    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer"));

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    else
    {
        app.UseHsts();
    }
    
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "trackwise API a échoué au démarrage");
}
finally
{
    Log.CloseAndFlush();
}