using System.Collections.Generic;
using System.Linq;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Core
{
    /// <summary>
    /// Klasa odpowiedzialna za zarządznie kategoriami
    /// </summary>
    public class Categories
    {
        /// <summary>
        /// Znajdź z listy kategorię o numerze id: id
        /// </summary>
        /// <param name="id">
        /// identyfikator kategorii którą należy znaleźć
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich kategorii dostępnych na serwerze
        /// </param>
        /// <returns>
        /// Znalezioną kategorię
        /// </returns>
        public static Category GetCategory(int id, List<Category> categories)
        {
            return categories.SingleOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Stwórz nową kategorię
        /// </summary>
        /// <param name="name">
        /// Nazwa nowej kategorii
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich kategorii dostępnych na serwerze
        /// </param>
        /// <returns>
        /// Nowo utworzoną kategorię
        /// </returns>
        public static Category CreateCategory(string name, List<Category> categories)
        {
            return CategoryExists(name, categories) ? null : new Category()
            {
                Id = categories.Count,
                Name = name.ToUpper(),
                ProblemCovers = new List<ProblemCover>(),
                Questions = new List<Question>()
            };
        }

        /// <summary>
        /// Sprawdź, czy kategoria o takiej nazwie istnieje
        /// </summary>
        /// <param name="name">
        /// Nazwa nowej kategorii
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich kategorii dostępnych na serwerze
        /// </param>
        /// <returns>
        /// Czy kategoria istnieje
        /// </returns>
        private static bool CategoryExists(string name, List<Category> categories)
        {
            return categories.SingleOrDefault(c => c.Name == name.ToUpper()) != null;
        }
    }
}
