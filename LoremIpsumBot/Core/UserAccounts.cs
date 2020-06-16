using System.Collections.Generic;
using System.Linq;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Core
{
    /// <summary>
    /// Klasa odpowiedzialna za zarządzanie kontami użytkowników
    /// </summary>
    public class UserAccounts
    {
        /// <summary>
        /// Znajdź konto użytkownika
        /// </summary>
        /// <param name="id">
        /// Discordowy identyfikator użytkownika
        /// </param>
        /// <param name="accounts">
        /// Lista wszystkich kont użytkowników dostępnych na serwerze
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich dostępnych na serwerze kategorii
        /// </param>
        /// <returns>
        /// Znalezione konto użytkownika
        /// </returns>
        public static UserAccount GetUserAccount(ulong id, List<UserAccount> accounts, List<Category> categories)
        {
            return GetOrCreateUserAccount(id, accounts, categories);
        }

        /// <summary>
        /// Znajdź lub stwórz konto użytkownika
        /// </summary>
        /// <param name="id">
        /// Discordowy identyfikator użytkownika
        /// </param>
        /// <param name="accounts">
        /// Lista wszystkich kont użytkowników dostępnych na serwerze
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich dostępnych na serwerze kategorii
        /// </param>
        /// <returns>
        /// Znalezione konto użytkownika
        /// </returns>
        private static UserAccount GetOrCreateUserAccount(ulong id, List<UserAccount> accounts, List<Category> categories)
        {
            // find
            UserAccount userAccount = accounts.SingleOrDefault(a => a.UserId == id);

            // if not found
            if (userAccount == null)
            {
                // create new
                userAccount = CreateUserAccount(id, accounts, categories);
            }

            return userAccount;
        }

        /// <summary>
        /// Stwórz konto użytkownika
        /// </summary>
        /// <param name="id">
        /// Discordowy identyfikator użytkownika
        /// </param>
        /// <param name="accounts">
        /// Lista wszystkich kont użytkowników dostępnych na serwerze
        /// </param>
        /// <param name="categories">
        /// Lista wszystkich dostępnych na serwerze kategorii
        /// </param>
        /// <returns>
        /// Nowo utworzone konto użytkownika
        /// </returns>
        private static UserAccount CreateUserAccount(ulong id, List<UserAccount> accounts, List<Category> categories)
        {
            // Initialize category complition
            List<int> categoryComplition = new List<int>();

            // Iterate over all categories
            foreach (Category item in categories)
            {
                // and populate categoryComplition with 0
                categoryComplition.Add(0);
            }

            // Create new user account
            UserAccount user = new UserAccount()
            {
                UserId = id,
                SeenQuestionsIds = new List<int>(),
                WrongAnswersQuestionIds = new List<int>(),
                CategoryComplition = categoryComplition
            };

            // add it to all users accounts list
            accounts.Add(user);
            // save all data
            Guilds.Save();

            // return newly created user
            return user;
        }

        /// <summary>
        /// Podlicz wszystkie punkty z kategorii
        /// </summary>
        /// <param name="user">
        /// Konto użytkownika
        /// </param>
        /// <returns>
        /// Wszystkie punkty zdobyte przez użytkownika
        /// </returns>
        public static int CalculateTotalPointsOfUser(UserAccount user)
        {
            // return sum of all user points
            return user.CategoryComplition.Sum();
        }
    }
}
