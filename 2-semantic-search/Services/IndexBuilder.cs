using ChatBot.Services;
using Pinecone;


namespace ChatBot.Services;
public class IndexBuilder(
    StringEmbeddingGenerator embeddingGenerator,
    IndexClient pineConeIndex,
    WikipediaClient wikipediaClient,
    DocumentStore documentStore)
{
    public async Task BuildDocumentIndex(string[] pageTitles)
    {
        foreach(var landMark in pageTitles)
        {
            var wikiPage = await wikipediaClient.GetWikipediaPageForTitle(landMark);
            var embedding = await embeddingGenerator.GenerateAsync([wikiPage.Content], 
                new Microsoft.Extensions.AI.EmbeddingGenerationOptions
                {
                    Dimensions = 512
                });
            var vectorArray = embedding[0].Vector.ToArray();
            var pineConeVector = new Vector
            {
                Id = wikiPage.Id,
                Values = vectorArray,
                Metadata = new Metadata
                {
                    {"title", wikiPage.Title}
                }
            };

            await pineConeIndex.UpsertAsync(new UpsertRequest
            {
                Vectors = [pineConeVector]
            });

            //TODO Save to Database
            documentStore.SaveDocument(wikiPage);
        }
    }
}