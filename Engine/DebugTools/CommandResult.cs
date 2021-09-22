namespace AGame.Engine.DebugTools
{
    public enum CommandResultType
    {
        Ok,
        Error,
        Warning
    }

    public class CommandResult : ConsoleLine
    {
        public CommandResultType Type { get; set; }

        public CommandResult(CommandResultType crt, string mess) : base(crt.ToString().ToUpper(), mess)
        {

        }

        public static CommandResult CreateOk(string message)
        {
            return new CommandResult(CommandResultType.Ok, message);
        }

        public static CommandResult CreateWarning(string message)
        {
            return new CommandResult(CommandResultType.Warning, message);
        }

        public static CommandResult CreateError(string message)
        {
            return new CommandResult(CommandResultType.Error, message);
        }
    }
}