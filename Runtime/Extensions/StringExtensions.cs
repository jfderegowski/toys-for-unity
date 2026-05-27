using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace fefek5.Toys.Runtime.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if a string contains null, empty or white space
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>True if the string is null, empty or white space</returns>
        public static bool IsBlank([NotNullWhen(false)] this string val) =>
            val.IsNullOrWhiteSpace() || val.IsNullOrEmpty();

        /// <summary>
        /// Checks if a string is Null or white space
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>True if the string is null or white space</returns>
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string val) => 
            string.IsNullOrWhiteSpace(val);

        /// <summary>
        /// Checks if a string is Null or empty
        /// </summary>
        /// <param name="value">Text</param>
        /// <returns>True if the string is null or empty</returns>
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string value) => 
            string.IsNullOrEmpty(value);

        /// <summary>
        /// Checks if a string is null and returns an empty string if it is
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>Empty string if the input is null</returns>
        public static string OrEmpty(this string val) => val ?? string.Empty;

        /// <summary>
        /// Shortens a string to the specified maximum length. If the string's length
        /// is less than the maxLength, the original string is returned.
        /// </summary>
        public static string Shorten(this string val, int maxLength)
        {
            if (val.IsBlank()) return val;
            return val.Length <= maxLength ? val : val[..maxLength];
        }

        /// <summary>
        /// Slices a string from the start index to the end index.
        /// </summary>
        /// <result>The sliced string.</result>
        public static string Slice(this string val, int startIndex, int endIndex)
        {
            if (val.IsBlank())
                throw new ArgumentNullException(nameof(val), "Value cannot be null or empty.");

            if (startIndex < 0 || startIndex > val.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            // If the end index is negative, it will be counted from the end of the string.
            endIndex = endIndex < 0 ? val.Length + endIndex : endIndex;

            if (endIndex < 0 || endIndex < startIndex || endIndex > val.Length)
                throw new ArgumentOutOfRangeException(nameof(endIndex));

            return val.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Converts the input string to an alphanumeric string, optionally allowing periods.
        /// </summary>
        /// <param name="input">The input string to be converted.</param>
        /// <param name="allowPeriods">A boolean flag indicating whether periods should be allowed in the output string.</param>
        /// <returns>
        /// A new string containing only alphanumeric characters, underscores, and optionally periods.
        /// If the input string is null or empty, an empty string is returned.
        /// </returns>
        public static string ConvertToAlphanumeric(this string input, bool allowPeriods = false)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var filteredChars = new List<char>();
            var lastValidIndex = -1;

            // Iterate over the input string, filtering and determining valid start/end indices
            foreach (var character in input
                         .Where(character =>
                             char.IsLetterOrDigit(character) || character == '_' || (allowPeriods && character == '.'))
                         .Where(character =>
                             filteredChars.Count != 0 || (!char.IsDigit(character) && character != '.')))
            {
                filteredChars.Add(character);
                lastValidIndex = filteredChars.Count - 1; // Update lastValidIndex for valid characters
            }

            // Remove trailing periods
            while (lastValidIndex >= 0 && filteredChars[lastValidIndex] == '.')
                lastValidIndex--;

            // Return the filtered string
            return lastValidIndex >= 0
                ? new string(filteredChars.ToArray(), 0, lastValidIndex + 1)
                : string.Empty;
        }
        
        /// <summary>
        /// Converts the input string to a file link.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="displayText">The text to display for the link. If null, the path is used.</param>
        /// <returns>A string containing an HTML link to the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the path is null or empty.</exception>
        public static string ToFileLink(this string path, string displayText = null)
        {
            if (path.IsBlank())
                throw new ArgumentNullException(nameof(path), "Value cannot be null or empty.");

            displayText ??= path;
            
            return $"<a href=\"file:///{path}\">{displayText}</a>";
        }
    }
}