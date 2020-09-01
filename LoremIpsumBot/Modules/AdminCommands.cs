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
    /// Komendy dla osób uprawnionych do zarządzania algorytmem
    /// </summary>
    public class AdminCommands : InteractiveBase<SocketCommandContext>
    {
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
        ///         1a.2. Pobiera wszystkie dostępne w gildii kategorie
        ///         1a.3. Wysyła do użytkownika informacje o istniejących kategoriach
        ///         1a.4. Oczekuje na reakcję użytkownika z nazwą nowej kategorii
        ///             1a.4a. Jeśli nie nadejdzie:
        ///                 1a.4a.1. Poinformuje użytkownika o błędzie
        ///                 1a.4a.2. Wstrzymuje proces
        ///                 1a.4a.3. KONIEC - algorytm nie otrzymał wymaganej informacji od użytkownika
        ///         1a.5. Sprawdzi czy kategoria o podanej przez użytkownika nazwie istnieje
        ///              1a.5a. Jeśli istnieje:
        ///                 1a.5a.1. Poinformuje użytkownika o błędzie
        ///                 1a.5a.2. Wstrzymuje proces
        ///                 1a.5a.3. KONIEC - kategoria już istnieje
        ///         1a.7. Stworzy nową kategorię
        ///         1a.8. Dodaje kategorię do bazy danych
        ///         1a.9. Zapisuje zmiany do bazy danych
        ///         1a.10. KONIEC - sukces, kategoria została dodana
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

            await Context.Channel.SendMessageAsync("Dodano kategorię.");
        }

        /// <summary>
        /// Stwórz pytanie
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Wymaga aby wywołujący komendę miał uprawnienia administratora na serwerze
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
        ///         1a.2. Pobiera wszystkie dostępne w gildii kategorie
        ///         1a.3. Wysyła do użytkownika informacje o istniejących kategoriach
        ///         1a.4. Oczekuje na reakcję użytkownika z indeksem kategorii
        ///             1a.4a. Jeśli nie nadejdzie lub będzie miało nieprawidłowy format:
        ///                 1a.4a.1. Poinformuje użytkownika o błędzie
        ///                 1a.4a.2. Wstrzymuje proces
        ///                 1a.4a.3. KONIEC - algorytm nie otrzymał wymaganej informacji od użytkownika lub nieprawidłowy format
        ///         1a.5. Oczekuje na reakcję użytkownika z treścią pytania
        ///             1a.5a. Jeśli nie nadejdzie:
        ///                 1a.5a.1. Poinformuje użytkownika o błędzie
        ///                 1a.5a.2. Wstrzymuje proces
        ///                 1a.5a.3. KONIEC - algorytm nie otrzymał wymaganej informacji
        ///         1a.6. Oczekuje na reakcję użytkownika z możliwymi odpowiedziami
        ///             1a.6a. Jeśli nie nadejdzie:
        ///                 1a.6a.1. Poinformuje użytkownika o błędzie
        ///                 1a.6a.2. Wstrzymuje proces
        ///                 1a.6a.3. KONIEC - algorytm nie otrzymał wymaganej informacji
        ///         1a.7. Odczytuje możliwe odpowiedzi z pojedynczej wiadomości
        ///         1a.8. Wysyła do użytkownika informacje o odczytanych możliwych odpowiedziach
        ///         1a.9. Oczekuje na reakcję użytkownika z indeksem prawidłowej odpowiedzi
        ///             1a.9a. Jeśli nie nadejdzie lub będzie miało nieprawidłowy format lub nie odczytano odpowiedzi o takim indeksie:
        ///                 1a.9a.1. Poinformuje użytkownika o błędzie
        ///                 1a.9a.2. Wstrzymuje proces
        ///                 1a.9a.3. KONIEC - algorytm nie otrzymał wymaganej informacji od użytkownika lub nieprawidłowy format
        ///         1a.10. Oczekuje na reakcję użytkownika z linkiem URL do obrazka
        ///             1a.10a. Jeśli nadejdzie:
        ///                 1a.10a.1. Zapisuje informacje o obrazku
        ///         1a.11. Sprawdza najniższy dostępny numer identyfikacyjny
        ///         1a.12. Tworzy nowe pytanie
        ///         1a.13. Dodaje pytanie do bazy danych
        ///         1a.14. Zapisuje zmiany do bazy danych
        ///         1a.15. KONIEC - sukces, pytanie zostało dodane
        ///     1b. Jeśli nie jest:
        ///         1b.1. Wypisuje informacje o nadużyciu
        ///         1b.2. KONIEC - użytkownik nie uprawniony
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

                await Context.Channel.SendMessageAsync("Dodano pytanie.");
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
        ///         1a.2. Pobiera wszystkie dostępne w gildii kategorie
        ///         1a.3. Wysyła do użytkownika informacje o istniejących kategoriach
        ///         1a.4. Oczekuje na reakcję użytkownika z indeksem kategorii
        ///             1a.4a. Jeśli nie nadejdzie lub będzie miało nieprawidłowy format:
        ///                 1a.4a.1. Poinformuje użytkownika o błędzie
        ///                 1a.4a.2. Wstrzymuje proces
        ///                 1a.4a.3. KONIEC - algorytm nie otrzymał wymaganej informacji od użytkownika lub nieprawidłowy format
        ///         1a.5. Oczekuje na reakcję użytkownika z tytułem opracowania
        ///             1a.5a. Jeśli nie nadejdzie:
        ///                 1a.5a.1. Poinformuje użytkownika o błędzie
        ///                 1a.5a.2. Wstrzymuje proces
        ///                 1a.5a.3. KONIEC - algorytm nie otrzymał wymaganej informacji
        ///         1a.6. Oczekuje na reakcję użytkownika z treścią opracowania
        ///             1a.6a. Jeśli nie nadejdzie:
        ///                 1a.6a.1. Poinformuje użytkownika o błędzie
        ///                 1a.6a.2. Wstrzymuje proces
        ///                 1a.6a.3. KONIEC - algorytm nie otrzymał wymaganej informacji
        ///         1a.7. Oczekuje na reakcję użytkownika z linkiem URL do obrazka
        ///             1a.7a. Jeśli nadejdzie:
        ///                 1a.7a.1. Zapisuje informacje o obrazku
        ///         1a.8. Sprawdza najniższy dostępny numer identyfikacyjny
        ///         1a.9. Tworzy nowe opracowanie
        ///         1a.10. Dodaje opracowanie do bazy danych
        ///         1a.11. Zapisuje zmiany do bazy danych
        ///         1a.12. KONIEC - sukces, pytanie zostało dodane
        ///     1b. Jeśli nie jest:
        ///         1b.1. Wypisuje informacje o nadużyciu
        ///         1b.2. KONIEC - użytkownik nie uprawniony
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

                await Context.Channel.SendMessageAsync("Dodano opracowanie problemu.");
            }
            catch (Exception)
            {
                await ReplyAsync("Niewłaściwy format. Wstrzymano dodawanie pytania.");
                return;
            }
        }
    }
}
