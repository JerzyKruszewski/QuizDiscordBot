using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using QuizDiscordBot.Core;
using QuizDiscordBot.Core.Objects;

namespace QuizDiscordBot.Modules
{
    /// <summary>
    /// Klasa z zarejestrwanymi komendami
    /// </summary>
    /// <remarks>
    /// Według konwencji komendy powinny być publicznymi asynchronicznymi zadaniami pomimo ostrzeżeń IDE
    /// </remarks>
    public class Commands : InteractiveBase<SocketCommandContext>
    {
        /// <summary>
        /// Komenda do pozostawienia oceny bota
        /// </summary>
        /// <param name="isPositive">
        /// Czy bot się podobał i spełnił swoje zadanie
        /// </param>
        /// <param name="description">
        /// Opis oceny
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// Schemat działania:
        /// 0. START - użytkownik wpisał komendę
        /// 1. Sprawdza czy ocena istnieje w bazie danych
        ///     1a. Jeśli nie:
        ///         1a.1. Tworzy nową ocenę z domyślnymi wartościami
        ///         1a.2. Dodaje ją do bazy danych ocen
        ///         1a.3. Zapisuje zmiany do bazy danych
        ///         1a.4. Przekazuje dalej dane
        ///     1b. Jeśli tak:
        ///         1b.1. Pobiera dane o ocene
        /// 2. Aktualizuje ocenę użytkownika
        /// 3. Zapisuje zmiany do bazy danych
        /// 4. KONIEC - sukces, pomyślnie oceniono pracę algorytmu
        /// </remarks>
        [Command("give feedback")]
        public async Task GiveFeedback(bool isPositive, [Remainder]string description)
        {
            // find user feedback
            UserFeedback userFeedback = UserFeedbacks.GetUserFeedback(Context.User.Id);

            // update suitable properties
            userFeedback.IsPositive = isPositive;
            userFeedback.Description = description;

            // save changes
            UserFeedbacks.Save();
        }

        /// <summary>
        /// Wyślij użytkownikowi opracowanie zagadnienia
        /// </summary>
        /// <param name="id">
        /// Id opracowania
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// Schemat działania:
        /// 0. START - użytkownik wpisał komendę
        /// 1. Sprawdza czy gildia istnieje w bazie danych
        ///     1a. Jeśli nie:
        ///         1a.1. Tworzy nową gildię
        ///         1a.2. Dodaje ją do bazy danych gildii
        ///         1a.3. Zapisuje zmiany do bazy danych
        ///         1a.4. Przekazuje dalej dane
        ///     1b. Jeśli tak:
        ///         1b.1. Pobiera dane o gildii
        /// 2. Pobiera dostępne w gildii kategorie
        /// 3. Wyszukuje opracowanie w kategoriach
        ///     3a. Jeśli nie ma takiego opracowania
        ///         3a.1. KONIEC - nie znaleziono opracowania
        /// 4. Tworzy sformatowaną wiadomość
        /// 5. Sprawdza czy opracowanie posiada informację o obrazku
        ///     5a. Jeśli tak:
        ///         5a.1. Dodaje do wiadomości obrazek
        /// 6. Wyświetla opracowanie
        /// 7. KONIEC - sukces, wyświetlono opracowanie
        /// </remarks>
        [Command("GetProblemCover")]
        [Alias("gpc")]
        public async Task GetProblemCover(int id)
        {
            ProblemCover problem = ProblemCovers.GetProblemCover(id, Guilds.GetGuild(Context.Guild.Id).Categories);

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = $"{problem.Id} - {problem.Title}",
                Description = problem.Description
            };

