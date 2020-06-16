using System.Collections.Generic;
using System.Linq;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Core
{
    /// <summary>
    /// Klasa odpowiedzialna za zarządznie pytaniami
    /// </summary>
    public class Questions
    {
        /// <summary>
        /// Znajdź pytanie
        /// </summary>
        /// <param name="id">
        /// Id pytania do znalezienia
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich kategorii dostępnych na serwerze.
        /// </param>
        /// <returns>
        /// Znalezione pytanie
        /// </returns>
        /// <remarks>
        /// Może zwracać null jeśli nie zostanie znalezione
        /// </remarks>
        public static Question GetQuestion(int id, List<Category> categories)
        {
            // initialize question with default "value"
            Question question = null;

            // iterate over each category
            foreach (Category category in categories)
            {
                // try to find a question with id: id
                question = category.Questions.SingleOrDefault(q => q.Id == id);

                // if successfully found
                if (question != null)
                {
                    // break the loop
                    break;
                }
            }

            return question;
        }

        /// <summary>
        /// Stwórz nowe pytanie
        /// </summary>
        /// <param name="description">
        /// Treść pytanie
        /// </param>
        /// <param name="rightAnswer">
        /// Indeks poprawnej odpowiedzi
        /// </param>
        /// <param name="possibleAnswers">
        /// Lista możliwych odpowiedzi
        /// </param>
        /// <param name="categories">
        /// Wszystkie kategorie na serwerze
        /// </param>
        /// <param name="imageURL">
        /// Ewentualny link do zdjęcia obrazującego pytanie
        /// </param>
        /// <returns>
        /// Nowo stworzone pytanie
        /// </returns>
        public static Question CreateQuestion(string description, int rightAnswer, List<string> possibleAnswers, List<Category> categories, string imageURL = null)
        {
            // initialize id with default value
            int id = 0;

            // iterate over all categories to find next suitable question id
            foreach (Category category in categories)
            {
                id += category.Questions.Count;
            }

            // return new Question object
            return new Question()
            {
                Id = id,
                PossibleAnswers = possibleAnswers,
                RightAnswer = rightAnswer,
                Description = description,
                ImageURL = imageURL
            };
        }
    }
}
