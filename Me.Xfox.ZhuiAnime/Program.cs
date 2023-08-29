global using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization;
using Me.Xfox.ZhuiAnime;
using Me.Xfox.ZhuiAnime.Modules;
using Me.Xfox.ZhuiAnime.Utils;
using Me.Xfox.ZhuiAnime.Utils.Toml;
using Microsoft.AspNetCore.Mvc.Formatters;
using Serilog;
using Modules = Me.Xfox.ZhuiAnime.Modules;
using ZhuiAnime = Me.Xfox.ZhuiAnime;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.Configuration.ReplaceJsonWithToml();
builder.Configuration.AddTomlFile("appsettings.Local.toml", true);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
);

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddControllers(options =>
    {
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
        options.OutputFormatters.RemoveType<StreamOutputFormatter>();
        options.Filters.Add<ZhuiAnimeError.ErrorExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy();
        options.JsonSerializerOptions.PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy();
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            throw new ZhuiAnimeError.BadRequest(context.ModelState);
    });

builder.Services.AddDbContext<ZAContext>(opt =>
{
    var connectionString = builder.Configuration.GetConnectionString(nameof(ZAContext)) ??
        throw new Exception("Connection string for ZAContext cannot be null");
    opt.UseNpgsql(connectionString);
    opt.UseSnakeCaseNamingConvention();
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "ZhuiAni.me API",
    });
    options.OperationFilter<ZhuiAnimeError.ErrorResponsesOperationFilter>();
    options.SchemaFilter<RequiredNotNullableSchemaFilter>();
    options.SupportNonNullableReferenceTypes();
    options.IncludeXmlComments(System.IO.Path.Combine(
        AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IModule))))
{
    Log.Logger.Information("Loading module {@Module}", type.FullName);
    type.GetMethod("ConfigureOn", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        ?.Invoke(null, new object[] { builder });
}
ZhuiAnime.Services.TurnstileService.ConfigureOn(builder);
ZhuiAnime.Services.TokenService.ConfigureOn(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(c => c.RouteTemplate = "/api/swagger/{documentName}/swagger.json");
    app.UseReDoc(c =>
    {
        c.RoutePrefix = "api/swagger";
        c.SpecUrl = "/api/swagger/v1/swagger.json";
    });
}
else
{
    app.UseHsts();
}
app.UseForwardedHeaders();

app.UseSentryTracing();
app.UseSerilogRequestLogging();

if (app.Environment.IsProduction()) app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");

app.MapFallbackToController("/api/{**path}",
    nameof(ZhuiAnime.Controllers.InternalController.EndpointNotFound),
    nameof(ZhuiAnime.Controllers.InternalController).Replace("Controller", ""));
app.MapFallbackToFile("index.html");

await app.RunAsync();
