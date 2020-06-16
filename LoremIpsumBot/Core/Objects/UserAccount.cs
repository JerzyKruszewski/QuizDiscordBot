using System.Collections.Generic;

namespace QuizDiscordBot.Core.Objects
{
    /// <summary>
    /// Klasa reprezentuje konto użytkownika
    /// </summary>
    /// <remarks>
    /// Konto użytkownika jest zależne od serwera z jakiego korzysta
    /// </remarks>
    public class UserAccount
    {
        /// <summary>
        /// Discordowy identyfikator użytkownika
        /// </summary>
        public ulong UserId { get; set; }

        /*/// <summary>
        /// Całkowita liczba punktów
        /// </summary>
        /// <remarks>
        /// Własność można obliczyć z CategoryComplition, jednak została zostawiona, żeby nie niszyczyć obecnego kodu.
        /// </remarks>
        public int TotalPoints { get; set; }*/

        /// <summary>
        /// Lista id pytań, które użytkownik już widział.
        /// </summary>
        /// <remarks>
        /// Używane przy próbie znalezienia niewidzianego wcześniej pytania oraz popraw wyniku.
        /// </remarks>
        public List<int> SeenQuestionsIds { get; set; }

        /// <summary>
        /// Lista id pytań na które użytkownik odpowiedział nieprawidłowo.
        /// </summary>
        /// <remarks>
        /// Używane przy poprawach wyniku.
        /// </remarks>
        public List<int> WrongAnswersQuestionIds { get; set; }

        /// <summary>
        /// Ilość poprawnych odpowiedzi ze względu na kategorie z której pochodziły pytania.
        /// </summary>
        public List<int> CategoryComplition { get; set; }
    }
}
