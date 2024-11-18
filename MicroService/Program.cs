WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

WebApplication app = builder.Build();

string chorus;
using (StreamReader sr = File.OpenText("./song.txt"))
{
    chorus = sr.ReadToEnd();
}

app.UsePathBase(new PathString("/api/"));
app.MapGet("/definitely-not-rickroll", () => Results.Ok(chorus));

app.Run();