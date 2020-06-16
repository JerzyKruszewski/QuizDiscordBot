using System.Collections.Generic;
using System.Linq;
using QuizDiscordBot.Storage;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Core
{
    /// <summary>
    /// Klasa odpowiedzialna za zarządzanie ocanami użytkowników
    /// </summary>
    public class UserFeedbacks
    {
        /// <summary>
        /// Ścieżka do pliku z bazą danych
        /// </summary>
        private const string FilePath = "./Resources/UserFeedback";

        /// <summary>
        /// Lista wszystkich ocen użytkowników
        /// </summary>
        private static readonly List<UserFeedback> _userFeedbacks;

        /// <summary>
        /// Obiekt umożliwiający zapis i odczyt danych
        /// </summary>
        private static readonly IStorage _storage;

        /// <summary>
        /// Konstruktor
        /// </summary>
        static UserFeedbacks()
        {
            _storage = new JsonStorage();

            // if file exist
            if (_storage.FileExist(FilePath))
            {
                // load the data
                _userFeedbacks = _storage.RestoreObject<List<UserFeedback>>(FilePath);
            }
            else // if not
            {
                // Initialize it
                _userFeedbacks = new List<UserFeedback>();
                // and save
                Save();
            }
        }

        /// <summary>
        /// Zapisz dane o ocenach w bazie
        /// </summary>
        public static void Save()
        {
            // save user feedback in file
            _storage.StoreObject(_userFeedbacks, FilePath);
        }

        /// <summary>
        /// Znajdź ocenę użytkoenika
        /// </summary>
        /// <param name="userId">
        /// Discordowe id autora
        /// </param>
        /// <returns>
        /// Znalezioną ocenę użytkownika
        /// </returns>
        public static UserFeedback GetUserFeedback(ulong userId)
        {
            return GetOrCreateUserFeedback(userId);
        }

        /// <summary>
        /// Znajdź lub stwórz ocenę użytkownika
        /// </summary>
        /// <param name="userId">
        /// Discordowe id autora
        /// </param>
        /// <returns>
        /// Znalezioną ocenę użytkownika
        /// </returns>
        private static UserFeedback GetOrCreateUserFeedback(ulong userId)
        {
            // try to find it
            UserFeedback feedback = _userFeedbacks.SingleOrDefault(f => f.UserId == userId);

            // if not found
            if (feedback == null)
            {
                // create new
                return CreateUserFeedback(userId);
            }

            return feedback;
        }

        /// <summary>
        /// Stwórz nową ocenę
        /// </summary>
        /// <param name="userId">
        /// Discordowe id autora
        /// </param>
        /// <returns>
        /// Nowo stworzoną ocenę użytkowika.
        /// </returns>
        private static UserFeedback CreateUserFeedback(ulong userId)
        {
            // Create new UserFeedback object
            UserFeedback feedback = new UserFeedback()
            {
                UserId = userId,
                IsPositive = false,
                Description = "Lorem Ipsum"
            };

            // Add it to database
            _userFeedbacks.Add(feedback);
            // Save the changes
            Save();

            return feedback;
        }
    }
}
