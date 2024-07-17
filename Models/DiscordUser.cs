using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShitpepeRebirth.Models
{
    public class DiscordUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string? Username { get; set; }
        public string UserBotRole { get; set; }
        public int UserLevel { get; set; }
        public ulong UserExpAmount { get; set; }

        public DiscordUser()
        {
            UserBotRole = "Newbie";
            UserLevel = 0;
            UserExpAmount = 0;
        }
    }

    public class DiscordUserWallet
    {
        [Key]
        public ulong Id { get; set; }
        public long MoneyAmount { get; set; }
        public ulong DiscordUserId { get; set; }
        public int WalletCode { get; set; }

        public DiscordUserWallet()
        {
            MoneyAmount = 0;
        }
    }

    public class ForJsonConvertion
    {
        public string? token;
        public string? connection;
        public string? GetToken(string? filePath)
        {
            string jsonFile = File.ReadAllText(filePath);
            var tokenConnection = JsonConvert.DeserializeObject<ForJsonConvertion>(jsonFile);
            return tokenConnection?.token;
        }
        public string? GetConnection(string? filePath)
        {
            string jsonFile = File.ReadAllText(filePath);
            var tokenConnection = JsonConvert.DeserializeObject<ForJsonConvertion>(jsonFile);
            return tokenConnection?.connection;
        }
    }

}
