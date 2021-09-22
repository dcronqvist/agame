namespace AGame.Engine.DebugTools
{
    public class ConsoleLine
    {
        public string BetweenBrackets { get; set; }
        public string Message { get; set; }

        public ConsoleLine(string betweenBrackets, string message)
        {
            BetweenBrackets = betweenBrackets;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{this.BetweenBrackets}]: {this.Message}";
        }
    }
}