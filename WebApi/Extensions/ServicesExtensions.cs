using AspNetCoreRateLimit;
using Entities.DataTransferObjects;
using Entities.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.ActionFilters;
using Presentation.Controllers;
using Repositories.Contracts;
using Repositories.EFCore;
using Services;
using Services.Contracts;
using System.Text;

namespace WebApi.Extensions
{
    public static class ServicesExtensions
    {
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>

            services.AddDbContext<RepositoryContext>(options => options.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, ServiceManager>();

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerService, LoggerManager>();

        public static void ConfigureActionFilters(this IServiceCollection services)
        {
            services.AddScoped<ValidationFilterAttribute>();
            services.AddSingleton<LogFilterAttribute>();
            services.AddScoped<ValidateMediaTypeAttribute>();

        }

        public static void ConfigureCors(this IServiceCollection services)
        {
            //Api'ye erişim sağlanabilmesi amaçlanır
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("X-Pagination"));
            });
        }

        public static void ConfigureDataShapper(this IServiceCollection services)
        {
            services.AddScoped<IDataShaper<BookDto>, DataShaper<BookDto>>();
        }

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var systemTextJsonOutputFormatter = config.OutputFormatters.OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault();

                if (systemTextJsonOutputFormatter is not null)
                {
                    systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.btkakademi.hateoas+json");

                    systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.btkakademi.apiroot+json");

                }

                var xmlOutputFormatter = config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();

                if (xmlOutputFormatter is not null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.btkakademi.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.btkakademi.apiroot+xml");
                }
            });
        }

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                //Response header bölümüne versiyon bilgisini ekliyoruz.
                options.ReportApiVersions = true;

                //Kullanıcı bir versiyon talep etmez ise default bir versiyonlama olsun.
                options.AssumeDefaultVersionWhenUnspecified = true;
                //
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
                options.Conventions.Controller<BooksController>().HasApiVersion(new ApiVersion(1, 0));
                options.Conventions.Controller<BooksV2Controller>().HasDeprecatedApiVersion(new ApiVersion(2, 0));
            });
        }
        public static void ConfigureResponseCaching(this IServiceCollection services) =>
            services.AddResponseCaching();

        public static void ConfigureHttpCacheHeader(this IServiceCollection services) =>
            services.AddHttpCacheHeaders(expirationOpt =>
            {
                expirationOpt.MaxAge = 90;
                expirationOpt.CacheLocation = CacheLocation.Private;
            }, validationOpt =>
            {
                //Yeniden validate etme zorunluluğu olmasın
                validationOpt.MustRevalidate = false;
            }
            );
        public static void ConfigureRateLimitingOptions(this IServiceCollection services)
        {
            var rateLimitRules = new List<RateLimitRule>()
            {
                new RateLimitRule()
                {
                    //tüm isteklerde 1 dakikada 3 request-limit olsun
                    Endpoint = "*",
                    Limit=60,
                    Period="1m"
                }
            };

            services.Configure<IpRateLimitOptions>(options =>
            {
                //Genel kural olarak ekliyoruz.
                options.GeneralRules = rateLimitRules;
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore,MemoryCacheIpPolicyStore>();   
            services.AddSingleton<IRateLimitConfiguration,RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy,AsyncKeyLockProcessingStrategy>();

        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentity<User, IdentityRole>(options =>
            {
                //Şifre oluştururken rakam istiyor muyuz?
                options.Password.RequireDigit = true;
                //küçük harf içersin mi?
                options.Password.RequireLowercase = false;
                //büyük harf içersin mi?
                options.Password.RequireUppercase = false;
                //özel karakter istiyor muyuz?
                options.Password.RequireNonAlphanumeric = false;
                //şifre uzunluğu
                options.Password.RequiredLength = 6;
                //Bir mail bir kere kullanılsın
                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders();//jwt için
        }

        public static void ConfigureJwt(this IServiceCollection services,IConfiguration configuration) 
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["secretKey"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["validIssuer"],
                    ValidAudience = jwtSettings["validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Web Api Project",
                    Version = "v1",
                    Description = "ASP.NET Core Web API",
                    TermsOfService = new Uri("https://www.btkakademi.gov.tr/"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Enes Cicek",
                        Email = "bmenescicek@hotmail.com"
                    }
                
                }) ;
                s.SwaggerDoc("v2",new OpenApiInfo() { Title="Web Api Project",Version="v2"});

                //Token' ile çalışacağımız için yapılacak konfigürasyon
                s.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme()
                {
                    In=ParameterLocation.Header,
                    Description="Place to add JWT with Bearer",
                    Name="Authorization",
                    Type=SecuritySchemeType.ApiKey,
                    Scheme="Bearer"
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            },
                            Name="Bearer"
                        },
                        new List<string>()
                    }
                });
            });

        }
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            
        }
         public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IBookService, BookManager>();
            services.AddScoped<ICategoryService, CategoryManager>();
            services.AddScoped<IAuthenticationService, AuthenticationManager>();
            
        }

    }

}