using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using System.IO;
using OpenAI_API;
using Telegram.Bot.Types.ReplyMarkups;

namespace TheBlessedCrypt
{
    internal class Program
    {
        private static GptService gpt;

        static void Main()
        {
            // Configuration
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var token = config["Telegram:Token"];
            var openAiKey = config["OpenAI:ApiKey"];
            gpt = new GptService(openAiKey);

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
                    // Crypto keyboard
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]  // The first row of buttons
                        {
                            InlineKeyboardButton.WithCallbackData("Bitcoin", "BTC"),
                            InlineKeyboardButton.WithCallbackData("Ethereum", "ETH")
                        },
                        new[]  // The second row of buttons
                        {
                            InlineKeyboardButton.WithCallbackData("Litecoin", "LTC"),
                            InlineKeyboardButton.WithCallbackData("Ripple", "XRP")
                        }
                    });

                    await client.SendMessage(message.Chat.Id, "Welcome to The Blessed Crypt! Choose a cryptocurrency:", replyMarkup: keyboard);
                }
            }

            // Processing callback responses to button clicks
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var selectedCrypto = callbackQuery.Data;

                string responseMessage = selectedCrypto switch
                {
                    "BTC" => "Bitcoin (BTC)",
                    "ETH" => "Ethereum (ETH)",
                    "LTC" => "Litecoin (LTC)",
                    "XRP" => "Ripple (XRP)",
                    _ => "USA Dollar"
                };
                string prompt = string.Empty;
                if (responseMessage == "USA Dollar")
                    prompt = $"Ответь в стиле прожарки.Я не знаю название криптовалюты,прожарь меня";
                else
                    prompt = $"Ответь в стиле стёбной прожарки.Насколько изменилась цена {responseMessage} за 7 дней?Объясни почему цена снизилась или увеличилась на основании новостей.Стеби их всех";
                var gptReply = await gpt.AskAsync(prompt);
                await client.SendMessage(callbackQuery.Message.Chat.Id, gptReply);
            }
        }

        private static async Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}");
        }
    }

    public class GptService
    {
        private readonly OpenAIAPI _api;

        public GptService(string apiKey)
        {
            _api = new OpenAIAPI(apiKey);
        }

        public async Task<string> AskAsync(string prompt)
        {
            var chat = _api.Chat.CreateConversation();
            chat.AppendSystemMessage("Ты стёбный прожаривающий криптоаналитик.Отвечай кратко,стёбно,но правдиво,основываясь на новостях.");
            chat.AppendUserInput(prompt);

            var reply = await chat.GetResponseFromChatbot();
            return reply ?? "GPT не ответил.";
        }
    }
}