using MonorailCss.Discovery;
using MonorailCss.WatchDemo.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// One line — Discovery force-loads every referenced non-BCL assembly (including
// Pennington.UI) and walks each one's IL for utility classes. Source watching, hot reload,
// and wwwroot/app.css pickup are all on by default.
builder.Services.AddMonorailCss();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseMonorailCss();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
