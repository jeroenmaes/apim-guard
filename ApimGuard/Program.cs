using ApimGuard.Models;
using ApimGuard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Azure settings
builder.Services.Configure<AzureConfiguration>(
    builder.Configuration.GetSection("Azure"));

// Register Graph API service
builder.Services.AddScoped<IGraphApiService, GraphApiService>();

// Register API Management service
builder.Services.AddScoped<IApiManagementService, ApiManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
