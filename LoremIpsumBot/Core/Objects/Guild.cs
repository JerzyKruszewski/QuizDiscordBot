using System.Collections.Generic;

namespace QuizDiscordBot.Core.Objects
{
    /// <summary>
    /// Klasa reprezentująca serwer discorda (gildię). Zawierająca listę kategorii oraz kont użytkowników.
    /// </summary>
    public class Guild
    {
        /// <summary>
        /// Discordowy identyfikator serwera
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Lista kategorii dostępnych na serwerze
        /// </summary>
        public List<Category> Categories { get; set; }

        /// <summary>
        /// Lista kont użytkowników
        /// </summary>
        public List<UserAccount> UserAccounts { get; set; }
    }
}
