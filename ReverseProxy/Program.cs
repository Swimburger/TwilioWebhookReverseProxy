var builder = WebApplication.CreateBuilder(args);

var useYarp = builder.Configuration.GetValue<bool>("UseYarp");

if (useYarp)
{
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
}
else
{
    builder.Services.AddControllers();
}

var app = builder.Build();

if (useYarp)
{
    app.MapReverseProxy();
}
else
{
    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}

app.Run();