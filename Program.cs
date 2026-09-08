using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using BaknusITCare.Data;
using BaknusITCare.Services;
using BaknusITCare.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Web API Controllers & CORS for Flutter / Mobile integration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Forwarded Headers for Proxy & Container Support
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Ensure Data directory exists for SQLite fallback
Directory.CreateDirectory("Data");

// Configure Database Connection (SQLite primary for fast, reliable standalone execution)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=db,1433;Database=BaknusITCareDb;User Id=sa;Password=BaknusSuperPassword123!;TrustServerCertificate=True;";

string? useSqliteVal = builder.Configuration["UseSqlite"];
bool useSqlite = string.IsNullOrEmpty(useSqliteVal) || string.Equals(useSqliteVal, "true", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlite)
    {
        options.UseSqlite("Data Source=Data/BaknusITCare.db");
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null));
    }
});

// Add Microsoft Fluent UI Components
builder.Services.AddFluentUIComponents();

// Register Core Domain & Integration Services
builder.Services.AddScoped<IMailcowAuthService, MailcowAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<UserSessionService>();

var app = builder.Build();

app.UseForwardedHeaders();

// 2. Initialize Database & Seed Default Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    bool initialized = false;
    int maxRetries = 10;

    while (maxRetries > 0 && !initialized)
    {
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            await DbInitializer.InitializeAsync(dbContext);
            logger.LogInformation("Database BaknusITCare berhasil diinisialisasi.");
            initialized = true;
        }
        catch (Exception ex)
        {
            maxRetries--;
            if (maxRetries > 0)
            {
                logger.LogWarning("Menunggu database SQL Server siap... Sisa percobaan: {Retries}. Pesan: {Msg}", maxRetries, ex.Message);
                await Task.Delay(5000);
            }
            else
            {
                logger.LogError(ex, "Gagal menginisialisasi database utama setelah beberapa kali percobaan.");
            }
        }
    }
}

// 3. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
