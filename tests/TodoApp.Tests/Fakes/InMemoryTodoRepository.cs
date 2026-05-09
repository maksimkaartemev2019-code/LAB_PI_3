using LAB_PI_3.Models;
using LAB_PI_3.Services;

namespace TodoApp.Tests.Fakes;

public sealed class InMemoryTodoRepository : ITodoRepository
{
    private List<TodoItem> items;

    public InMemoryTodoRepository(IEnumerable<TodoItem>? initialItems = null)
    {
        items = initialItems?.Select(Clone).ToList() ?? new List<TodoItem>();
    }

    public IReadOnlyList<TodoItem> SavedItems => items;

    public Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TodoItem>>(items.Select(Clone).ToList());
    }

    public Task SaveAsync(IEnumerable<TodoItem> savedItems, CancellationToken cancellationToken = default)
    {
        items = savedItems.Select(Clone).ToList();
        return Task.CompletedTask;
    }

    private static TodoItem Clone(TodoItem item)
    {
        return new TodoItem
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            IsCompleted = item.IsCompleted,
            CreatedAt = item.CreatedAt
        };
    }
}
