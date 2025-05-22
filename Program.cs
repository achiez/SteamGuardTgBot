using AchiesUtilities.Extensions;
using Newtonsoft.Json.Linq;
using SteamLib.SteamMobile;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

const string accountsDirectory = "mafiles";

if (File.Exists("settings.json") == false)
{
    Console.WriteLine("settings.json not found, please create it with the following content:");
    Console.WriteLine("{\"telegramBotToken\": \"YOUR_TELEGRAM_BOT_TOKEN\",\n\"whitelist\": [123]");
}

var settings = File.ReadAllText("settings.json");
var j = JObject.Parse(settings);
var telegramBotToken = j["telegramBotToken"]?.ToString();
var whitelistArray = j["whitelist"]?.ToObject<long[]>() ?? [];

if (string.IsNullOrWhiteSpace(telegramBotToken) || whitelistArray.Length == 0)
{
    Console.WriteLine("Invalid settings.json, please provide a valid telegramBotToken and whitelist.");
    return;
}




var accountSecrets = LoadAccounts(accountsDirectory);

var bot = new SteamGuardBot(telegramBotToken, accountSecrets, whitelistArray);


await bot.RunAsync();


Dictionary<string, string> LoadAccounts(string directoryPath)
{
    if (!Directory.Exists(directoryPath))
    {
        Console.WriteLine($"Directory '{directoryPath}' not found.");
        Directory.CreateDirectory(directoryPath);
    }

    var res = new Dictionary<string, string>();

    foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.mafile"))
    {
        try
        {

            var json = File.ReadAllText(filePath);
            var j = JObject.Parse(json);
            var sharedSecret = j["shared_secret"]?.ToString();
            sharedSecret ??= j.Properties().FirstOrDefault(p => p.Name.EqualsIgnoreCase("shared_secret"))?.Value.ToString();
            sharedSecret ??= j.Properties().FirstOrDefault(p => p.Name.EqualsIgnoreCase("sharedsecret"))?.Value.ToString();

            var accountName = j["account_name"]?.ToString();
            accountName ??= j.Properties().FirstOrDefault(p => p.Name.EqualsIgnoreCase("account_name"))?.Value.ToString();
            accountName ??= j.Properties().FirstOrDefault(p => p.Name.EqualsIgnoreCase("accountname"))?.Value.ToString();

            if (string.IsNullOrEmpty(sharedSecret) || string.IsNullOrEmpty(accountName))
            {
                Console.WriteLine($"Invalid file format: '{filePath}'");
                continue;
            }

            res.Add(accountName, sharedSecret);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file '{filePath}': {ex.Message}");
        }
    }

    Console.WriteLine("Loaded {0} accounts", res.Count);
    return res;
}



public class SteamGuardBot
{
    private readonly Dictionary<string, string> _accountSecrets = new();
    private readonly HashSet<long> whitelist;
    private readonly ITelegramBotClient _botClient;

    public SteamGuardBot(string telegramBotToken, Dictionary<string, string> accountSecrets, long[] whiteList)
    {
        _botClient = new TelegramBotClient(telegramBotToken);
        _accountSecrets = accountSecrets;
        whitelist = new HashSet<long>(whiteList);
    }

    public async Task RunAsync()
    {
        var test = await _botClient.TestApiAsync();
        if (!test)
        {
            Console.WriteLine("Invalid token, please check your settings.json file.");
            return;
        }
        var cts = new CancellationTokenSource();
        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: cts.Token);

        Console.WriteLine("Bot started, press ESC to stop...");
        while (Console.ReadKey().Key != ConsoleKey.Escape)
        {

        }

        await cts.CancelAsync();
        Console.WriteLine("stopped");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Text: { } text, From: { Id: long userId } })
        {
            if (whitelist.Contains(userId))
            {
                string accountName = text.Trim();
                if (_accountSecrets.TryGetValue(accountName, out var sharedSecret))
                {
                    string steamGuardCode = SteamGuardCodeGenerator.GenerateCode(sharedSecret);

                    await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{steamGuardCode}", cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Account '{accountName}' not found.", cancellationToken: cancellationToken);
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "You are not authorized to use this bot.", cancellationToken: cancellationToken);
            }
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
        Console.WriteLine(exception);
        return Task.CompletedTask;
    }
}


