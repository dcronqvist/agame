namespace AGame.Engine.DebugTools
{
    enum CommandResultType
    {
        Ok,
        Error,
        Warning
    }

    class CommandResult
    {
        public CommandResultType Type { get; set; }
        public string Message { get; set; }
    }
}