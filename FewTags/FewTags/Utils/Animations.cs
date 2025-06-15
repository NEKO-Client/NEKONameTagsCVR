using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace NekoNameTagsCVR.Utils
{
    public static class Animations
    {
        public static Color GetUnityColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return Color.black;
                case ConsoleColor.DarkBlue:
                    return Color.blue;
                case ConsoleColor.DarkGreen:
                    return Color.green;
                case ConsoleColor.DarkCyan:
                    return Color.cyan;
                case ConsoleColor.DarkRed:
                    return Color.red;
                case ConsoleColor.DarkMagenta:
                    return Color.magenta;
                case ConsoleColor.DarkYellow:
                    return Color.yellow;
                case ConsoleColor.Gray:
                    return Color.gray;
                case ConsoleColor.DarkGray:
                    return Color.gray;
                case ConsoleColor.Blue:
                    return Color.blue;
                case ConsoleColor.Green:
                    return Color.green;
                case ConsoleColor.Cyan:
                    return Color.cyan;
                case ConsoleColor.Red:
                    return Color.red;
                case ConsoleColor.Magenta:
                    return Color.magenta;
                case ConsoleColor.Yellow:
                    return Color.yellow;
                case ConsoleColor.White:
                    return Color.white;
                default:
                    return Color.white;
            }
        }

        public static async Task DisplayRainbowText(string text, GameObject plateHolder, TMPro.TextMeshProUGUI textMeshPro)
        {
            int delay = 500; // Delay in milliseconds
            ConsoleColor[] colors = {
                ConsoleColor.Red,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.Blue,
                ConsoleColor.Magenta
            };

            while (true)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    for (int j = 0; j < text.Length; j++)
                    {
                        // Set the color for the TextMeshPro component
                        textMeshPro.color = GetUnityColor(colors[(i + j) % colors.Length]);
                        textMeshPro.text = text;
                        await Task.Delay(delay);
                    }
                }

            }
        }

        // Animation text display method
        public static async Task DisplayAnimatedText(string text, TMPro.TextMeshProUGUI textMeshPro)
        {
            int delay = 500; // Delay in milliseconds

            if (textMeshPro == null)
            {
                MelonLogger.Error("TextMeshProUGUI instance is null. Cannot display text.");
                return;
            }

            try
            {
                MelonLogger.Msg("Initializing animated text display...");

                // Disable auto-sizing and adjust the container size
                textMeshPro.autoSizeTextContainer = false;
                textMeshPro.rectTransform.sizeDelta = new Vector2(200, 50);

                // Extract and sanitize tags
                var tags = new Regex(@"(<color=#[0-9A-Fa-f]{6}>|</color>|<b>|</b>)");
                var matches = tags.Matches(text);
                var tagPositions = new List<(int index, string tag)>();
                string sanitizedText = text;

                foreach (Match match in matches)
                {
                    int sanitizedIndex = match.Index - tagPositions.Sum(tp => tp.tag.Length);
                    tagPositions.Add((sanitizedIndex, match.Value));
                    sanitizedText = sanitizedText.Remove(match.Index, match.Length);
                }

                MelonLogger.Msg($"Sanitized text for animation: {sanitizedText}");

                // Animation loop
                while (true)
                {
                    // Display characters one by one
                    for (int i = 1; i <= sanitizedText.Length; i++)
                    {
                        string displayText = GetTextWithTags(sanitizedText, i, tagPositions);
                        textMeshPro.text = displayText;
                        //MelonLogger.Msg($"Displayed Text: {textMeshPro.text}");
                        await Task.Delay(delay);
                    }

                    // Delete characters one by one
                    for (int i = sanitizedText.Length - 1; i >= 0; i--)
                    {
                        string displayText = GetTextWithTags(sanitizedText, i, tagPositions);
                        textMeshPro.text = displayText;
                        //MelonLogger.Msg($"Cleared Text: {textMeshPro.text}");
                        await Task.Delay(delay);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error during text animation: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Utility method to process text with tags
        private static string GetTextWithTags(string text, int visibleCount, List<(int index, string tag)> tagPositions)
        {
            var result = new System.Text.StringBuilder();
            int currentIndex = 0;

            // Process visible characters and tags
            foreach (var tagPosition in tagPositions)
            {
                if (currentIndex < tagPosition.index && visibleCount > 0)
                {
                    // Add visible text up to the tag position
                    int charsToAdd = Math.Min(visibleCount, tagPosition.index - currentIndex);
                    result.Append(text.Substring(currentIndex, charsToAdd));
                    visibleCount -= charsToAdd;
                    currentIndex += charsToAdd;
                }

                // Add the tag if it's within the range
                if (tagPosition.index <= currentIndex)
                {
                    result.Append(tagPosition.tag);
                }
            }

            // Add any remaining visible characters
            if (visibleCount > 0 && currentIndex < text.Length)
            {
                result.Append(text.Substring(currentIndex, visibleCount));
            }

            return result.ToString();
        }


        // Helper function to remove color tags from the text
        private static string RemoveColorTags(string text)
        {
            // Regex pattern to match color tags
            string pattern = @"<color=([^>]+)>";
            return System.Text.RegularExpressions.Regex.Replace(text, pattern, "");
        }
    }
}
