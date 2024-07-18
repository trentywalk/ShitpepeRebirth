using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using ShitpepeRebirth.Models;

namespace ShitpepeRebirth
{
    public class Program
    {
        private static DiscordSocketClient? _client;
        private static string? _token;

        public static async Task Main()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });
            _client.Log += Log;
            _token = new ForJsonConvertion().GetToken(@"./config.json");

            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.MessageReceived += MessageHandler;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.SetCustomStatusAsync("Пути назад .Net");
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static async Task Client_Ready()
        {
            var userInfoCommand = new SlashCommandBuilder()
                .WithName("user-info")
                .WithDescription("Команда, выводящая информацию о пользователе")
                .AddOption("user", ApplicationCommandOptionType.User, "Пользователь, для которого необходимо вывести информацию", isRequired: false);

            var serverInfoCommand = new SlashCommandBuilder()
                .WithName("server-info")
                .WithDescription("Команда, выводящая информацию о сервере");

            var russianRouletteCommand = new SlashCommandBuilder()
                .WithName("rr")
                .WithDescription("Команда для игры в русскую рулетку")
                .AddOption("mode", ApplicationCommandOptionType.String, "Режим русской рулетки (classic, role, mute, ban, custom)", isRequired: true)
                .AddOption("role", ApplicationCommandOptionType.Role, "Роль для режима игры на роль", isRequired: false)
                .AddOption("bullet_number", ApplicationCommandOptionType.Integer, "Количество пуль для режима custom (максимум 5)", isRequired: false);

            var createWalletCommand = new SlashCommandBuilder()
                .WithName("create-wallet")
                .WithDescription("Команда для создания кошелька для валюты внутри бота")
                .AddOption("pin_code", ApplicationCommandOptionType.Integer, "Код для подтверждений операций с валютой", isRequired: true);

            var checkBotStatsCommand = new SlashCommandBuilder()
                .WithName("check-my-stats")
                .WithDescription("Команда для вывода статистики внутри бота");

            var commandList = new List<SlashCommandBuilder>();
            commandList.AddRange([
                userInfoCommand,
                serverInfoCommand,
                russianRouletteCommand,
                createWalletCommand,
                checkBotStatsCommand
                ]);

            try
            {
                foreach (var item in commandList)
                {
                    await _client.CreateGlobalApplicationCommandAsync(item.Build());
                }
            }
            catch (HttpException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "user-info":
                    {
                        await HandleUserInfoCommand(command);
                        break;
                    }
                case "server-info":
                    {
                        await HandleServerInfoCommand(command);
                        break;
                    }
                case "rr":
                    {
                        await HandleRussianRouletteCommand(command);
                        break;
                    }
                case "create-wallet":
                    {
                        await HandleCreateWalletCommand(command);
                        break;
                    }
                case "check-my-stats":
                    {
                        await HandleCheckBotStatsCommand(command);
                        break;
                    }
                default:
                    await command.RespondAsync("Данная команда не найдена", ephemeral: true);
                    break;
            }
        }

        private static async Task HandleUserInfoCommand(SocketSlashCommand command)
        {
            var curUser = (SocketGuildUser?)command.Data.Options.Where(x => x.Name == "user").Select(x => x.Value).FirstOrDefault();
            if (curUser != null) await SendUserInfo(command, curUser);
            else await SendUserInfo(command, (SocketGuildUser)command.User);
        }

        private static async Task SendUserInfo(SocketSlashCommand command, SocketGuildUser? user)
        {
            var embedBuilder = new EmbedBuilder()
                .WithAuthor(user?.ToString(), user?.GetAvatarUrl())
                .WithTitle("Информация о пользователе")
                .WithDescription(
                $@"
                > - ID: {user?.Id}
                > - Глобальное имя: {user?.Username}
                > - Дата создания аккаунта: {user?.CreatedAt}
                > - Имя на сервере: {user?.DisplayName}
                > - Является ли аккаунт ботом: {user?.IsBot}
                ")
                .WithColor(Color.DarkOrange)
                .WithCurrentTimestamp();
            await command.RespondAsync(embed: embedBuilder.Build());
        }

        private static async Task HandleServerInfoCommand(SocketSlashCommand command)
        {
            var curServer = command.GetChannelAsync().Result;
            var embedBuilder = new EmbedBuilder()
                .WithAuthor(command.User.ToString(), command.User.GetAvatarUrl())
                .WithTitle("Информация о канале")
                .WithDescription($@"
                > - ID: {curServer.Id}
                > - Название канала: {curServer.Name}
                > - Дата создания: {curServer.CreatedAt}
                > - Количество пользователей: {curServer.GetUsersAsync().CountAsync()}
                ")
                .WithColor(Color.DarkOrange)
                .WithCurrentTimestamp();
            await command.RespondAsync(embed: embedBuilder.Build());
        }

        private static async Task HandleRussianRouletteCommand(SocketSlashCommand command)
        {
            //not today
        }

        private static async Task HandleCreateWalletCommand(SocketSlashCommand command)
        {
            //not today
        }

        private static async Task HandleCheckBotStatsCommand(SocketSlashCommand command)
        {
            using (DBContext dbc = new())
            {
                var user = dbc.DiscordUsers.Where(x => x.Id == command.User.Id).FirstOrDefault();
                if (user != null)
                {
                    var embedBuilder = new EmbedBuilder()
                        .WithAuthor(command.User.ToString(), command.User.GetAvatarUrl())
                        .WithTitle("Ваша статистика внутри бота")
                        .WithDescription($@"
                        > - {user.Id}
                        > - {user.Username}
                        > - {user.UserBotRole}
                        > - {user.UserLevel}
                        > - {user.UserExpAmount}
                        ")
                        .WithColor(Color.DarkOrange)
                        .WithCurrentTimestamp();
                    await command.RespondAsync(embed: embedBuilder.Build());
                }
                else
                    await command.RespondAsync("Вы не написали ни одного сообщения на серверах, где есть бот");
            }
        }

        private static async Task MessageHandler(SocketMessage msgParam)
        {
            var message = msgParam as SocketUserMessage;
            if (message == null || message.Author.IsBot) return;
            var messageAuthor = message.Author;
            using (DBContext dbc = new())
            {
                var user = dbc.DiscordUsers.Where(x => x.Id == messageAuthor.Id).FirstOrDefault();
                if (user != null)
                {
                    Console.WriteLine($"Пользователь {user.Username} получил +1 к опыту");
                    user.UserExpAmount += 1;
                }
                else
                {
                    DiscordUser discordUser = new();
                    discordUser.Id = messageAuthor.Id;
                    discordUser.Username = messageAuthor.Username;
                    await dbc.DiscordUsers.AddAsync(discordUser);
                    
                }
                await dbc.SaveChangesAsync();
            }
        }

    }
}