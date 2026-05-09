using LAB_PI_3.Models;

namespace LAB_PI_3.Services;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(IEnumerable<TodoItem> items, CancellationToken cancellationToken = default);
}
