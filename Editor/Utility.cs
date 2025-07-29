using System.Text;
using UnityEngine.UIElements;

namespace LocaleGenerator.Editor
{
    internal class Utility
    {
        public static VisualElement CreateTextField(string label, string initialValue, string tooltip)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 5
                }
            };

            var textField = new TextField(label)
            {
                value = initialValue,
                style =
                {
                    flexGrow = 1,
                    marginRight = 5
                },
                tooltip = tooltip
            };

            row.Add(textField);

            return row;
        }

        public static string? SanitizeName(string name, bool keepHyphens = false, bool capitalizeFirst = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var builder = new StringBuilder();
            bool capitalizeNext = capitalizeFirst;

            foreach (var c in name)
            {
                if (c == '-')
                {
                    if (keepHyphens)
                    {
                        builder.Append('-');
                    }
                    else
                    {
                        capitalizeNext = true;
                    }

                    continue;
                }

                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    continue;
                }

                builder.Append(capitalizeNext && char.IsLetter(c) ? char.ToUpper(c) : c);
                capitalizeNext = false;
            }

            if (builder.Length > 0 && char.IsDigit(builder[0]))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString();
        }
    }
}