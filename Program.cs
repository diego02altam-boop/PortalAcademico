using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;        // ApplicationDbContext + SeedData
using PortalAcademico.Services;    // ICursoCacheService, CursoCacheService

var builder = WebApplication.CreateBuilder(args);

// ================== Services ==================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=portal.db"; // fallback local

// DbContext (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity + Roles
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // para pruebas locales
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Cache distribuida: Redis si hay cadena; si no, memoria en proceso (fallback)
var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "PortalAcademico:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache(); // fallback
}

// Sesión (usa la cache registrada arriba)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Para leer Session en vistas (_Layout)
builder.Services.AddHttpContextAccessor();

// Servicio de cache del catálogo
builder.Services.AddScoped<ICursoCacheService, CursoCacheService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ============== Seeding (migrar BD + datos iniciales) ==============
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    // Asegura 3 cursos + rol y usuario Coordinador
    await SeedData.InitializeAsync(sp);
}

// ================== Pipeline ==================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cursos}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();
