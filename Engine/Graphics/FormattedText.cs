using System.Collections.Generic;
using System.Linq;

namespace AGame.Engine.Graphics;

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

        public string GetTagName()
        {
            return this.value.Split(" ").First();
        }

        public Dictionary<string, string> GetTagAttributes()
        {
            var attributes = new Dictionary<string, string>();
            var tagName = this.GetTagName();
            var tagAttributes = this.value.Split(" ").Skip(1).ToArray();
            foreach (var attribute in tagAttributes)
            {
                var attributeName = attribute.Split("=").First();
                var attributeValue = attribute.Split("=").Skip(1).First();
                attributes.Add(attributeName, attributeValue);
            }
            return attributes;
        }

        public string GetAttributeValue(string attrib, string defaultValue = "")
        {
            var attributes = this.GetTagAttributes();
            return attributes.GetValueOrDefault(attrib, defaultValue);
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

        string escapable = @"<>";

        while (currentIndex < Text.Length)
        {
            if (Text[currentIndex] == char.Parse(@"\"))
            {
                if (currentIndex + 1 < Text.Length && escapable.Contains(Text[currentIndex + 1]))
                {
                    currentText += Text[currentIndex + 1];
                    currentIndex += 2;
                }
            }

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
                    currentIndex++;
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