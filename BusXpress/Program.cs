using BusManagement.Hubs;
using BusManagement.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
// Register HttpContextAccessor for URL generation (needed for secure downloads)
builder.Services.AddHttpContextAccessor();


// 1) Services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(5);
    opts.Cookie.HttpOnly = true;
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "Admin");
    options.Conventions.AuthorizeFolder("/Clients", "Client");
    options.Conventions.AuthorizeFolder("/Drivers", "Driver");
    options.Conventions.AuthorizePage("/LicenseDownload", "LicenseAccess");
});

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", opts =>
    {
        opts.LoginPath = "/Login";
        opts.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization(cfg =>
{
    cfg.AddPolicy("Admin", p => p.RequireRole("Admin"));
    cfg.AddPolicy("Client", p => p.RequireRole("Client"));
    cfg.AddPolicy("Driver", p => p.RequireRole("Driver"));
    // Add a policy for license photo access (can be used to restrict download page)
    cfg.AddPolicy("LicenseAccess", policy =>
        policy.RequireRole("Admin", "Driver"));
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<AdminDashboardPublisher>();

var app = builder.Build();

// 2) Middleware
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 3) Endpoint mapping
app.MapRazorPages();
app.MapHub<DashboardHub>("/hubs/dashboard");

// 4) Fallback
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/Login");
    return Task.CompletedTask;
});

app.Run();
