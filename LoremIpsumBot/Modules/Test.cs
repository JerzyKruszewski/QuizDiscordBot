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
    public class Test : InteractiveBase<SocketCommandContext>
    {
        /// <summary>
        /// Komenda testowa
        /// </summary>
        /// <param name="expiresafteruse">
        /// Czy komenda ma przestać działać po 1. zaznaczeniu odpowiedzi
        /// </param>
        /// <param name="singleuseperuser">
        /// Czy komenda wspiera tylko 1 odpowiedź na 1 użytkownika
        /// </param>
        /// <param name="sourceuser">
        /// Czy komenda będzie reagować jedynie na użytkownika który ją wywołał
        /// </param>
        /// <returns></returns>
        [Command("quiz", RunMode = RunMode.Async)]
        public async Task Test_EmedReactionReply(bool expiresafteruse = true, bool singleuseperuser = true, bool sourceuser = true)
        {
            var one = new Emoji("1⃣");
            var two = new Emoji("2⃣");

            var embed = new EmbedBuilder()
                .WithTitle("2+2=?")
                .AddField(one.Name, "4", true)
                .AddField(two.Name, "0", true)
                .Build();

            await InlineReactionReplyAsync(new ReactionCallbackData("text", embed, expiresafteruse, singleuseperuser, TimeSpan.FromSeconds(20), (c) => c.Channel.SendMessageAsync("Timed Out!"))
                .WithCallback(one, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Dobra Odpowiedź!"))
                .WithCallback(two, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Matematyka się kłania.")), sourceuser
            );
        }

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
        /// Wymaga aby wywołujący komendę miał uprawnienia administratora na serwerze
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

            // check if user send a second message
            if (message != null)
            {
                // Create new category
                Category newCategory = Categories.CreateCategory(message.Content, guild.Categories);

                // Add category to server informations
                guild.Categories.Add(newCategory);

                // Save server informations with new category
                Guilds.Save();
            }
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

                SocketMessage categoryIdM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (categoryIdM != null)
                {
                    Category category = guild.Categories.FirstOrDefault(x => x.Id == Int32.Parse(categoryIdM.Content.Trim()));

                    if (category == null)
                    {
                        return;
                    }

                    await ReplyAsync("Podaj treść pytania:");

                    SocketMessage questionDescM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                    if (questionDescM != null)
                    {
                        string description = questionDescM.Content;

                        await ReplyAsync("Podaj możliwe odpowiedzi, oddziel je |");

                        SocketMessage possibleAnswM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                        if (possibleAnswM != null)
                        {
                            List<string> possibleAnswers = possibleAnswM.Content.Split('|').ToList();

                            int i = 0;
                            string message = "";

                            foreach (string pAns in possibleAnswers)
                            {
                                message += $"{i} - {pAns}\n";
                                i++;
                            }

                            await ReplyAsync($"{message}\n\nPodaj indeks prawidłowej odpowiedzi:");

                            SocketMessage rightAnswM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                            if (rightAnswM != null)
                            {
                                int rightAns = Int32.Parse(rightAnswM.Content.Trim());

                                if (rightAns < 0 || rightAns >= possibleAnswers.Count)
                                {
                                    await ReplyAsync("Niewłaściwy format. Wstrzymano dodawanie pytania.");
                                    return;
                                }

                                await ReplyAsync("Podaj URL zdjęcia do pytania (zignoruj tą wiadomość przez min, aby nic nie dodawać):");

                                string imageURL = null;

                                SocketMessage imageURLM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                                if (imageURLM != null)
                                {
                                    imageURL = imageURLM.Content;
                                }

                                Question newQuestion = Questions.CreateQuestion(description, rightAns, possibleAnswers, guild.Categories, imageURL);

                                category.Questions.Add(newQuestion);
                            }
                        }
                    }

                    Guilds.Save();
                }
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

                msg += "\nPodaj nazwę kategorii:";

                await Context.Channel.SendMessageAsync(msg);

                SocketMessage categoryNameM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                if (categoryNameM != null)
                {
                    Category category = guild.Categories.FirstOrDefault(x => x.Name == categoryNameM.Content.Trim().ToUpper());

                    if (category == null)
                    {
                        return;
                    }

                    await ReplyAsync("Podaj tytuł problemu:");

                    SocketMessage problemTitleM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                    if (problemTitleM != null)
                    {
                        string title = problemTitleM.Content;

                        await ReplyAsync("Podaj treść opracowania:");

                        SocketMessage problemDescM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                        if (problemDescM != null)
                        {
                            string description = problemDescM.Content;

                            await ReplyAsync("Podaj URL zdjęcia do opracowania (zignoruj tą wiadomość przez min, aby nic nie dodawać):");

                            string imageURL = null;

                            SocketMessage imageURLM = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));

                            if (imageURLM != null)
                            {
                                imageURL = imageURLM.Content;
                            }

                            ProblemCover problem = ProblemCovers.CreateProblemCover(title, description, guild.Categories, imageURL);

                            category.ProblemCovers.Add(problem);
                        }
                    }

                    Guilds.Save();
                }
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
