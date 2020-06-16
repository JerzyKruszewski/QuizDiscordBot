namespace QuizDiscordBot.Core.Objects
{
    /// <summary>
    /// Klasa reprezentująca opracowanie problemu.
    /// </summary>
    public class ProblemCover
    {
        /// <summary>
        /// Identyfikator opracowania
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tytuł opracowania
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Opis opracowania
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ewentualny link do zdjęcia oprazującego problem
        /// </summary>
        public string ImageURL { get; set; }
    }
}
