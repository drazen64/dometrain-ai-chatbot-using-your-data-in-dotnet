
using ChatBot.Models;
using Microsoft.Extensions.AI;

namespace ChatBot.Services;

public class VectorSearchServiceWithHyde(StringEmbeddingGenerator embeddingGenerator,
                           Pinecone.IndexClient pineconeIndex,
                           DocumentChunkStore contentStore,
                           IChatClient chatClient,
                           ChatOptions chatOptions,
                           PromptService promptService)
{

    private async Task<string?> GeneryteHypothesisAsync(string question)
    {
        //var systemText = "Generate a concise hypothesis about the following question.";  
        var systemText = "You create concise, factual reference passages.";
        var userPrompt = promptService.HydePrompt.Replace("{{question}}", question);

        var messages = (new[]
        {
            new ChatMessage(ChatRole.System, systemText),
            new ChatMessage(ChatRole.User, userPrompt)
        }).ToList();

        var response = await chatClient.GetResponseAsync(messages, chatOptions);
        var text = response?.Text?.Trim();
        if(string.IsNullOrWhiteSpace(text))
            return null;
        return text.Length > 1500 ? text.Substring(0, 1500) : text;
    }

    public async Task<List<DocumentChunk>> FindTopKChunks(string question, int k)
    {
        if (string.IsNullOrWhiteSpace(question))
            return [];

        var hypothesis = await GeneryteHypothesisAsync(question);

        var textToEmbed = hypothesis ?? question;

        var embeddings = await embeddingGenerator.GenerateAsync([textToEmbed],
                new Microsoft.Extensions.AI.EmbeddingGenerationOptions
                {
                    Dimensions = 512
                });        

        var vector = embeddings[0].Vector.ToArray();

        var response = await pineconeIndex.QueryAsync(new Pinecone.QueryRequest
        {
            Vector = vector,
            TopK = (uint)k,
            IncludeMetadata = true
        });

        var matches = (response.Matches ?? []).ToList();
        if (matches.Count == 0)
            return [];
        var ids = matches.Select(m => m.Id!).Where(id => !string.IsNullOrEmpty(id));
        var articles = contentStore.GetDocumentChunks(ids);
        var scoreById = matches.Where(m => m.Id is not null)
                               .ToDictionary(m => m.Id!, m => m.Score);
        var ordered = articles.OrderByDescending(a => scoreById.GetValueOrDefault(a.Id, 0f))
                                .Take(k)
                                .ToList();  

        return ordered;

    }



}