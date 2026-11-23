using FoodPantryManagement.Data;
using FoodPantryManagement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string databasePath = Path.Combine(builder.Environment.ContentRootPath, "foodpantry.db");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PantryDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));
builder.Services.AddHttpClient<OpenMeteoService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(8);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("FoodPantryManagement/1.0");
});

var app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    PantryDbContext db = scope.ServiceProvider.GetRequiredService<PantryDbContext>();
    await PantrySeed.InitializeAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
