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



        // Configure logging
        builder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));
        builder.Services.AddSingleton<ILoggerFactory>(sp =>
            LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            }));

        // Configure OpenAI Chat Client with logging and function invocation support
        builder.Services.AddSingleton<IChatClient>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var client = new OpenAI.Chat.ChatClient(
                "gpt-5-mini", 
                openAiKey).AsIChatClient();
            return new ChatClientBuilder(client)
                .UseLogging(loggerFactory)
                .UseFunctionInvocation(loggerFactory, c =>
                {
                    c.IncludeDetailedErrors = true;
                })
                .Build(sp);
        });

        builder.Services.AddTransient<ChatOptions>(s => new ChatOptions
        {

        });

        builder.Services.AddSingleton<RagQuestionService>();
        builder.Services.AddSingleton<PromptService>();

        builder.Services.AddSingleton<VectorSearchService>();

        builder.Services.AddSingleton<StringEmbeddingGenerator>( s=> new OpenAI.Embeddings.EmbeddingClient(            
            model: "text-embedding-3-small",
            apiKey: openAiKey
        ).AsIEmbeddingGenerator());

        builder.Services.AddSingleton<IndexClient>(s => new PineconeClient(pineConeKey).Index("landmark-chunks"));

        builder.Services.AddSingleton<WikipediaClient>();

        builder.Services.AddSingleton<IndexBuilder>();

        builder.Services.AddSingleton<DocumentStore>();

        builder.Services.AddSingleton<ArticleSplitter>();
        builder.Services.AddSingleton<DocumentChunkStore>();
        builder.Services.AddSingleton<VectorSearchServiceWithHyde>();

    }
}
