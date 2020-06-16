namespace QuizDiscordBot.Core.Objects
{
    /// <summary>
    /// Klasa reprezentująca ocenę urzytkownika
    /// </summary>
    public class UserFeedback
    {
        /// <summary>
        /// Discordowy identyfikator osoby pozostawiający ocenę
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Czy ocane jest pozytywna
        /// </summary>
        public bool IsPositive { get; set; }

        /// <summary>
        /// Opis oceny
        /// </summary>
        public string Description { get; set; }
    }
}
