# Telegram Bot Configuration

Configure your Telegram bot using the following settings.

## Configuration File

```json
{
  "telegramBotToken": "your_token_here",
  "whitelist": [123, 456]
}
```

### Parameters:

* **`telegramBotToken`**
  Your unique access token from the Telegram Bot API.
  Example:
  `"telegramBotToken": "76544444:DABEZ2412AF7D4_t0f29_n2lkBr8_yiS42"`

* **`whitelist`**
  A list of Telegram user IDs who are allowed to interact with the bot.
  Format: an array of integers.
  Example:
  `"whitelist": [9999999, 12345678]`

## Usage

1. Get a token from [@BotFather](https://t.me/BotFather).
2. Replace `your_token_here` with your actual bot token.
3. Add Telegram user IDs to the `whitelist` array.
4. Save the configuration to a `settings.json` file in the project root directory.
5. Place your `.maFile` files inside the `maFiles` directory before starting the bot.
6. Run the bot — it will only respond to whitelisted users.


## Tips

* To find your Telegram user ID, use [@userinfobot](https://t.me/userinfobot).
