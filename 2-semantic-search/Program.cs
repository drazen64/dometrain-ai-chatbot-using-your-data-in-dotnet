

using ChatBot;
using ChatBot.Services;

var builder = WebApplication.CreateBuilder(args);

Startup.ConfigureServices(builder);

var app = builder.Build();

//var indexer = app.Services.GetRequiredService<IndexBuilder>();
//await indexer.BuildDocumentIndex(SourceData.LandmarkNames);

//System.Console.WriteLine("Done");

app.UseCors("FrontendCors");

app.MapGet("/search", async (string query, VectorSearchService search) =>
{
    var results = await search.FindTopKArticles(query, 3);
    return Results.Ok(results);
});

app.Run();