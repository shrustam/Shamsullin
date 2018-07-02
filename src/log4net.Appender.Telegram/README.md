Configure the appender in the following way:

```xml
<log4net>
    <root>
      <level value="DEBUG" />
      <appender-ref ref="TelegramAppender" />
    </root>
    <appender name="TelegramAppender" type="log4net.Appender.Telegram.TelegramAppender, log4net.Appender.Telegram">
      <Token>put bot's unique token here</Token>
      <ChatId>put bot's ChatId here</ChatId>
      <ParseMode>Default|Markdown|Html</ParseMode>
      <filter type="log4net.Filter.LevelRangeFilter">
        <param name="LevelMin" value="ERROR"/>
        <param name="LevelMax" value="FATAL"/>
      </filter>
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%message" />
      </layout>
    </appender>
</log4net>
```
