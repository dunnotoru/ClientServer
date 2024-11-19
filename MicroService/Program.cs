WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

WebApplication app = builder.Build();

string chorus;
using (StreamReader sr = File.OpenText("./song.txt"))
{
    chorus = sr.ReadToEnd();
}

app.UseRouting();
app.MapGet("/api/definitely-not-rickroll", () => Results.Ok(chorus));

app.Run();