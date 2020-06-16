using System.Collections.Generic;
using System.Linq;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Core
{
    /// <summary>
    /// Klasa odpowiedzialna za zarządzanie opracowaniami problemów
    /// </summary>
    public class ProblemCovers
    {
        /// <summary>
        /// Znajdź opracowanie problemu.
        /// </summary>
        /// <param name="id">
        /// Id opracowania, które należy znaleźć.
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich dostępnych na serwerze kategorii
        /// </param>
        /// <returns>
        /// Znalezione opracowanie problemu
        /// </returns>
        /// <remarks>
        /// Może zwrócić null jeśli nie znajdzie opracowania.
        /// </remarks>
        public static ProblemCover GetProblemCover(int id, List<Category> categories)
        {
            // initialize with null "value"
            ProblemCover problem = null; 

            // iterate over all categories
            foreach (Category category in categories)
            {
                // try to find problem with id in this category
                problem = category.ProblemCovers.SingleOrDefault(p => p.Id == id);

                // if successfully found
                if (problem != null)
                {
                    // break the loop
                    break;
                }
            }

            // return problem cover
            return problem;
        }

        /// <summary>
        /// Stwórz nowe opracowanie
        /// </summary>
        /// <param name="title">
        /// Tytuł opracowania
        /// </param>
        /// <param name="description">
        /// Opis opracowania
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich dostępnych na serwerze kategorii
        /// </param>
        /// <param name="imageURL">
        /// Ewentualny link do zdjęcia obrazującego problem
        /// </param>
        /// <returns>
        /// Stworzone opacowanie
        /// </returns>
        public static ProblemCover CreateProblemCover(string title, string description, List<Category> categories, string imageURL = null)
        {
            // initialize id number
            int id = 0;

            // iterate over categories to find next valid problem cover id
            foreach (Category category in categories)
            {
                id += category.ProblemCovers.Count;
            }

            // return newly created ProblemCover object
            return new ProblemCover()
            {
                Id = id,
                Title = title,
                Description = description,
                ImageURL = imageURL
            };
        }
    }
}
