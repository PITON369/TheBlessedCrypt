using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TheBlessedCrypt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var basePath = AppContext.BaseDirectory;
            for (int i = 0; i <= 4; i++)
                basePath = Directory.GetParent(basePath)!.FullName;

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var token = config["Telegram:Token"];

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("❌ Token not found! Check configuration file.");
                return;
            }

            var client = new TelegramBotClient(token);
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        private static async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text == "/start")
                {
                    await client.SendMessage(message.Chat.Id, "Welcome to The Blessed Crypt!");
                }
                else
                {
                    await client.SendMessage(message.Chat.Id, "You said: " + message.Text);
                }
                return;
            }
        }

        private static async Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
        }
    }
}
