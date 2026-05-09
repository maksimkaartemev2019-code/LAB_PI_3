using LAB_PI_3.Services;

namespace TodoApp.Tests.Fakes;

public sealed class AlwaysConfirmService : IConfirmationService
{
    public bool Confirm(string message, string title)
    {
        return true;
    }
}