            if (problem.ImageURL != null)
            {
                embed.ImageUrl = problem.ImageURL;
            }

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        #region GetQuestion()
        /// <summary>
        /// Zadaj użytkownikowi pytanie
        /// </summary>
        /// <param name="id">
        /// numer identyfikacyjny pytania
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// jeśli użytkownik nie poda id pytania, bot podejmie próbę wylosowania pytania, którego użytkownik jeszcze nie widział.
        /// 
        /// Schemat działania:
        /// 0. START - użytkownik wpisał komendę
        /// 1. Sprawdza czy gildia istnieje w bazie danych
        ///     1a. Jeśli nie:
        ///         1a.1. Tworzy nową gildię
        ///         1a.2. Dodaje ją do bazy danych gildii
        ///         1a.3. Zapisuje zmiany do bazy danych
        ///         1a.4. Przekazuje dalej dane
        ///     1b. Jeśli tak:
        ///         1b.1. Pobiera dane o gildii
        /// 2. Sprawdza czy podany użytkownik istnieje w bazie danych
        ///     2a. Jeśli nie:
        ///         2a.1. Tworzy nowego użytkownika
        ///         2a.2. Dodaje go do bazy danych gildii
        ///         2a.3. Zapisuje zmiany do bazy danych
        ///         2a.4. Przekazuje dalej dane
        ///     2b. Jeśli tak:
        ///         2b.1. Pobiera dane o użytkowniku
        /// 3. Sprawdza czy użytkownik srecyzował które pytanie chce zobaczyć
        ///     3a. Jeśli tak:
        ///         3a.1. Sprawdza czy użytkownik widział to pytanie już wcześniej
        ///             3a.1a. Jeśli tak:
        ///                 3a.1a.1. Zapamiętuje tą informację
        ///         3a.2. Wyszykuje z bazy danych pytanie o podanym numerze identyfikacyjnym
        ///     3b. Jeśli nie:
        ///         3b.1. Sprawdza najwyższy dostępny numer Id pytania
        ///         3b.2. Pobiera wszystkie dostępne pytania
        ///         3b.3. Pobiera wszystkie pytania, których użytkownik wcześniej nie widział
        ///         3b.4. Sprawdza czy użytkownik widział wszystkie pytania
        ///             3b.4a. Jeśli tak:
        ///                 3b.4a.1. Zapamiętuje, że użytkownik widział pytanie
        ///                 3b.4a.2. Losuje pytanie z listy wszystkich dostępnych pytań
        ///             3b.4b. Jeśli nie:
        ///                 3b.4a.1. Losuje pytanie z listy pytań nie widzianych wcześniej przez użytkownika
        /// 4. Tworzy sformatowaną wiadomość
        /// 5. Dodaje do jej treści potrzebne możliwe odpowiedzi
        /// 6. Sprawdza czy pytanie posiada informację o obrazku
        ///     6a. Jeśli tak:
        ///         6a.1. Dodaje do wiadomości obrazek
        /// 7. Dodaje do właściwości wiadomości informacje czy ma się zakończyć po pierwszym użyciu, czy ma wspierać tylko jedną interakcję na użytkownika,
        ///    czas trwania oraz co zrobić po zakończeniu.
        /// 8. Dla każdej możliwej odpowiedzi: dodaje przycisk umożliwiający interakcję i informację co zrobić jak użytkownik wejdzie w interakcję z daną odpowiedzią
        ///    w zależności od tego czy odpowiedź jest poprawna
        /// 9. Wyświeta sformatowane pytanie
        /// 10. Oczekuje reakcji użytkownika
        ///     10a. Jeśli nadejdzie:
        ///         10a.1. Sprawdza z której kategorii pochodziło pytanie
        ///         10a.2. Sprawdza czy użytkownik widział pytanie wcześniej i czy udzielił poprawnej odpowiedzi
        ///             10a.2a. Jeśli widział wcześniej pytanie i odpowiedział poprawnie:
        ///                 10a.2a.1. Sprawdza czy użytkownik udzielił poprzednio na to pytanie błędnej odpowiedzi
        ///                     10a.2a.1a. Jeśli tak (poprawa):
        ///                         10a.2a.1a.1. Usuwa pytanie z listy pytań na które udzielił błędnej odpowiedzi
        ///                         10a.2a.1a.2. Dodaje użytkownikowi punkty za poprawną odpowiedź
        ///             10a.2b. Jeśli nie widział wcześniej pytania ale odpowiedział na nie poprawnie:
        ///                 10a.2b.1. Dodaje użytkownikowi punkty za poprawną odpowiedź
        ///             10a.2c. Jeśli nie widział wcześniej pytania i nie udzielił na nie poprawnej odpowiedzi:
        ///                 10a.2c.1. Dodaje pytanie do listy na które użytkownik nie udzielił poprawnej odpowiedzi
        ///         10a.3. Sprawdza czy użytkownik widział wcześniej to pytanie
        ///             10a.3a. Jeśli widzi je po raz pierwszy:
        ///                 10a.3a.1. Dodaje pytanie do widzianych wcześniej pytań
        ///         10a.4. Informuje użytkownika o wyniku
        /// 11. Po zakończeniu czasu zapisuje zmiany do bazy danych
        /// 12. Wysyła do użytkownika informacje o zakończeniu czasu
        /// 13. KONIEC - sukces, pomyślnie wyświetlono pytanie
        /// </remarks>
        [Command("GetQuestion", RunMode = RunMode.Async)]
        [Alias("gq")]
        public async Task GetQuestion(int id = -100)
        {
            // question which will be fetch from server database
            Question question = null;

            // maximum of server questions ids
            int maxId = 0;

            // if question was already seen
            bool wasSeen = false;

            // Get server informations
            Guild guild = Guilds.GetGuild(Context.Guild.Id);

            // Get user account
            UserAccount userAccount = UserAccounts.GetUserAccount(Context.User.Id, guild.UserAccounts, guild.Categories);

            // check if question id is valid
            if (id >= 0)
            {
                // check if user already seen this question
                if (userAccount.SeenQuestionsIds.Contains(id))
                {
                    wasSeen = true;
                }

                // get question with id from database
                question = Questions.GetQuestion(id, guild.Categories);
            }
            else
            {
                Random random = new Random(DateTime.Now.Millisecond);
                List<Question> allQuestions = new List<Question>();

                // Get maximum question id
                foreach (Category category in guild.Categories)
                {
                    maxId += category.Questions.Count;

                    allQuestions.AddRange(category.Questions); // and all questions as well
                }

                // get all unseen questions
                IEnumerable<Question> unseenQuestions = from q in allQuestions
                                                        where !userAccount.SeenQuestionsIds.Contains(q.Id)
                                                        select q;

                int unseenQuestionsCount = unseenQuestions.ToList().Count;

                // if list of unseen questions is empty
                if (unseenQuestionsCount == 0)
                {
                    // send random question
                    wasSeen = true;
                    question = allQuestions.ElementAt(random.Next(0, allQuestions.Count));
                }
                else
                {
                    // send unseen question
                    question = unseenQuestions.ElementAt(random.Next(0, unseenQuestionsCount));
                }

                id = question.Id;
            }

            // Create embed for message with question
            EmbedBuilder embedBuild = new EmbedBuilder
            {
                Title = $"Pytanie #{question.Id}",
                Description = question.Description
            };

            // add possible answers
            for (int i = 0; i < question.PossibleAnswers.Count; i++)
            {
                embedBuild.AddField(Util.AnswersEmojis.ToList()[i].Name, question.PossibleAnswers[i], true);
            }

            // add image
            if (question.ImageURL != null)
            {
                embedBuild.ImageUrl = question.ImageURL;
            }

            // build embed
            Embed embed = embedBuild.Build();

            // ReactionCallbackData(message content, embed, expires after first use, if command should react to only one answer from 1 user,
            //                                                                                          command timeout, what to do if command expires)
            ReactionCallbackData reactionData = new ReactionCallbackData("", embed, true, true, TimeSpan.FromSeconds(120), (c) => SaveChanges(c.Channel));

            // Iterate all possible answers and add answer placeholder
            for (int i = 0; i < question.PossibleAnswers.Count; i++)
            {
                // check if this iteration answer is right
                if (i == question.RightAnswer)
                {
                    // Add answer placecholder with information what if user select this answer
                    // reactionData.WithCallback(answer emoji, what to do if user select answer)
                    // CheckScore(if question was seen, is this good answer, question id, question category, user account, source channel)
                    reactionData.WithCallback(Util.AnswersEmojis.ToList()[i], (c, r) => CheckScore(wasSeen, true, id, GetCategoryContainingQuestionId(id, guild.Categories), userAccount, c.Channel));
                }
                else
                {
                    reactionData.WithCallback(Util.AnswersEmojis.ToList()[i], (c, r) => CheckScore(wasSeen, false, id, GetCategoryContainingQuestionId(id, guild.Categories), userAccount, c.Channel));
                }
            }

            // Send quiz message and await user selection
            await InlineReactionReplyAsync(reactionData, true);
        }

