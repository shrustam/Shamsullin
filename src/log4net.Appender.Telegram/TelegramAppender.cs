using System.Configuration;
using log4net.Core;
using Telegram.Bot;

namespace log4net.Appender.Telegram
{
    public class TelegramAppender : AppenderSkeleton
    {
        public string Token { get; set; }

        public string ChatId { get; set; }

        protected override void Append(LoggingEvent e)
        {
            if (string.IsNullOrEmpty(Token))
            {
                throw new ConfigurationErrorsException("PLease set the Token under TelegramAppender configuration section: <Token>...</Token>");
            }

            if (string.IsNullOrEmpty(ChatId))
            {
                throw new ConfigurationErrorsException("PLease set the ChatId under TelegramAppender configuration section: <ChatId>...</ChatId>");
            }

            var message = Layout == null ? e.RenderedMessage : RenderLoggingEvent(e);
            var bot = new TelegramBotClient(Token);
            bot.SendTextMessageAsync(ChatId, message);
        }
    }
}
