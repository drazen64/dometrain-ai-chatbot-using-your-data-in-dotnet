namespace ChatBot.Models;

public record DocumentChunk(
    string Id, // Id of a chunk
    string Title, // Page title
    string Section, // chunk subtitle
    int ChunkIndex, //
    string Content, // Chunk content
    string SourcePageUrl // source page
);
