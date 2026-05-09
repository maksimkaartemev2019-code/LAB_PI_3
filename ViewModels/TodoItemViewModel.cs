using LAB_PI_3.Models;

namespace LAB_PI_3.ViewModels;

public sealed class TodoItemViewModel : ObservableObject
{
    private readonly TodoItem item;
    private bool isSelectedForDeletion;

    public TodoItemViewModel(TodoItem item)
    {
        this.item = item;
    }

    public TodoItem Model => item;

    public Guid Id => item.Id;

    public string Title
    {
        get => item.Title;
        set
        {
            if (item.Title != value)
            {
                item.Title = value;
                OnPropertyChanged();
            }
        }
    }

    public string Description
    {
        get => item.Description;
        set
        {
            if (item.Description != value)
            {
                item.Description = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsCompleted
    {
        get => item.IsCompleted;
        set
        {
            if (item.IsCompleted != value)
            {
                item.IsCompleted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public DateTime CreatedAt => item.CreatedAt;

    public string StatusText => item.StatusText;

    public bool IsSelectedForDeletion
    {
        get => isSelectedForDeletion;
        set => SetProperty(ref isSelectedForDeletion, value);
    }
}
