namespace QuizDiscordBot
{
    /// <summary>
    /// Struktura przechowująca dane o bocie
    /// </summary>
    internal struct BotConfig
    {
        /// <summary>
        /// Token do autoryzacji zapytań API
        /// </summary>
        public string token;

        /// <summary>
        /// Prefix do komendy
        /// </summary>
        public string cmdPrefix;

        /// <summary>
        /// Discordowe Id właściciela
        /// </summary>
        /// <remarks>
        /// Nieużyte 
        /// </remarks>
        public ulong ownerID;
    }
}
