using fk_news_detector.Services;
using fk_news_detector.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Cassandra Session (Singleton) ──────────────────
builder.Services.AddSingleton<Cassandra.ISession>(
    _ => CassandraSessionFactory.CreateSession(builder.Configuration));
// ───────────────────────────────────────────────────

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<INewsExtractionService, NewsExtractionService>();

var app = builder.Build();

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