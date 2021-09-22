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

        public static CommandResult CreateOk(string message)
        {
            return new CommandResult()
            {
                Type = CommandResultType.Ok,
                Message = message
            };
        }

        public static CommandResult CreateWarning(string message)
        {
            return new CommandResult()
            {
                Type = CommandResultType.Warning,
                Message = message
            };
        }

        public static CommandResult CreateError(string message)
        {
            return new CommandResult()
            {
                Type = CommandResultType.Error,
                Message = message
            };
        }

        public override string ToString()
        {
            return $"[{this.Type.ToString().ToUpper()}]: {this.Message}";
        }
    }
}