# log4net.Appender.Telegram

Configure:
```xml
<appender name="TelegramAppender" type="log4net.Appender.Telegram.TelegramAppender, log4net.Appender.Telegram">
    <ChatId>Your User ID</ChatId>
    <Token>Your Telegram Bot Token</Token>
    <filter type="log4net.Filter.LevelRangeFilter">
        <param name="LevelMin" value="ERROR" />
        <param name="LevelMax" value="FATAL" />
    </filter>
    <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%message" />
    </layout>
</appender>
```

Use:
```cs
LogManager.GetLogger(string.Empty).Fatal("Fatal message");
```

Appender is non-blocking. What is why call ``LogManager.Shutdown();`` when application finishes.
