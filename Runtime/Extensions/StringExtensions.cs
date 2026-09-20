using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

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
        /// <example>
        /// <code><![CDATA[
        /// "C:/Saves/slot0.json".ToFileLink("[File]");
        /// // before: C:/Saves/slot0.json
        /// // after:  <a href="file:///C:/Saves/slot0.json">[File]</a>
        /// ]]></code>
        /// </example>
        public static string ToFileLink(this string path, string displayText = null)
        {
            if (path.IsBlank())
                throw new ArgumentNullException(nameof(path), "Value cannot be null or empty.");

            displayText ??= path;
            
            return $"<a href=\"file:///{path}\">{displayText}</a>";
        }

        /// <summary>
        /// Converts the input string to a hyperlink.
        /// </summary>
        /// <param name="url">The target of the link.</param>
        /// <param name="displayText">The text to display for the link. If null, the url is used.</param>
        /// <returns>A string containing an HTML link.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the url is null or empty.</exception>
        /// <example>
        /// <code><![CDATA[
        /// "https://unity.com".ToLink("Unity");
        /// // before: https://unity.com
        /// // after:  <a href="https://unity.com">Unity</a>
        /// ]]></code>
        /// </example>
        public static string ToLink(this string url, string displayText = null)
        {
            if (url.IsBlank())
                throw new ArgumentNullException(nameof(url), "Value cannot be null or empty.");

            displayText ??= url;

            return $"<a href=\"{url}\">{displayText}</a>";
        }

        /// <summary>
        /// Converts the input string to a file link that points at a specific line.
        /// The line is passed to the click handler as part of the hyperlink data, next to the href.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="line">The line the file should be opened at.</param>
        /// <param name="displayText">The text to display for the link. If null, the path and the line are used.</param>
        /// <returns>A string containing an HTML link to the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the path is null or empty.</exception>
        /// <example>
        /// <code><![CDATA[
        /// "Assets/Scripts/Player.cs".ToFileLink(42);
        /// // before: Assets/Scripts/Player.cs
        /// // after:  <a href="file:///Assets/Scripts/Player.cs" line="42">Assets/Scripts/Player.cs:42</a>
        /// ]]></code>
        /// </example>
        public static string ToFileLink(this string path, int line, string displayText = null)
        {
            if (path.IsBlank())
                throw new ArgumentNullException(nameof(path), "Value cannot be null or empty.");

            displayText ??= $"{path}:{line}";

            return $"<a href=\"file:///{path}\" line=\"{line}\">{displayText}</a>";
        }

        /// <summary>
        /// Wraps the string in a rich text color tag, as supported by the Unity console.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="color">The color to apply.</param>
        /// <returns>The text wrapped in a color tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Saved".SetColor(Color.green);
        /// // before: Saved
        /// // after:  <color=#00FF00FF>Saved</color>
        /// ]]></code>
        /// </example>
        public static string SetColor(this string val, Color color) =>
            val.SetColor(color.ToHex());

        /// <summary>
        /// Wraps the string in a rich text color tag, as supported by the Unity console.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="hexOrName">A hexadecimal color (e.g. "#FF0000") or a rich text color name (e.g. "red").</param>
        /// <returns>The text wrapped in a color tag, or the original value if it or the color is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Saved".SetColor("#4CAF50");
        /// // before: Saved
        /// // after:  <color=#4CAF50>Saved</color>
        /// ]]></code>
        /// </example>
        public static string SetColor(this string val, string hexOrName) =>
            val.IsBlank() || hexOrName.IsBlank() ? val : $"<color={hexOrName}>{val}</color>";

        /// <summary>
        /// Wraps the string in a rich text size tag, as supported by the Unity console.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="pixels">The font size in pixels.</param>
        /// <returns>The text wrapped in a size tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Title".SetSize(24);
        /// // before: Title
        /// // after:  <size=24>Title</size>
        /// ]]></code>
        /// </example>
        public static string SetSize(this string val, int pixels) =>
            val.SetSize(pixels.ToString());

        /// <summary>
        /// Wraps the string in a rich text size tag, as supported by the Unity console.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="size">A size with an optional unit (e.g. "14", "120%", "+2" or "1.5em").</param>
        /// <returns>The text wrapped in a size tag, or the original value if it or the size is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Title".SetSize("120%");
        /// // before: Title
        /// // after:  <size=120%>Title</size>
        /// ]]></code>
        /// </example>
        public static string SetSize(this string val, string size) =>
            val.IsBlank() || size.IsBlank() ? val : $"<size={size}>{val}</size>";

        /// <summary>
        /// Wraps the string in a rich text bold tag, as supported by the Unity console.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in a bold tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Saved".SetBold();
        /// // before: Saved
        /// // after:  <b>Saved</b>
        /// ]]></code>
        /// </example>
        public static string SetBold(this string val) =>
            val.IsBlank() ? val : $"<b>{val}</b>";

        /// <summary>
        /// Wraps the string in a rich text italic tag, as supported by the Unity console.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in an italic tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Saved".SetItalic();
        /// // before: Saved
        /// // after:  <i>Saved</i>
        /// ]]></code>
        /// </example>
        public static string SetItalic(this string val) =>
            val.IsBlank() ? val : $"<i>{val}</i>";

        /// <summary>
        /// Wraps the string in a rich text underline tag. Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in an underline tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Saved".SetUnderline();
        /// // before: Saved
        /// // after:  <u>Saved</u>
        /// ]]></code>
        /// </example>
        public static string SetUnderline(this string val) =>
            val.IsBlank() ? val : $"<u>{val}</u>";

        /// <summary>
        /// Wraps the string in a rich text strikethrough tag. Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in a strikethrough tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Saved".SetStrikethrough();
        /// // before: Saved
        /// // after:  <s>Saved</s>
        /// ]]></code>
        /// </example>
        public static string SetStrikethrough(this string val) =>
            val.IsBlank() ? val : $"<s>{val}</s>";

        /// <summary>
        /// Wraps the string in a rich text noparse tag, so that any markup inside it is displayed verbatim
        /// instead of being parsed. Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in a noparse tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "<b>not bold</b>".SetNoParse();
        /// // before: <b>not bold</b>
        /// // after:  <noparse><b>not bold</b></noparse>
        /// ]]></code>
        /// </example>
        public static string SetNoParse(this string val) =>
            val.IsBlank() ? val : $"<noparse>{val}</noparse>";

        /// <summary>
        /// Wraps the string in a rich text mark tag, highlighting it with the given color.
        /// Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="color">The highlight color. It is drawn behind the text, so a translucent color is usually wanted.</param>
        /// <returns>The text wrapped in a mark tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Warning".SetMark(new Color(1f, 1f, 0f, 0.5f));
        /// // before: Warning
        /// // after:  <mark=#FFFF0080>Warning</mark>
        /// ]]></code>
        /// </example>
        public static string SetMark(this string val, Color color) =>
            val.SetMark(color.ToHex());

        /// <summary>
        /// Wraps the string in a rich text mark tag, highlighting it with the given color.
        /// Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="hexOrName">A hexadecimal color with alpha (e.g. "#FFFF0080") or a rich text color name (e.g. "yellow").</param>
        /// <returns>The text wrapped in a mark tag, or the original value if it or the color is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Warning".SetMark("#FFFF0080");
        /// // before: Warning
        /// // after:  <mark=#FFFF0080>Warning</mark>
        /// ]]></code>
        /// </example>
        public static string SetMark(this string val, string hexOrName) =>
            val.IsBlank() || hexOrName.IsBlank() ? val : $"<mark={hexOrName}>{val}</mark>";

        /// <summary>
        /// Wraps the string in a rich text subscript tag. Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in a subscript tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "H" + "2".SetSubscript() + "O";
        /// // before: H2O
        /// // after:  H<sub>2</sub>O
        /// ]]></code>
        /// </example>
        public static string SetSubscript(this string val) =>
            val.IsBlank() ? val : $"<sub>{val}</sub>";

        /// <summary>
        /// Wraps the string in a rich text superscript tag. Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <returns>The text wrapped in a superscript tag, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "m" + "2".SetSuperscript();
        /// // before: m2
        /// // after:  m<sup>2</sup>
        /// ]]></code>
        /// </example>
        public static string SetSuperscript(this string val) =>
            val.IsBlank() ? val : $"<sup>{val}</sup>";

        /// <summary>
        /// Wraps the string in a rich text link tag, making it clickable in UI Toolkit. The id is reported by the
        /// pointer link tag events raised by the text element. The Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="id">The id reported when the link is clicked.</param>
        /// <returns>The text wrapped in a link tag, or the original value if it or the id is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Open settings".SetLink("settings");
        /// // before: Open settings
        /// // after:  <link="settings">Open settings</link>
        /// ]]></code>
        /// </example>
        public static string SetLink(this string val, string id) =>
            val.IsBlank() || id.IsBlank() ? val : $"<link=\"{id}\">{val}</link>";

        /// <summary>
        /// Applies a rich text alpha tag to the string. The tag has no closing form and would otherwise leak onto the
        /// following text, so the opaque value is restored afterwards. Rendered by UI Toolkit; the Unity console ignores it.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="alpha">The opacity to apply, from 0 (transparent) to 255 (opaque).</param>
        /// <returns>The text preceded by an alpha tag and followed by an opaque one, or the original value if it is blank.</returns>
        /// <example>
        /// <code><![CDATA[
        /// "Faded".SetAlpha(128);
        /// // before: Faded
        /// // after:  <alpha=#80>Faded<alpha=#FF>
        /// ]]></code>
        /// </example>
        public static string SetAlpha(this string val, byte alpha) =>
            val.IsBlank() ? val : $"<alpha=#{alpha:X2}>{val}<alpha=#FF>";

        /// <summary>
        /// Ensures the string starts with the given prefix, adding it only if it is missing.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="prefix">The prefix to apply.</param>
        /// <returns>The text with the prefix, or the original value if it or the prefix is blank.</returns>
        public static string SetPrefix(this string val, string prefix) =>
            val.IsBlank() || prefix.IsBlank() || val.StartsWith(prefix, StringComparison.Ordinal)
                ? val
                : prefix + val;

        /// <summary>
        /// Ensures the string ends with the given suffix, adding it only if it is missing.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="suffix">The suffix to apply.</param>
        /// <returns>The text with the suffix, or the original value if it or the suffix is blank.</returns>
        public static string SetSuffix(this string val, string suffix) =>
            val.IsBlank() || suffix.IsBlank() || val.EndsWith(suffix, StringComparison.Ordinal)
                ? val
                : val + suffix;

        /// <summary>
        /// Removes the given prefix from the start of the string, if it is present.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="prefix">The prefix to remove.</param>
        /// <returns>The text without the prefix, or the original value if it does not start with it.</returns>
        public static string RemovePrefix(this string val, string prefix) =>
            val.IsBlank() || prefix.IsBlank() || !val.StartsWith(prefix, StringComparison.Ordinal)
                ? val
                : val[prefix.Length..];

        /// <summary>
        /// Removes the given suffix from the end of the string, if it is present.
        /// </summary>
        /// <param name="val">Text</param>
        /// <param name="suffix">The suffix to remove.</param>
        /// <returns>The text without the suffix, or the original value if it does not end with it.</returns>
        public static string RemoveSuffix(this string val, string suffix) =>
            val.IsBlank() || suffix.IsBlank() || !val.EndsWith(suffix, StringComparison.Ordinal)
                ? val
                : val[..^suffix.Length];
    }
}
