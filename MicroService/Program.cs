WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5002");

WebApplication app = builder.Build();

string chorus;
using (StreamReader sr = File.OpenText("./chorus.txt"))
{
    chorus = sr.ReadToEnd();
}

app.UsePathBase(new PathString("/api/"));
app.MapGet("/definitely-not-rickroll", () => Results.Ok(chorus));

app.Run();