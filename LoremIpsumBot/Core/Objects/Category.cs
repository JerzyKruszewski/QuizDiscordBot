using System.Collections.Generic;

namespace QuizDiscordBot.Core.Objects
{
    /// <summary>
    /// Klasa reprezentuje kategorie pytań i opracowań dostępnych na serwerze
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Numer identyfikacyjny kategorii
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nazwa kategorii
        /// </summary>
        /// <remarks>
        /// Unikalna nazwa
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Lista opracowań problemów dla tej kategorii
        /// </summary>
        public List<ProblemCover> ProblemCovers { get; set; }

        /// <summary>
        /// Lista pytań dostępnych dla tej kategorii
        /// </summary>
        public List<Question> Questions { get; set; }
    }
}
