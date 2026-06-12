using fk_news_detector.Infrastructure;
using fk_news_detector.Services;
using fk_news_detector.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Cassandra.ISession>(_ =>
    CassandraSessionFactory.Create(builder.Configuration));
builder.Services.AddSingleton<INewsExtractionService, NewsExtractionService>();
builder.Services.AddHttpClient("ML");
builder.Services.AddScoped<IDetectionService, DetectionService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IArticleService, ArticleService>();

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