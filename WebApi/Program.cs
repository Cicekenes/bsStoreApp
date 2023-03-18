using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using Presentation.ActionFilters;
using Services;
using Services.Contracts;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

builder.Services.AddControllers(config =>
{
    //Xml,json,csv kabul edilebilþr
    config.RespectBrowserAcceptHeader = true;
    //Bir istek geldiðinde kabul edip etmedðimizi bildirmemiz gerekiyor.Kabul etmiyorsak 406 kodu dönülür
    config.ReturnHttpNotAcceptable = true;
    //
    config.CacheProfiles.Add("5mins", new CacheProfile() { Duration = 300 });
})
    .AddXmlDataContractSerializerFormatters()
    .AddCustomCsvFormatter()
    //xml format kabul ettiðimiz belirttik
    .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly)
 .AddNewtonsoftJson(opt =>
 {
     opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
 });



builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    //Modelfilter dýþýna çýkýnca otomatik 400 kodu üretir.402 hata kodu üretmemiz adýna bu özelliði bastýrmýþ oluruz
    options.SuppressModelStateInvalidFilter = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureLoggerService();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.ConfigureActionFilters();
builder.Services.ConfigureCors();
builder.Services.ConfigureDataShapper();
builder.Services.AddCustomMediaTypes();
builder.Services.AddScoped<IBookLinks, BookLinks>();
builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeader();
builder.Services.AddMemoryCache();
builder.Services.ConfigureRateLimitingOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.RegisterRepositories();
builder.Services.RegisterServices();
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerService>();

app.ConfigureExceptionHandler(logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(s =>
    {
        s.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Api Project v1");
        s.SwaggerEndpoint("/swagger/v2/swagger.json", "Web Api Project v2");
    });
}

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseIpRateLimiting();
app.UseCors("CorsPolicy");
//Microsoft cors'tan sonra caching ifadesinin çaðrýlmasýný tavsiye eder.
app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
