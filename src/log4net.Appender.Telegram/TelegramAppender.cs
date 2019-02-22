using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using log4net.Core;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace log4net.Appender.Telegram
{
    public class TelegramAppender : AppenderSkeleton
    {
        public string Token { get; set; }

        public string ChatId { get; set; }

        public ParseMode ParseMode { get; set; }

        protected static TelegramBotClient Bot;

        private readonly List<Task> _tasks = new List<Task>();

        protected override void Append(LoggingEvent e)
        {
            if (string.IsNullOrEmpty(Token))
            {
                throw new ConfigurationErrorsException("Please set the Token under TelegramAppender configuration section: <Token>...</Token>");
            }

            if (string.IsNullOrEmpty(ChatId))
            {
                throw new ConfigurationErrorsException("Please set the ChatId under TelegramAppender configuration section: <ChatId>...</ChatId>");
            }

            if (Bot == null) Bot = new TelegramBotClient(Token);
            var message = Layout == null ? e.RenderedMessage : RenderLoggingEvent(e);
            _tasks.Add(Bot.SendTextMessageAsync(ChatId, message, ParseMode));
        }

        protected override void OnClose()
        {
            Task.WaitAll(_tasks.Where(x => !x.IsCompleted).ToArray());
        }
    }
}