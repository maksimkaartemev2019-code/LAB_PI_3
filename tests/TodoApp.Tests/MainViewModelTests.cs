using LAB_PI_3.Models;
using LAB_PI_3.ViewModels;
using TodoApp.Tests.Fakes;
using Xunit;

namespace TodoApp.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public async Task AddAsync_WithValidTask_AddsTaskAndPersistsIt()
    {
        var repository = new InMemoryTodoRepository();
        var viewModel = CreateViewModel(repository);

        viewModel.NewTitle = "Подготовить отчет";
        viewModel.NewDescription = "Собрать требования и скриншоты";

        await viewModel.AddAsync();

        Assert.Single(viewModel.Tasks);
        Assert.Single(repository.SavedItems);
        Assert.Equal("Подготовить отчет", repository.SavedItems[0].Title);
        Assert.Equal("Собрать требования и скриншоты", repository.SavedItems[0].Description);
    }

    [Fact]
    public async Task AddAsync_WithInvalidInput_ShowsValidationErrorAndDoesNotSave()
    {
        var repository = new InMemoryTodoRepository();
        var viewModel = CreateViewModel(repository);

        viewModel.NewTitle = new string('A', TodoItemValidator.MaxTitleLength + 1);
        viewModel.NewDescription = new string('B', TodoItemValidator.MaxDescriptionLength + 1);

        await viewModel.AddAsync();

        Assert.Empty(viewModel.Tasks);
        Assert.Empty(repository.SavedItems);
        Assert.True(viewModel.HasError);
    }

    [Fact]
    public async Task SelectedFilter_SeparatesAllActiveAndCompletedTasks()
    {
        var repository = new InMemoryTodoRepository(new[]
        {
            new TodoItem { Title = "Активная", IsCompleted = false, CreatedAt = DateTime.Now.AddMinutes(-1) },
            new TodoItem { Title = "Выполненная", IsCompleted = true, CreatedAt = DateTime.Now }
        });
        var viewModel = CreateViewModel(repository);

        await viewModel.InitializeAsync();

        viewModel.SelectedFilter = TodoFilter.Все;
        Assert.Equal(2, viewModel.FilteredTasks.Count);

        viewModel.SelectedFilter = TodoFilter.Активные;
        Assert.Single(viewModel.FilteredTasks);
        Assert.Equal("Активная", viewModel.FilteredTasks[0].Title);

        viewModel.SelectedFilter = TodoFilter.Выполненные;
        Assert.Single(viewModel.FilteredTasks);
        Assert.Equal("Выполненная", viewModel.FilteredTasks[0].Title);
    }

    [Fact]
    public async Task SaveSelectedAsync_UpdatesSelectedTask()
    {
        var repository = new InMemoryTodoRepository(new[]
        {
            new TodoItem { Title = "Старая", Description = "До", IsCompleted = false }
        });
        var viewModel = CreateViewModel(repository);
        await viewModel.InitializeAsync();

        viewModel.SelectedTodo = viewModel.Tasks[0];
        viewModel.EditorTitle = "Новая";
        viewModel.EditorDescription = "После";
        viewModel.EditorIsCompleted = true;

        await viewModel.SaveSelectedAsync();

        Assert.Equal("Новая", repository.SavedItems[0].Title);
        Assert.Equal("После", repository.SavedItems[0].Description);
        Assert.True(repository.SavedItems[0].IsCompleted);
    }

    [Fact]
    public async Task DeleteCheckedAsync_RemovesSelectedTasks()
    {
        var repository = new InMemoryTodoRepository(new[]
        {
            new TodoItem { Title = "Удалить" },
            new TodoItem { Title = "Оставить" }
        });
        var viewModel = CreateViewModel(repository);
        await viewModel.InitializeAsync();
        viewModel.Tasks.First(task => task.Title == "Удалить").IsSelectedForDeletion = true;

        await viewModel.DeleteCheckedAsync();

        Assert.Single(viewModel.Tasks);
        Assert.Single(repository.SavedItems);
        Assert.Equal("Оставить", repository.SavedItems[0].Title);
    }

    private static MainViewModel CreateViewModel(InMemoryTodoRepository repository)
    {
        return new MainViewModel(repository, new AlwaysConfirmService(), TimeSpan.Zero);
    }
}
