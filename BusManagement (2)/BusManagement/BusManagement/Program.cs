var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login"; // Redirect to Login page for unauthorized users
        options.AccessDeniedPath = "/AccessDenied"; // Optional for unauthorized access
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();


builder.Services.AddAuthorization(options =>
{
    // Define the 'Driver' role policy
    options.AddPolicy("Driver", policy =>
        policy.RequireRole("Driver"));

    // Define the 'Client' role policy
    options.AddPolicy("Client", policy =>
        policy.RequireRole("Client"));

    // Define the 'Admin' role policy
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
});




builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "Admin"); // Require authentication for all pages in /Clients
    options.Conventions.AuthorizeFolder("/Clients", "Client"); // Require authentication for all pages in /Clients
    options.Conventions.AuthorizeFolder("/Drivers", "Driver"); // Require authentication for all pages in /Clients
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();

    // Redirect root URL to Login page
    endpoints.MapGet("/", context =>
    {
        context.Response.Redirect("/Login");
        return Task.CompletedTask;
    });
});

app.Run();
