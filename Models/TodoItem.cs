using System.Text.Json.Serialization;

namespace LAB_PI_3.Models;

public sealed class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public string StatusText => IsCompleted ? "Выполнена" : "Активна";
}
