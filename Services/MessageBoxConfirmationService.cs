using System.Windows;

namespace LAB_PI_3.Services;

public sealed class MessageBoxConfirmationService : IConfirmationService
{
    public bool Confirm(string message, string title)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