        /// <summary>
        /// Zapisz zmiany do bazy danych
        /// </summary>
        /// <param name="channel">
        /// Kanał na który należy wysłać informacje o zapisaniu zmian w bazie
        /// </param>
        /// <returns></returns>
        private static async Task SaveChanges(ISocketMessageChannel channel)
        {
            Guilds.Save();
            await channel.SendMessageAsync("Timed Out! Changes Commited To Database!");
        }

        /// <summary>
        /// Poinformuj o wyniku
        /// </summary>
        /// <param name="goodAnswer">
        /// Czy odpowiedź była prawidłowa
        /// </param>
        /// <param name="channel">
        /// Kanał na którym trzeba poinformować użytkownika o wyniku
        /// </param>
        private static async void NotifyAboutResult(bool goodAnswer, ISocketMessageChannel channel)
        {
            if (goodAnswer)
            {
                await channel.SendMessageAsync("Good Answer!");
            }
            else
            {
                await channel.SendMessageAsync("Bad Answer!");
            }
        }

        /// <summary>
        /// Sprawdź i oceń odpowiedź użytkownika
        /// </summary>
        /// <param name="wasSeen">
        /// Czy użytkownik widział to pytanie
        /// </param>
        /// <param name="goodAnswer">
        /// Czy użytkownik odpowiedział poprawnie na pytanie
        /// </param>
        /// <param name="id">
        /// Id pytania
        /// </param>
        /// <param name="categoryId">
        /// Id kategorii
        /// </param>
        /// <param name="userAccount">
        /// Konto użytkownika
        /// </param>
        /// <param name="channel">
        /// Kanał na który należy poinformować użytkownika o wyniku
        /// </param>
        /// <returns></returns>
        private static async Task CheckScore(bool wasSeen, bool goodAnswer, int id, int categoryId, UserAccount userAccount, ISocketMessageChannel channel)
        {
            if (wasSeen && goodAnswer)
            {
                // check if user has this question in wrongly answered questions
                if (userAccount.WrongAnswersQuestionIds.Contains(id))
                {
                    // remove this question from wrongly answered questions
                    userAccount.WrongAnswersQuestionIds.Remove(id);

                    // Add points to category scores
                    userAccount.CategoryComplition[categoryId]++;
                }
            }
            else if (goodAnswer) //if user haven't seen this question and answers correctly
            {
                // Add points to category scores
                userAccount.CategoryComplition[categoryId]++;
            }
            else if (!wasSeen) //if user haven't seen this question and answers wrongly
            {
                // Add question to wrongly answered questions
                userAccount.WrongAnswersQuestionIds.Add(id);
            }

            // check if user seen this question for the first time
            if (!wasSeen)
            {
                userAccount.SeenQuestionsIds.Add(id);
            }

            NotifyAboutResult(goodAnswer, channel);
        }

