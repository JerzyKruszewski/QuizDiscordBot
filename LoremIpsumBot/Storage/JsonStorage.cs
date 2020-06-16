using System.IO;
using Newtonsoft.Json;

namespace QuizDiscordBot.Storage
{
    /// <summary>
    /// Klasa do obsługi zapisów i odczytów w plikach .json
    /// </summary>
    public class JsonStorage : IStorage
    {
        /// <summary>
        /// Wczytaj obiekt typu T z pliku JSON.
        /// </summary>
        /// <typeparam name="T">
        /// Typ obiektu wczytywanego z pliku
        /// </typeparam>
        /// <param name="filePath">
        /// Ścieżka do pliku bez rozszerzenia
        /// </param>
        /// <returns>
        /// Obiekt typu T z pliku pod ścieżką filepath
        /// </returns>
        /// <remarks>
        /// Rozszerzenie .json należy pominąć przy wywoływaniu metody
        /// </remarks>
        public T RestoreObject<T>(string filepath)
        {
            string json = File.ReadAllText($"{filepath}.json");
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Zapisz obiekt w pliku
        /// </summary>
        /// <param name="obj">
        /// Obiekt do zapisania
        /// </param>
        /// <param name="filePath">
        /// Ścieżka do pliku bez rozszerzenia
        /// </param>
        /// <remarks>
        /// Rozszerzenie .json należy pominąć przy wywoływaniu metody
        /// </remarks>
        public void StoreObject(object obj, string filepath)
        {
            string file = $"{filepath}.json";

            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            File.WriteAllText(file, json);
        }

        /// <summary>
        /// Sprawdź czy istnieje plik pod ścieżką filepath
        /// </summary>
        /// <param name="filePath">
        /// Ścieżka do pliku bez rozszerzenia
        /// </param>
        /// <returns>
        /// true - jeśli istnieje i vice versa
        /// </returns>
        /// <remarks>
        /// Rozszerzenie .json należy pominąć przy wywoływaniu metody
        /// </remarks>
        public bool FileExist(string filepath)
        {
            return File.Exists($"{filepath}.json");
        }
    }
}
