using System.Collections.Generic;
using System.Linq;
using QuizDiscordBot.Storage;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Core
{
    /// <summary>
    /// Klasa odpowiedzialna za zarządzanie danymi o serwerze.
    /// </summary>
    public class Guilds
    {
        /// <summary>
        /// Ścieżka do pliku z bazą danych
        /// </summary>
        private const string FilePath = "./Resources/Guild";

        /// <summary>
        /// Lista wszystkich serwerów obsługiwanych przez bota
        /// </summary>
        private static readonly List<Guild> _guilds;

        /// <summary>
        /// Obiekt umożliwiający zapis i odczyt danych
        /// </summary>
        private static readonly IStorage _storage;

        /// <summary>
        /// Konstruktor
        /// </summary>
        static Guilds()
        {
            _storage = new JsonStorage();

            // if file exist
            if (_storage.FileExist(FilePath))
            {
                // load all the data
                _guilds = _storage.RestoreObject<List<Guild>>(FilePath);
            }
            else // if not
            {
                // initialize data
                _guilds = new List<Guild>();
                Save();
            }
        }

        /// <summary>
        /// Zapisz dane o serwerach w bazie
        /// </summary>
        public static void Save()
        {
            _storage.StoreObject(_guilds, FilePath);
        }

        /// <summary>
        /// Znajdź dane o serwerze
        /// </summary>
        /// <param name="id">
        /// Discordowe id serwera
        /// </param>
        /// <returns>
        /// Obiekt zawierający wszystkie potrzebne dane o serwerze
        /// </returns>
        public static Guild GetGuild(ulong id)
        {
            return GetOrCreateGuild(id);
        }

        /// <summary>
        /// Znajdź lub stwórz (jeśli nie istnieje) obiekt z danymi o serwerze
        /// </summary>
        /// <param name="id">
        /// Discordowe id serwera
        /// </param>
        /// <returns>
        /// Obiekt zawierający wszystkie potrzebne dane o serwerze
        /// </returns>
        private static Guild GetOrCreateGuild(ulong id)
        {
            Guild guild = _guilds.SingleOrDefault(g => g.Id == id);

            if (guild == null)
            {
                return CreateGuild(id);
            }

            return guild;
        }

        /// <summary>
        /// Stwórz obiekt z danymi o serwerze
        /// </summary>
        /// <param name="id">
        /// Discordowe id serwera
        /// </param>
        /// <returns>
        /// Nowo utworzony obiekt zawierający wszystkie potrzebne dane o serwerze
        /// </returns>
        private static Guild CreateGuild(ulong id)
        {
            Guild guild = new Guild()
            {
                Id = id,
                Categories = new List<Category>(),
                UserAccounts = new List<UserAccount>()
            };

            _guilds.Add(guild);
            Save();

            return guild;
        }
    }
}
