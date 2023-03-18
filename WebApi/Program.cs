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
    //Xml,json,csv kabul edilebil�r
    config.RespectBrowserAcceptHeader = true;
    //Bir istek geldi�inde kabul edip etmed�imizi bildirmemiz gerekiyor.Kabul etmiyorsak 406 kodu d�n�l�r
    config.ReturnHttpNotAcceptable = true;
    //
    config.CacheProfiles.Add("5mins", new CacheProfile() { Duration = 300 });
})
    .AddXmlDataContractSerializerFormatters()
    .AddCustomCsvFormatter()
    //xml format kabul etti�imiz belirttik
    .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly)
 .AddNewtonsoftJson(opt =>
 {
     opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
 });



builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    //Modelfilter d���na ��k�nca otomatik 400 kodu �retir.402 hata kodu �retmemiz ad�na bu �zelli�i bast�rm�� oluruz
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
//Microsoft cors'tan sonra caching ifadesinin �a�r�lmas�n� tavsiye eder.
app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
