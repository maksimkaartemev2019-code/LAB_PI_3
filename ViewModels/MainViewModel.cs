using System.Collections.ObjectModel;
using System.Windows.Input;
using LAB_PI_3.Models;
using LAB_PI_3.Services;

namespace LAB_PI_3.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ITodoRepository repository;
    private readonly IConfirmationService confirmationService;
    private readonly TimeSpan operationDelay;
    private string newTitle = string.Empty;
    private string newDescription = string.Empty;
    private string editorTitle = string.Empty;
    private string editorDescription = string.Empty;
    private bool editorIsCompleted;
    private string errorMessage = string.Empty;
    private bool isBusy;
    private TodoFilter selectedFilter;
    private TodoItemViewModel? selectedTodo;

    public MainViewModel(ITodoRepository repository, IConfirmationService confirmationService, TimeSpan? operationDelay = null)
    {
        this.repository = repository;
        this.confirmationService = confirmationService;
        this.operationDelay = operationDelay ?? TimeSpan.FromMilliseconds(700);

        FilterOptions = new ObservableCollection<TodoFilter>(
            Enum.GetValues<TodoFilter>());
        Tasks = new ObservableCollection<TodoItemViewModel>();
        FilteredTasks = new ObservableCollection<TodoItemViewModel>();

        InitializeCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
        AddCommand = new AsyncRelayCommand(AddAsync, () => !IsBusy);
        SaveSelectedCommand = new AsyncRelayCommand(SaveSelectedAsync, () => !IsBusy && SelectedTodo is not null);
        DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync, () => !IsBusy && SelectedTodo is not null);
        DeleteCheckedCommand = new AsyncRelayCommand(DeleteCheckedAsync, () => !IsBusy && Tasks.Any(task => task.IsSelectedForDeletion));
    }

    public ObservableCollection<TodoFilter> FilterOptions { get; }

    public ObservableCollection<TodoItemViewModel> Tasks { get; }

    public ObservableCollection<TodoItemViewModel> FilteredTasks { get; }

    public ICommand InitializeCommand { get; }

    public AsyncRelayCommand AddCommand { get; }

    public AsyncRelayCommand SaveSelectedCommand { get; }

    public AsyncRelayCommand DeleteSelectedCommand { get; }

    public AsyncRelayCommand DeleteCheckedCommand { get; }

    public string NewTitle
    {
        get => newTitle;
        set
        {
            if (SetProperty(ref newTitle, value))
            {
                ClearError();
            }
        }
    }

    public string NewDescription
    {
        get => newDescription;
        set
        {
            if (SetProperty(ref newDescription, value))
            {
                ClearError();
            }
        }
    }

    public string EditorTitle
    {
        get => editorTitle;
        set
        {
            if (SetProperty(ref editorTitle, value))
            {
                ClearError();
            }
        }
    }

    public string EditorDescription
    {
        get => editorDescription;
        set
        {
            if (SetProperty(ref editorDescription, value))
            {
                ClearError();
            }
        }
    }

    public bool EditorIsCompleted
    {
        get => editorIsCompleted;
        set => SetProperty(ref editorIsCompleted, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public TodoFilter SelectedFilter
    {
        get => selectedFilter;
        set
        {
            if (SetProperty(ref selectedFilter, value))
            {
                RefreshFilter();
            }
        }
    }

    public TodoItemViewModel? SelectedTodo
    {
        get => selectedTodo;
        set
        {
            if (SetProperty(ref selectedTodo, value))
            {
                LoadEditorFromSelection();
                RaiseCommandStates();
            }
        }
    }

    public async Task InitializeAsync()
    {
        await RunBusyAsync(async () =>
        {
            Tasks.Clear();
            var loadedItems = await repository.LoadAsync();
            foreach (var item in loadedItems.OrderByDescending(item => item.CreatedAt))
            {
                Tasks.Add(new TodoItemViewModel(item));
            }

            RefreshFilter();
        }, includeDelay: false);
    }

    public async Task AddAsync()
    {
        var validationError = TodoItemValidator.Validate(NewTitle, NewDescription);
        if (validationError is not null)
        {
            SetError(validationError);
            return;
        }

        await RunBusyAsync(async () =>
        {
            var item = new TodoItem
            {
                Title = NewTitle.Trim(),
                Description = (NewDescription ?? string.Empty).Trim(),
                CreatedAt = DateTime.Now
            };

            var viewModel = new TodoItemViewModel(item);
            Tasks.Insert(0, viewModel);
            await SaveAllAsync();
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            SelectedTodo = viewModel;
            RefreshFilter();
        });
    }

    public async Task SaveSelectedAsync()
    {
        if (SelectedTodo is null)
        {
            return;
        }

        var validationError = TodoItemValidator.Validate(EditorTitle, EditorDescription);
        if (validationError is not null)
        {
            SetError(validationError);
            return;
        }

        await RunBusyAsync(async () =>
        {
            SelectedTodo.Title = EditorTitle.Trim();
            SelectedTodo.Description = (EditorDescription ?? string.Empty).Trim();
            SelectedTodo.IsCompleted = EditorIsCompleted;
            await SaveAllAsync();
            RefreshFilter();
        });
    }

    public async Task DeleteSelectedAsync()
    {
        if (SelectedTodo is null)
        {
            return;
        }

        if (!confirmationService.Confirm($"Удалить задачу \"{SelectedTodo.Title}\"?", "Подтверждение удаления"))
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            Tasks.Remove(SelectedTodo);
            SelectedTodo = null;
            await SaveAllAsync();
            RefreshFilter();
        });
    }

    public async Task DeleteCheckedAsync()
    {
        var checkedTasks = Tasks.Where(task => task.IsSelectedForDeletion).ToList();
        if (checkedTasks.Count == 0)
        {
            return;
        }

        if (!confirmationService.Confirm($"Удалить выбранные задачи: {checkedTasks.Count}?", "Подтверждение удаления"))
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            foreach (var task in checkedTasks)
            {
                Tasks.Remove(task);
            }

            if (SelectedTodo is not null && checkedTasks.Any(task => task.Id == SelectedTodo.Id))
            {
                SelectedTodo = null;
            }

            await SaveAllAsync();
            RefreshFilter();
        });
    }

    private async Task RunBusyAsync(Func<Task> operation, bool includeDelay = true)
    {
        try
        {
            IsBusy = true;
            if (includeDelay && operationDelay > TimeSpan.Zero)
            {
                await Task.Delay(operationDelay);
            }

            await operation();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task SaveAllAsync()
    {
        return repository.SaveAsync(Tasks.Select(task => task.Model));
    }

    private void RefreshFilter()
    {
        FilteredTasks.Clear();

        var filtered = SelectedFilter switch
        {
            TodoFilter.Активные => Tasks.Where(task => !task.IsCompleted),
            TodoFilter.Выполненные => Tasks.Where(task => task.IsCompleted),
            _ => Tasks
        };

        foreach (var task in filtered)
        {
            FilteredTasks.Add(task);
        }

        RaiseCommandStates();
    }

    private void LoadEditorFromSelection()
    {
        EditorTitle = SelectedTodo?.Title ?? string.Empty;
        EditorDescription = SelectedTodo?.Description ?? string.Empty;
        EditorIsCompleted = SelectedTodo?.IsCompleted ?? false;
    }

    private void ClearError()
    {
        if (HasError)
        {
            SetError(string.Empty);
        }
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        OnPropertyChanged(nameof(HasError));
    }

    private void RaiseCommandStates()
    {
        AddCommand.RaiseCanExecuteChanged();
        SaveSelectedCommand.RaiseCanExecuteChanged();
        DeleteSelectedCommand.RaiseCanExecuteChanged();
        DeleteCheckedCommand.RaiseCanExecuteChanged();
    }
}
