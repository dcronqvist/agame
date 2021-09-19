namespace AGame.DebugTools
{
    interface ICommand
    {
        string GetHandle();
        CommandResult Execute();
    }
}