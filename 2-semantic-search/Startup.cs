using ChatBot.Services;
using Microsoft.Extensions.AI;
using Pinecone;

namespace ChatBot;

static class Startup
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var openAiKey = Utils.RequireEnv("OPENAI_API_KEY");
        var pineConeKey = Utils.RequireEnv("PINECONE_API_KEY");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendCors", policy =>
                policy
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                );
        });

        builder.Services.AddSingleton<VectorSearchService>();

        builder.Services.AddSingleton<StringEmbeddingGenerator>( s=> new OpenAI.Embeddings.EmbeddingClient(            
            model: "text-embedding-3-small",
            apiKey: openAiKey
        ).AsIEmbeddingGenerator());

        builder.Services.AddSingleton<IndexClient>(s => new PineconeClient(pineConeKey).Index("wikipedia-landmarks"));

        builder.Services.AddSingleton<WikipediaClient>();

        builder.Services.AddSingleton<IndexBuilder>();

        builder.Services.AddSingleton<DocumentStore>();
    }
}
