using Fool.CardGame.Web.Events.Hubs;
using Fool.CardGame.Web.Services;
using Fool.Core.Services;
using Fool.Core.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITableService, TableService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddHostedService<BackgroundGameService>();

builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.MapHub<GameHub>("/gameHub");
app.Run();
