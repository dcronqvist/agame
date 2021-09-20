namespace AGame.Engine.DebugTools
{
    interface ICommand
    {
        string GetHandle();
        CommandResult Execute();
    }
}