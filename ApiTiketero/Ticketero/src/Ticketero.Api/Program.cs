using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using Serilog;
using Ticketero.Api.Middleware;
using Ticketero.Application.Extensions;
using Ticketero.Infrastructure.Data;
using Ticketero.Infrastructure.Extensions;

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ticketero-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ticketero API", Version = "v1", Description = "API del sistema de turnos Ticketero" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
        });

        var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
        foreach (var xmlFile in xmlFiles)
        {
            c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
        }
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<Ticketero.Api.Services.IPdfExportService, Ticketero.Api.Services.PdfExportService>();

    // Distributed cache: Redis en producción, Memory en desarrollo
    if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Redis")))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
        });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
    }

    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key no está configurada. " +
            "Configure la variable de entorno Jwt__Key con una clave de al menos 32 caracteres. " +
            "Ejemplo para desarrollo: Jwt__Key=TicketeroDevKey2026!@#$%^&*()MinLength32");
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = 429;
        options.AddFixedWindowLimiter("Api", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("PermitirFrontend", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
            if (origins is { Length: > 0 })
            {
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        });
    });

    if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Redis")))
    {
        builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis")!,
            options => { options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("Ticketero"); });
    }
    else
    {
        builder.Services.AddSignalR();
    }

    var app = builder.Build();

    var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "multimedia");
    Directory.CreateDirectory(uploadsPath);

    // Middleware pipeline (error handling FIRST)
    app.UseErrorHandling();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("PermitirFrontend");
    app.UseRateLimiter();

    app.UseStaticFiles();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<Ticketero.Api.Hubs.TicketHub>("/ticketHub");

    // Health checks: liveness (simple) vs readiness (incluye BD)
    app.MapHealthChecks("/healthz");
    app.MapHealthChecks("/ready");

    // Seed condicional: solo si SEED_DATABASE=true o en Development
    if (app.Configuration.GetValue<bool>("SeedDatabase"))
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketeroDbContext>();
            var defaultPassword = app.Configuration["Seed:DefaultPassword"];
            await DatabaseSeeder.SeedAsync(context, defaultPassword);
        }
    }

    Log.Information("Ticketero API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}