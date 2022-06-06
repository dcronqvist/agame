using System.Collections.Generic;

namespace AGame.Engine.DebugTools
{
    public class ConsoleLine
    {
        public string BetweenBrackets { get; set; }
        public string Message { get; set; }

        private Dictionary<string, string> betweenBracketsToCol;

        public ConsoleLine(string betweenBrackets, string message)
        {
            BetweenBrackets = betweenBrackets;
            Message = message;
            betweenBracketsToCol = new Dictionary<string, string>() {
                { "OK", "0x00FF00" },
                { "ERROR", "0xFF0000" },
                { "WARNING", "0xFFFF00" }
            };
        }

        public override string ToString()
        {
            string hex = betweenBracketsToCol.GetValueOrDefault(this.BetweenBrackets, "0xFFFFFF");
            return $"[<{hex}>{this.BetweenBrackets}</>]: {this.Message}";
        }
    }
}