using System.Collections.Generic;

namespace QuizDiscordBot.Core.Objects
{
    /// <summary>
    /// Klasa reprezentująca pytania
    /// </summary>
    public class Question
    {
        /// <summary>
        /// identyfikator pytania
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Opis pytania
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Lista możliwych odpowiedzi na pytanie
        /// </summary>
        /// <remarks>
        /// Maksymalnie 20.
        /// Lista wszystkich możliwych dostępna jest pod: LoremIpsumBot.Util (odpowiednia funkcja konwertuje string na obiekt Emoji)
        /// </remarks>
        public List<string> PossibleAnswers { get; set; }

        /// <summary>
        /// Indeks prawidłowej odpowiedzi do pytania
        /// </summary>
        public int RightAnswer { get; set; }

        /// <summary>
        /// Ewentualny link do obrazka obrazującego problem w pytaniu
        /// </summary>
        public string ImageURL { get; set; }
    }
}
