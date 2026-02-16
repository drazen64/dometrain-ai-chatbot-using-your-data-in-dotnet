namespace ChatBot.Services;

public class PromptService
{
    static readonly Dictionary<string, string> Prompts = new Dictionary<string, string>();

    public PromptService()
    {
        var promptsDirectory = Path.Combine(AppContext.BaseDirectory, "Prompts");
        foreach(var promptName in new[]{"RagSystemPrompt", "ChatSystemPrompt", "HydePrompt"})
        {
            var promptText = File.ReadAllText(Path.Combine(promptsDirectory, promptName + ".txt"));
            Prompts[promptName] = promptText;
        }
    }


    public string RagSystemPrompt => Prompts["RagSystemPrompt"];
    public string ChatSystemPrompt => Prompts["ChatSystemPrompt"];
    public string HydePrompt => Prompts["HydePrompt"];
}