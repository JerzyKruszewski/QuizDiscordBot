namespace QuizDiscordBot.Storage
{
    /// <summary>
    /// Interface do obsługi zapisów i odczytów
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Wczytaj obiekt typu T.
        /// </summary>
        /// <typeparam name="T">
        /// Typ obiektu wczytywanego z pliku
        /// </typeparam>
        /// <param name="filePath">
        /// Ścieżka do pliku
        /// </param>
        /// <returns>
        /// Obiekt typu T z pliku pod ścieżką filepath
        /// </returns>
        public T RestoreObject<T>(string filePath);

        /// <summary>
        /// Zapisz obiekt w pliku
        /// </summary>
        /// <param name="obj">
        /// Obiekt do zapisania
        /// </param>
        /// <param name="filePath">
        /// Ścieżka do pliku
        /// </param>
        public void StoreObject(object obj, string filePath);

        /// <summary>
        /// Sprawdź czy istnieje plik pod ścieżką filepath
        /// </summary>
        /// <param name="filePath">
        /// Ścieżka do pliku
        /// </param>
        /// <returns>
        /// true - jeśli istnieje i vice versa
        /// </returns>
        public bool FileExist(string filePath);
    }
}
