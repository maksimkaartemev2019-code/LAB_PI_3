namespace LAB_PI_3.Models;

public static class TodoItemValidator
{
    public const int MaxTitleLength = 100;
    public const int MaxDescriptionLength = 500;

    public static string? Validate(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Название обязательно.";
        }

        if (title.Trim().Length > MaxTitleLength)
        {
            return $"Название не должно быть длиннее {MaxTitleLength} символов.";
        }

        if ((description ?? string.Empty).Trim().Length > MaxDescriptionLength)
        {
            return $"Описание не должно быть длиннее {MaxDescriptionLength} символов.";
        }

        return null;
    }
}
