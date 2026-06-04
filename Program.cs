using fk_news_detector.Services;
using fk_news_detector.Infrastructure;
using fk_news_detector.Repositories;
using fk_news_detector.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Cassandra Session (Singleton)
builder.Services.AddSingleton<Cassandra.ISession>(
    _ => CassandraSessionFactory.CreateSession(builder.Configuration));

// Repositories
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IDetectionResultRepository, DetectionResultRepository>();
builder.Services.AddScoped<IBlacklistedDomainRepository, BlacklistedDomainRepository>();

// Unit Of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddSingleton<INewsExtractionService, NewsExtractionService>();
builder.Services.AddHttpClient<IDetectionService, DetectionService>(
    client =>
    {
        client.BaseAddress = new Uri("http://127.0.0.1:8000");
    });

builder.Services.AddControllersWithViews();

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

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Cassandra connected.");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();