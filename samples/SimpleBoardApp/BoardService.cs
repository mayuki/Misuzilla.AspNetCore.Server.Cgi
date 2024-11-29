using System.Text.Json;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

class BoardService
{
    public string DataDirectory => "entries";

    public async ValueTask<IReadOnlyList<BoardEntry>> GetEntriesAsync(int entriesPerPage, Guid? after = null)
    {
        EnsureDataDirectory();

        var entries = Directory.EnumerateFiles(DataDirectory, "*.json")
            .Select(x => JsonSerializer.Deserialize<BoardEntry>(File.ReadAllBytes(x), AppJsonSerializerContext.Default.BoardEntry)!)
            .ToArray();
        return entries.OrderByDescending(x => x.CreatedAt).Take(entriesPerPage).ToArray();
    }

    public async ValueTask AddEntryAsync(string name, string body)
    {
        EnsureDataDirectory();

        var entry = new BoardEntry(Guid.CreateVersion7(), name, body, DateTimeOffset.UtcNow);
        File.WriteAllText(Path.Combine(DataDirectory, entry.Id + ".json"), JsonSerializer.Serialize(entry, typeof(BoardEntry), AppJsonSerializerContext.Default));
    }

    private void EnsureDataDirectory()
    {
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }
    }
}


public record BoardEntry(Guid Id, string Name, string Body, DateTimeOffset CreatedAt);