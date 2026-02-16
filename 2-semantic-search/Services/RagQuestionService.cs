
using Microsoft.Extensions.AI;

namespace ChatBot.Services;

public class RagQuestionService(VectorSearchServiceWithHyde vectorSearchService, IChatClient client, ChatOptions chatOptions, PromptService promptService)
{

    public async Task<string> AnswerQuestion(string question)
    {
        //
        // 1. Retrieval
        //
        var serchResults = await vectorSearchService.FindTopKChunks(question, 5);

        //
        // 2. Augmentation
        //
        var systemPrompt = promptService.RagSystemPrompt;
        var userPrompt = $@"User question: 
{question}

Retrieved article sections:
{string.Join("\n\n", serchResults.Select(chunk => @$"TItle: {chunk.Title}
Section: {chunk.Section}
Part: {chunk.ChunkIndex + 1}
Content: {chunk.Content}
URL: {chunk.SourcePageUrl}"))}
";

        var messages = (new[]
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, userPrompt)
        }).ToList();


        //
        // 3. Generation
        //
        var response = await client.GetResponseAsync(messages, chatOptions);

        return response.Text;

    }
}