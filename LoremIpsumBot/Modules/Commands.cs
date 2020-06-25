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
        /// Stwórz kategorię dla pytań i problemów
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Wymaga aby wywołujący komendę miał uprawnienia administratora na serwerze (gildii)
        /// 
        /// Schemat działania:
        /// 0. START - użytkownik wpisał komendę
        /// 1. Sprawdza czy użytkownik jest uprawniony do wywołania komendy
        ///     1a. Jeśli jest:
        ///         1a.1. Sprawdza czy gildia istnieje w bazie danych
        ///             1a.1a. Jeśli nie:
        ///                 1a.1a.1. Tworzy nową gildię
        ///                 1a.1a.2. Dodaje ją do bazy danych gildii
        ///                 1a.1a.3. Zapisuje zmiany do bazy danych
        ///                 1a.1a.4. Przekazuje dalej dane
        ///             1a.1b. Jeśli tak:
        ///                 1a.1b.1. Pobiera dane o gildii
        ///         1a.2. Pobiera wszystkie dosyępne w gildii kategorie
        ///         1a.3. Wysyła do użytkownika informacje o istniejących kategoriach
        ///         1a.4. Oczekuje na reakcję użytkownika z nazwą nowej kategorii
        ///             1a.4a. Jeśli nie nadejdzie:
        ///                 1a.4a.1. Poinformuje użytkownika o błędzie
        ///                 1a.4a.2. Wstrzymuje proces
        ///                 1a.4a.3. KONIEC - algorytm nie otrzymał wymaganej informacji od użytkownika
        ///             1a.4b. Jeśli nadejdzie:
        ///                 1a.4b.1. Sprawdzi czy kategoria o podanej przez użytkownika nazwie istnieje
        ///                     1a.4b.1a. Jeśli istnieje:
        ///                         1a.4b.1a.1. Poinformuje użytkownika o błędzie
        ///                         1a.4b.1a.2. Wstrzymuje proces
        ///                         1a.4b.1a.3. KONIEC - kategoria już istnieje
        ///                     1a.4b.1b. Jeśli nie istnieje:
        ///                         1a.4b.1b.1. Stworzy nową kategorię
        ///                         1a.4b.1b.2. Dodaje kategorię do bazy danych
        ///                         1a.4b.1b.3. Zapisuje zmiany do bazy danych
        ///                         1a.4b.1b.4. KONIEC - sukces, kategoria została dodana
        ///     1b. Jeśli nie jest:
        ///         1b.1. Wypisuje informacje o nadużyciu
        ///         1b.2. KONIEC - użytkownik nie uprawniony
        /// </remarks>
        [Command("CreateCategory", RunMode = RunMode.Async)]
        [Alias("cc")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CreateCategory()
        {
            // Get server information (questions, user accounts, etc.)
            Guild guild = Guilds.GetGuild(Context.Guild.Id);

            // Get all existing categories
            string msg = "Guild Category List:\n";

            foreach (Category category in guild.Categories)
            {
                msg += $"{category.Id} - {category.Name} ({category.ProblemCovers.Count}/{category.Questions.Count})\n";
            }
            
            msg += "\nPodaj nazwę nowej kategorii:";
            
            // Send return message
            await Context.Channel.SendMessageAsync(msg);

            // Wait 2 min for message from source user with category name
            SocketMessage message = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

            // check if user send next message
            if (message == null)
            {
                await Context.Channel.SendMessageAsync("Wstrzymano dodawanie kategorii.");
                return;
            }

            // Create new category
            Category newCategory = Categories.CreateCategory(message.Content, guild.Categories);

            if (newCategory == null)
            {
                await Context.Channel.SendMessageAsync("Kategoria istnieje. Wstrzymano dodawanie kategorii.");
                return;
            }

            // Add category to server informations
            guild.Categories.Add(newCategory);

            // Save server informations with new category
            Guilds.Save();
        }

        /// <summary>
        /// Stwórz pytanie
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Wymaga aby wywołujący komendę miał uprawnienia administratora na serwerze
        /// </remarks>
        [Command("CreateQuestion", RunMode = RunMode.Async)]
        [Alias("cq")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CreateQuestion()
        {
            try
            {
                Guild guild = Guilds.GetGuild(Context.Guild.Id);

                string msg = "Guild Category List:\n";

                foreach (Category category in guild.Categories)
                {
                    msg += $"{category.Id} - {category.Name} ({category.ProblemCovers.Count}/{category.Questions.Count})\n";
                }

                msg += "\nPodaj indeks kategorii:";

                await Context.Channel.SendMessageAsync(msg);

                SocketMessage massageWithCategoryId = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (massageWithCategoryId == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                Category desiredCategory = guild.Categories.SingleOrDefault(x => x.Id == Int32.Parse(massageWithCategoryId.Content.Trim()));

                if (desiredCategory == null)
                {
                    await Context.Channel.SendMessageAsync("Nie znaleziono kategorii. Wstrzymano dodawanie pytania.");
                    return;
                }

                await ReplyAsync("Podaj treść pytania:");

                SocketMessage messageWithQuestionDescription = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithQuestionDescription == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                string description = messageWithQuestionDescription.Content;

                await ReplyAsync("Podaj możliwe odpowiedzi, oddziel je |");

                SocketMessage messageWithAllPossibleAnswers = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithAllPossibleAnswers == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                List<string> allPossibleAnswers = messageWithAllPossibleAnswers.Content.Split('|').ToList();

                int i = 0;
                string message = "";

                foreach (string possibleAnswer in allPossibleAnswers)
                {
                    message += $"{i} - {possibleAnswer}\n";
                    i++;
                }

                await ReplyAsync($"{message}\n\nPodaj indeks prawidłowej odpowiedzi:");

                SocketMessage messageWithRightAnswer = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithRightAnswer == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                int rightAnswer = Int32.Parse(messageWithRightAnswer.Content.Trim());

                if (rightAnswer < 0 || rightAnswer >= allPossibleAnswers.Count)
                {
                    await ReplyAsync("Niewłaściwy format. Wstrzymano dodawanie pytania.");
                    return;
                }

                await ReplyAsync("Podaj URL zdjęcia do pytania (zignoruj tą wiadomość przez min, aby nic nie dodawać):");

                string imageURL = null;

                SocketMessage messageWithImageUrl = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithImageUrl != null)
                {
                    imageURL = messageWithImageUrl.Content;
                }

                Question newQuestion = Questions.CreateQuestion(description, rightAnswer, allPossibleAnswers, guild.Categories, imageURL);

                desiredCategory.Questions.Add(newQuestion);

                Guilds.Save();
            }
            catch (Exception)
            {
                await ReplyAsync("Niewłaściwy format. Wstrzymano dodawanie pytania.");
                return;
            }
        }

        /// <summary>
        /// Stwórz opracowanie dla problemu
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Wymaga aby wywołujący komendę miał uprawnienia administratora na serwerze
        /// </remarks>
        [Command("CreateProblemCover", RunMode = RunMode.Async)]
        [Alias("cpc")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CreateProblemCover()
        {
            try
            {
                Guild guild = Guilds.GetGuild(Context.Guild.Id);

                string msg = "Guild Category List:\n";

                foreach (Category category in guild.Categories)
                {
                    msg += $"{category.Id} - {category.Name} ({category.ProblemCovers.Count}/{category.Questions.Count})\n";
                }

                msg += "\nPodaj indeks kategorii:";

                await Context.Channel.SendMessageAsync(msg);

                SocketMessage massageWithCategoryId = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (massageWithCategoryId == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                Category desiredCategory = guild.Categories.SingleOrDefault(x => x.Id == Int32.Parse(massageWithCategoryId.Content.Trim()));

                if (desiredCategory == null)
                {
                    await Context.Channel.SendMessageAsync("Nie znaleziono kategorii. Wstrzymano dodawanie pytania.");
                    return;
                }

                await ReplyAsync("Podaj tytuł problemu:");

                SocketMessage messageWithProblemTitle = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithProblemTitle == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                string title = messageWithProblemTitle.Content;

                await ReplyAsync("Podaj treść opracowania:");

                SocketMessage messageWithProblemDescription = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithProblemDescription == null)
                {
                    await Context.Channel.SendMessageAsync("Nie otrzymano informacji zwrotnej. Wstrzymano dodawanie pytania.");
                    return;
                }

                string description = messageWithProblemDescription.Content;

                await ReplyAsync("Podaj URL zdjęcia do opracowania (zignoruj tą wiadomość przez min, aby nic nie dodawać):");

                string imageURL = null;

                SocketMessage messageWithImageUrl = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (messageWithImageUrl != null)
                {
                    imageURL = messageWithImageUrl.Content;
                }

                ProblemCover problem = ProblemCovers.CreateProblemCover(title, description, guild.Categories, imageURL);

                desiredCategory.ProblemCovers.Add(problem);

                Guilds.Save();
            }
            catch (Exception)
            {
                await ReplyAsync("Niewłaściwy format. Wstrzymano dodawanie pytania.");
                return;
            }
        }

        /// <summary>
        /// Wyślij użytkownikowi opracowanie zagadnienia
        /// </summary>
        /// <param name="id">
        /// Id opracowania
        /// </param>
        /// <returns></returns>
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

        /// <summary>
        /// Zadaj użytkownikowi pytanie
        /// </summary>
        /// <param name="id">
        /// numer pytania
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// jeśli użytkownik nie poda id pytania, bot podejmie próbę wylosowania pytania, którego użytkownik jeszcze nie widział.
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

        /// <summary>
        /// Sprawdź postępu użytkownika
        /// </summary>
        /// <param name="user">
        /// Użytkownik discorda
        /// </param>
        /// <returns></returns>
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
                fields.Add(new EmbedFieldBuilder() { IsInline = false, Name = guild.Categories[i].Name, Value = $"{100*(double)userAccount.CategoryComplition[i]/(double)guild.Categories[i].Questions.Count}%" });
            }

            embed.Fields = fields;

            // send message to user
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
