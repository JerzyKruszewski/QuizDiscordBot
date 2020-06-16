using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace QuizDiscordBot
{
    /// <summary>
    /// Klasa narzędzi urzywanych w innych miejscach w programie
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Lista emotek oznaczających możliwe odpowiedzi
        /// </summary>
        /// <remarks>
        /// Odgórny limit Discord: 20 emotek
        /// </remarks>
        public static IEnumerable<Emoji> AnswersEmojis { get; } = new List<Emoji>()
        {
            new Emoji("🇦"),
            new Emoji("🇧"),
            new Emoji("🇨"),
            new Emoji("🇩"),
            new Emoji("🇪"),
            new Emoji("🇫"),
            new Emoji("🇬"),
            new Emoji("🇭"),
            new Emoji("🇮"),
            new Emoji("🇯"),
            new Emoji("🇰"),
            new Emoji("🇱"),
            new Emoji("🇲"),
            new Emoji("🇳"),
            new Emoji("🇴"),
            new Emoji("🇵"),
            new Emoji("🇷"),
            new Emoji("🇸"),
            new Emoji("🇹"),
            new Emoji("🇺")
        };
    }
}
