using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;

namespace ShitpepeRebirth
{
    public class Program
    {
        private static DiscordSocketClient _client;
        public static string _token;

        public static async Task Main()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;

            using (JsonTextReader JTR = new(new StringReader(File.ReadAllText("D:\\discord\\ShitpepeRebirth\\config.json"))))
            {
                while (JTR.Read())
                {
                    if (JTR.Value != null)
                    {
                        _token = Convert.ToString(JTR.Value);
                    }
                }
            }

            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            await _client.LoginAsync(TokenType.Bot, _token);
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
            //ulong guildId = 828224191517556747;

            var userInfoCommand = new SlashCommandBuilder()
                .WithName("user-info")
                .WithDescription("Команда, выводящая информацию о пользователе")
                .AddOption("user", ApplicationCommandOptionType.User, "Пользователь, для которого необходимо вывести информацию", isRequired: false);
            try
            {
                var myList = await _client.GetGlobalApplicationCommandsAsync();
                Console.WriteLine(myList.First());
                //await _client.Rest.DeleteAllGlobalCommandsAsync();
                //await _client.CreateGlobalCommandAsync(guildCommand.Build());
                //await _client.CreateGlobalApplicationCommandAsync(userInfoCommand.Build());
                //await _client.Rest.CreateGuildCommand(guildCommand.Build(), guildId);
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
            }
        }

        private static async Task HandleUserInfoCommand(SocketSlashCommand command)
        {
            var curUser = (SocketGuildUser?)command.Data.Options.Where(x => x.Name == "user").Select(x => x.Value).FirstOrDefault();

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(curUser?.ToString(), curUser?.GetAvatarUrl())
                .WithTitle("Информация о пользователе")
                .WithDescription(
                $@"
                > - ID: {curUser?.Id}
                > - Глобальное имя: {curUser?.GlobalName}
                > - Дата создания аккаунта: {curUser?.CreatedAt}
                > - Имя на сервере: {curUser?.Username}
                > - Является ли аккаунт ботом: {curUser?.IsBot}
                ")
                .WithColor(Color.DarkOrange)
                .WithCurrentTimestamp();
            await command.RespondAsync(embed: embedBuilder.Build());
        }
    }
}