using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tools;

public class Typewriter
{
    public async Task TypeMessageBySymbols(ITelegramBotClient client, long chatId, CancellationToken cancellationToken,
        string messageText)
    {
        Message sentmessage = await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Печатает...",
            cancellationToken: cancellationToken);

        string stringForType = String.Empty;
        
        foreach (var symbol in messageText)
        {
            if (!Char.IsWhiteSpace(symbol))
            {
                stringForType += symbol;
                await Task.Delay(50, cancellationToken);
                await client.EditMessageTextAsync(chatId: chatId, messageId: sentmessage.MessageId, text: stringForType,
                    cancellationToken: cancellationToken);
            }
            else
            {
                stringForType += " ";
            }
        }
    }
    
    public async Task TypeMessageByWords(ITelegramBotClient client, long chatId, CancellationToken cancellationToken,
        string messageText)
    {
        Message sentmessage = await client.SendTextMessageAsync(
            chatId: chatId,
            text: "...",
            cancellationToken: cancellationToken);

        string stringForType = String.Empty;
        
        foreach (var symbol in messageText.Split())
        {
            stringForType += symbol + " ";
            await Task.Delay(50, cancellationToken);
            await client.EditMessageTextAsync(chatId: chatId, messageId: sentmessage.MessageId, text: stringForType,
                cancellationToken: cancellationToken);
        }
    }
}