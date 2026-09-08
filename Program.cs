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

    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.InitializeAsync(dbContext);
        logger.LogInformation("Database BaknusITCare berhasil diinisialisasi.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Koneksi database utama gagal, mencoba fallback SQLite...");
        try
        {
            var sqliteOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            sqliteOptionsBuilder.UseSqlite("Data Source=Data/BaknusITCare.db");
            using var sqliteContext = new ApplicationDbContext(sqliteOptionsBuilder.Options);
            await DbInitializer.InitializeAsync(sqliteContext);
            logger.LogInformation("Database SQLite fallback berhasil diinisialisasi.");
        }
        catch (Exception sqliteEx)
        {
            logger.LogError(sqliteEx, "Gagal menginisialisasi database fallback.");
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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
