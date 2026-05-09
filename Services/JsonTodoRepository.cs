using System.IO;
using System.Text.Json;
using LAB_PI_3.Models;

namespace LAB_PI_3.Services;

public sealed class JsonTodoRepository : ITodoRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string filePath;

    public JsonTodoRepository(string? filePath = null)
    {
        this.filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LAB_PI_3",
            "tasks.json");
    }

    public async Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return Array.Empty<TodoItem>();
        }

        await using var stream = File.OpenRead(filePath);
        var items = await JsonSerializer.DeserializeAsync<List<TodoItem>>(
            stream,
            SerializerOptions,
            cancellationToken);

        return items is null ? Array.Empty<TodoItem>() : items;
    }

    public async Task SaveAsync(IEnumerable<TodoItem> items, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, items, SerializerOptions, cancellationToken);
    }
}
