namespace AGame.Engine.DebugTools
{
    public enum CommandResultType
    {
        Ok,
        Error,
        Warning
    }

    public class CommandResult
    {
        public CommandResultType Type { get; set; }
        public string Message { get; set; }
    }
}