        /// <summary>
        /// Znajdź kategorie zawierającą pytanie
        /// </summary>
        /// <param name="id">
        /// Id pytania
        /// </param>
        /// <param name="categories">
        /// Kategorie obecne na serwerze
        /// </param>
        /// <returns>
        /// Id kategorii która zawiera pytanie
        /// </returns>
        private static int GetCategoryContainingQuestionId(int id, List<Category> categories)
        {
            return categories.SingleOrDefault(x => x.Questions.SingleOrDefault(y => y.Id == id) != null).Id;
        }
        #endregion

        /// <summary>
        /// Sprawdź postępu użytkownika
        /// </summary>
        /// <param name="user">
        /// Użytkownik discorda
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// Schemat działania:
        /// 0. START - użytkownik wpisał komendę
        /// 1. Sprawdza, czy użytkownik sprecyzował czyje postępy chce sprawdzić
        ///     1a. Jeśli nie:
        ///         1a.1. Ustawia wartość domyślną parametru user jako użytkownika, który wywołał komendę
        /// 2. Sprawdza czy gildia istnieje w bazie danych
        ///     2a. Jeśli nie:
        ///         2a.1. Tworzy nową gildię
        ///         2a.2. Dodaje ją do bazy danych gildii
        ///         2a.3. Zapisuje zmiany do bazy danych
        ///         2a.4. Przekazuje dalej dane
        ///     2b. Jeśli tak:
        ///         2b.1. Pobiera dane o gildii
        /// 3. Sprawdza czy podany użytkownik istnieje w bazie danych
        ///     3a. Jeśli nie:
        ///         3a.1. Tworzy nowego użytkownika
        ///         3a.2. Dodaje go do bazy danych gildii
        ///         3a.3. Zapisuje zmiany do bazy danych
        ///         3a.4. Przekazuje dalej dane
        ///     3b. Jeśli tak:
        ///         3b.1. Pobiera dane o użytkowniku
        /// 4. Tworzy sformatowaną wiadomość
        /// 5. Zapełnia wiadomość informacjami o postępach użytkownika
        /// 6. Wyświetla postępy użytkownika
        /// 7. KONIEC - sukces, pomyślnie wyświetlono postępy użytkownika
        /// </remarks>
        [Command("Status")]
        public async Task Status(SocketGuildUser user = null)
        {
            // check if user was specified
            if (user == null)
            {
                // if not initialize it with command sender user
                user = Context.User as SocketGuildUser;
            }

            // get server and user account informations
            Guild guild = Guilds.GetGuild(Context.Guild.Id);
            UserAccount userAccount = UserAccounts.GetUserAccount(user.Id, guild.UserAccounts, guild.Categories);

            // create embed
            EmbedBuilder embed = new EmbedBuilder
            {
                ThumbnailUrl = user.GetAvatarUrl(),
                Title = $"{user.Username}'s status:",
                Color = new Color(255, 255, 0)
            };

            // create embed fields with points
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>()
            {
                //global points
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = "Points: ",
                    Value = UserAccounts.CalculateTotalPointsOfUser(userAccount)
                }
            };

            // foreach category
            for (int i = 0; i < guild.Categories.Count; i++)
            {
                // add category points
                fields.Add(new EmbedFieldBuilder() 
                { 
                    IsInline = false, 
                    Name = guild.Categories[i].Name, 
                    Value = $"{100*(double)userAccount.CategoryComplition[i]/(double)guild.Categories[i].Questions.Count}%" 
                });
            }

            embed.Fields = fields;

            // send message to user
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
