using System.Collections.Generic;

namespace AGame.Engine.Graphics
{
    public class FormattedText
    {
        public enum FTTokenType
        {
            OpeningTag,
            ClosingTag,
            Text
        }

        public struct FTToken
        {
            public FTTokenType type;
            public string value;

            public FTToken(FTTokenType type, string value)
            {
                this.type = type;
                this.value = value;
            }
        }

        public string Text { get; set; }

        public FormattedText(string text)
        {
            this.Text = text;
        }

        public FTToken[] PerformFormatting()
        {
            List<FTToken> tokens = new List<FTToken>();

            int currentIndex = 0;
            string currentText = "";

            while (currentIndex < Text.Length)
            {
                if (Text[currentIndex] == char.Parse("<"))
                {
                    if (currentText != "")
                    {
                        tokens.Add(new FTToken(FTTokenType.Text, currentText));
                        currentText = "";
                    }

                    string tagText = "";
                    // Beginning of tag
                    currentIndex++;

                    bool openingTag = true;

                    if (Text[currentIndex] == char.Parse("/"))
                    {
                        // This is a closing tag
                        openingTag = false;
                    }
                    else
                    {
                        // This is an opening tag
                    }

                    while (currentIndex < Text.Length && Text[currentIndex] != char.Parse(">"))
                    {
                        tagText += Text[currentIndex];
                        currentIndex++;
                    }

                    tokens.Add(new FTToken(openingTag ? FTTokenType.OpeningTag : FTTokenType.ClosingTag, tagText));
                    currentIndex++;
                }
                else
                {
                    currentText += Text[currentIndex];
                    currentIndex++;
                }
            }

            if (currentText != "")
            {
                tokens.Add(new FTToken(FTTokenType.Text, currentText));
            }

            return tokens.ToArray();
        }
    }
}