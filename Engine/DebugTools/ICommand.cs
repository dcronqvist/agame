namespace AGame.Engine.DebugTools
{
    public interface ICommand
    {
        string GetHandle();
        CommandResult Execute();
    }
}