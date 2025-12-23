# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

_Новые изменения появятся здесь._

## [0.4.1-alpha] - 2025-12-23

### Changed

- **`LinkedMessage.Message`**: Изменён тип с `Message` на `MessageBody` для соответствия реальной структуре API
  - В API поле `link.message` содержит только `MessageBody` (mid, seq, text), а не полный объект `Message`
  - Добавлены convenience properties: `LinkedMessage.Mid`, `LinkedMessage.Seq` для удобного доступа
  - Свойство `LinkedMessage.Text` продолжает работать как раньше

### Fixed

- **Исправлена критическая ошибка десериализации `LinkedMessage`**: теперь при получении webhook с reply/forward сообщением все поля корректно заполняются
  - Ранее поля `LinkedMessage.Message` (mid, seq, text, sender, chat_id) оставались `null` из-за несоответствия типа данных
  - Причина: в реальном API `link.message` содержит `MessageBody`, а не полный `Message`
  - Теперь `link.message` корректно парсится как `MessageBody` с полями `mid`, `seq`, `text`
  - Добавлены хелперы `Mid`, `Seq` для удобного доступа к данным связанного сообщения

## [0.4.0-alpha] - 2025-12-23

### Changed

- **`Message.Text`**: Теперь помечено `[JsonIgnore]` - это convenience property для доступа к `Body.Text`, не сериализуется в JSON
  - Свойство `Text` больше НЕ сериализуется в JSON при отправке сообщений
  - В официальном MAX API текст находится в `message.body.text`, а не `message.text`
  - При чтении сообщений `Text` продолжает работать как удобный хелпер для `Body.Text`
  - Упрощена логика синхронизации: `Text` теперь напрямую работает с `Body.Text`
- Обновлены примеры в `SampleRuntime.cs` для использования `Message.Recipient` вместо `Message.Chat`

### Fixed

- Исправлена сериализация `Message` объектов: свойство `Text` больше не создаёт дублирующее поле в JSON
- Теперь при отправке сообщений текст корректно передаётся только в `body.text` согласно официальной документации MAX API

### Removed

- Удалены legacy/backward-compat поля из `Message`: `From`, `Id`, `Chat`, `Date`, `Type`
- Удалён legacy алиас `Chat.Id` (используйте `Chat.ChatId`)
- Удалены legacy алиасы `ChatType.Private` и `ChatType.Group` (используйте `Dialog` и `Chat`)
- Удалён неиспользуемый enum `MessageType` (после удаления `Message.Type`)

## [0.3.9-alpha] - 2025-12-23

### Fixed

- **`NewMessageLink`**: Исправлено имя JSON-поля для message ID с `id` на `mid` в соответствии с Max API.
  - Ранее API возвращал ошибку: `"Field 'request.link.mid' cannot be null"`
  - Теперь при forward/reply сообщений используется корректное поле `mid`

## [0.3.8-alpha] - 2025-12-23

Технический релиз с теми же изменениями, что и 0.3.7-alpha (для корректной публикации в NuGet).

## [0.3.7-alpha] - 2025-12-23

### Added
- **Enum `MessageLinkType`** с значениями `Forward` и `Reply` для типизации ссылок на сообщения.
- **Поле `NewMessageLink.Type`** (обязательное) - требуется Max API для правильной обработки forward/reply.
- 4 новых теста в `NewMessageLinkTests`:
  - `ChatId_ShouldAcceptPositiveValues_ForPersonalChats` (2 сценария)
  - `ChatId_ShouldAcceptNegativeValues_ForGroupChats` (2 сценария)
  - `ShouldSerializeCorrectly_WithNegativeChatId_ForGroupChats`

### Changed
- **BREAKING**: Изменен тип `messageId` с `long` на `string` в методах:
  - `IMessagesApi.ForwardMessageAsync()`
  - `IMessagesApi.ReplyToMessageAsync()`
  - `NewMessageLink.Id`
- **BREAKING**: В `NewMessageLink` добавлено обязательное поле `Type: MessageLinkType`.
- `ForwardMessageAsync` автоматически устанавливает `Type = MessageLinkType.Forward`.
- `ReplyToMessageAsync` автоматически устанавливает `Type = MessageLinkType.Reply`.
- Обновлена валидация: вместо проверки `messageId <= 0` используется `ValidateNotEmpty()`.
- Обновлены тесты для использования `MaxJsonSerializer` вместо стандартного `JsonSerializer`.

### Fixed
- **Критическое**: Исправлена ошибка `{"code":"proto.payload","message":"Field 'request.link.type' cannot be null"}` при вызове `ForwardMessageAsync` и `ReplyToMessageAsync`.
- Убран атрибут `[Range(1, long.MaxValue)]` из `NewMessageLink.ChatId`, который блокировал отрицательные ID для групповых чатов.
- Добавлена документация для `NewMessageLink.ChatId`: положительные значения для личных чатов, отрицательные для групповых.

## [0.3.6-alpha] - 2025-12-23

### Removed
- Удален метод `BaseApi.ValidateChatId()` и все его вызовы (20 мест), так как валидация chatId не требуется. Max API принимает любые значения `long` для chatId:
  - **Dialog** (личные чаты): 0 или положительные
  - **Chat** (групповые чаты): положительные  
  - **Channel** (каналы): отрицательные (как в Telegram Bot API)

### Fixed
- Убран `[Range(1, long.MaxValue)]` атрибут в `Chat.ChatId`, так как он блокировал отрицательные ID каналов и chatId = 0 для диалогов.

## [0.3.5-alpha] - 2025-12-22

### Added
- Unit-тесты `MessageRecipientTests` с 8 сценариями проверки десериализации и сериализации `MessageRecipient.ChatId` и `UserId` со значениями 0, null и положительными числами.
- Метод `BaseApi.ValidateUserIdStrict()` для строгой валидации userId в операциях, где 0 недопустим (например, удаление админа/участника чата).

### Changed
- Обновлены unit-тесты валидации API методов: для большинства методов убраны проверки на `chatId = 0` и `userId = 0`, оставлены только проверки на отрицательные значения. Для `RemoveChatAdminAsync` и `RemoveChatMemberAsync` сохранена проверка на 0, так как это требуется контрактом интерфейса.
- `RemoveChatAdminAsync` и `RemoveChatMemberAsync` теперь используют `ValidateUserIdStrict()` вместо `ValidateUserId()` для соответствия контракту интерфейса `IChatsApi`, который требует отклонять userId <= 0.

### Fixed
- **КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ:** Убрана строгая валидация `[Range(1, long.MaxValue)]` для `MessageRecipient.ChatId` и `MessageRecipient.UserId`, чтобы поддержать личные сообщения (direct messages), где Max API может передавать `ChatId = 0` или `null`. Оба свойства остались nullable (`long?`), но теперь принимают любые значения, включая 0.
- **КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ:** Исправлена валидация в `BaseApi.ValidateChatId()` и `BaseApi.ValidateUserId()` - изменена проверка с `<= 0` на `< 0`, что разрешает использование значения 0 для личных сообщений (dialogs) в соответствии с поведением Max API. Отрицательные значения по-прежнему запрещены.
- Исправлена несогласованность валидации: `RemoveChatAdminAsync` и `RemoveChatMemberAsync` теперь корректно отклоняют userId = 0 в соответствии с контрактом интерфейса, используя новый метод `ValidateUserIdStrict()`.

## [0.3.3-alpha] - 2025-11-27

### Added
- Поддержка `intent` для inline-кнопок: новый enum `ButtonIntent`, свойство `InlineKeyboardButton.Intent` и обновлённый конвертер обеспечивают соответствие API и позволяют задавать позитивные/негативные стили.
- Модель `BotCommand`, запрос `UpdateBotInfoRequest` и публичный метод `SetCommandsAsync` в `BotApi`, позволяющие задавать меню команд через официальный PATCH `/me`.
- Unit-тесты для новых типов и поведения (`InlineKeyboardButton`, `BotCommand`, `BotApi.SetCommandsAsync`), чтобы предотвратить регрессии.

### Changed
- GitHub Actions требования локально подтверждены: `dotnet format` и полный прогон `dotnet test` с покрытием соответствуют текущему CI.

## [0.3.2-alpha] - 2025-11-25

### Fixed
- Исправлена критическая ошибка в `EditMessageReplyMarkupAsync`: теперь метод корректно сохраняет другие вложения (изображения, файлы) при удалении или замене клавиатуры. Ранее пустой массив удалял все вложения сообщения.
- Исправлена потенциальная `NullReferenceException` в `EditMessageReplyMarkupAsync` при обработке сообщений без вложений: добавлена безопасная обработка null-значений для `Body` и `Attachments`.

## [0.3.1-alpha] - 2025-11-25

### Added
- Метод `EditMessageReplyMarkupAsync` для редактирования только клавиатуры сообщения, аналогичный Telegram API. Позволяет удалять или заменять inline-клавиатуру в одну строчку.

### Changed
- Увеличен лимит валидации `CallbackId` с 64 до 512 символов для поддержки реальных callback_id от Max API (~128 символов).

### Removed
- Удалены устаревшие legacy части для соответствия оригинальному Max API:
  - Удалено устаревшее свойство `Update.CallbackQuery` (используйте `Update.Callback`).
  - Удалены устаревшие enum aliases `UpdateType.Message` и `UpdateType.CallbackQuery` (используйте `UpdateType.MessageCreated` и `UpdateType.MessageCallback`).

### Fixed
- Исправлена ошибка валидации при обработке callback-запросов: реальные callback_id от API (~128 символов) теперь корректно принимаются благодаря увеличению лимита валидации.

## [0.3.0-alpha] - 2025-11-24

### Added
- Полная синхронизация типов `Update` с официальной документацией Max API.
- Расширение `UpdateType` enum: добавлены все 16 официальных типов обновлений (`MessageCreated`, `MessageCallback`, `MessageEdited`, `MessageRemoved`, `BotAdded`, `BotRemoved`, `BotStarted`, `BotStopped`, `DialogMuted`, `DialogUnmuted`, `DialogCleared`, `DialogRemoved`, `UserAdded`, `UserRemoved`, `ChatTitleChanged`, `MessageChatCreated`).
- Типизированные обёртки для всех типов update: `MessageEditedUpdate`, `MessageRemovedUpdate`, `BotAddedUpdate`, `BotRemovedUpdate`, `BotStartedUpdate`, `BotStoppedUpdate`, `UserAddedUpdate`, `UserRemovedUpdate`, `ChatTitleChangedUpdate`, `MessageChatCreatedUpdate` и другие.
- Свойство `Update.Callback` для доступа к callback-запросам (соответствует API полю `callback`).
- Свойства `Update.Chat`, `Update.User`, `Update.InviterId`, `Update.ChatId`, `Update.IsMuted` для различных типов обновлений.
- Свойство `Chat.ChatId` как основное поле, соответствующее API полю `chat_id`.
- Свойство `Message.Sender` как основное поле, соответствующее API полю `sender`.
- Класс `LinkedMessage` для представления связанных сообщений (forward/reply).
- Unit-тесты для всех новых типов update с примерами из реального API.

### Changed
- `UpdateType.Message` и `UpdateType.CallbackQuery` помечены как устаревшие (используйте `MessageCreated` и `MessageCallback`).
- `Update.CallbackQuery` помечено как устаревшее (используйте `Update.Callback`).
- `Message.From` помечено как устаревшее (используйте `Message.Sender`).
- `Chat.Id` теперь read-only свойство-алиас для `Chat.ChatId` (для обратной совместимости).
- `ChatType.Private` и `ChatType.Group` помечены как устаревшие (используйте `Dialog` и `Chat` соответственно).
- Значения `ChatType` enum приведены в соответствие с API: `Dialog` (вместо `Private`), `Chat` (вместо `Group`).
- `UpdateJsonConverter` обновлён для корректной десериализации всех типов update и их специфичных полей.
- `UpdateHandlerExecutor` обновлён для использования новых значений `UpdateType.MessageCreated` и `UpdateType.MessageCallback`.
- Все примеры и тесты обновлены для использования новых API-совместимых имён полей.

### Fixed
- Исправлена десериализация `ChatType` enum: добавлен `JsonStringEnumConverter` для корректного преобразования строковых значений API.
- Исправлена десериализация `Update` объектов: все поля теперь корректно парсятся в зависимости от `update_type`.
- Исправлены тесты для соответствия новой структуре API (использование `chat_id` вместо `id`, `sender` вместо `from`, `dialog`/`chat` вместо `private`/`group`).

## [0.2.7-alpha] - 2025-11-24

### Changed
- Обновление версии пакета.

## [0.2.6-alpha] - 2025-11-24

### Fixed
- Исправлена критическая ошибка сериализации inline-клавиатуры: изменено JSON-поле с `inline_keyboard` на `buttons` для соответствия API Max Messenger. Теперь сервер корректно получает структуру `payload.buttons` вместо `payload.inline_keyboard`, что устраняет `NullPointerException` на стороне сервера при вызове `getButtons()`.
- Добавлена нормализация кнопок клавиатуры в `CreateInlineKeyboardAttachment` для предотвращения null-значений в массивах кнопок. Свойство `Buttons` в `InlineKeyboard` теперь гарантированно не содержит null-элементов.

## [0.2.5-alpha] - 2025-11-24

### Added
- Типизированные обертки `MessageUpdate` и `CallbackQueryUpdate` для улучшенной типобезопасности при работе с обновлениями. Структура теперь аналогична Telegram Bot API с разделением по типу обновления.
- Метод `SendMessageToUserAsync` для отправки сообщений с inline-клавиатурой пользователю по `userId` (дополняет существующий метод с `chatId`).

### Changed
- Рефакторинг структуры `Update`: добавлены свойства `MessageUpdate` и `CallbackQueryUpdate` для доступа к типизированным оберткам. Старые свойства `Message` и `CallbackQuery` сохранены для обратной совместимости.
- Улучшена структура обновлений по аналогии с Telegram Bot API, что позволяет использовать pattern matching для обработки разных типов обновлений.

## [0.2.4-alpha] - 2025-11-24

### Fixed
- Исправлена ошибка валидации при обработке вебхуков типа `message_created`. Свойство `Message.Id` теперь nullable (`long?`), что позволяет корректно десериализовать обновления, где числовой `message.id` отсутствует, а уникальный ID сообщения передаётся только в `message.body.mid` (строка).

## [0.2.3-alpha] - 2025-11-24

### Added
- Перегрузка метода `SendMessageAsync` с опциональным параметром `InlineKeyboard? keyboard` для упрощения отправки сообщений с клавиатурой. Теперь можно отправлять сообщения с inline-клавиатурой без необходимости вручную создавать `SendMessageRequest` и `AttachmentRequest`.

### Changed
- Обновлен пример `KeyboardBotSample` для демонстрации использования новой упрощенной перегрузки `SendMessageAsync` с клавиатурой.

## [0.2.0-alpha] - 2025-11-17

### Added
- Enforced XML documentation (CS1591) for all public APIs and added `DocumentationCoverageTests` to guard regressions.
- Introduced `examples/Max.Bot.Examples` console project with Echo/Command/Keyboard/File bot samples inspired by Telegram.Bot and VkNet.
- Added comprehensive smoke tests for the new samples (`SampleBotsTests`) leveraging mocked runtimes.
- Expanded README with installation steps, quick start, feature overview, and sample catalogue plus references to official docs and sibling SDKs.

### Changed
- Updated `Directory.Build.props` to treat missing XML docs as errors while exempting the test project.
- Refined solution structure to include the examples project for local experimentation.

## [0.1.0-alpha] - 2025-01-XX

### Added
- Initial project setup
- Basic infrastructure and folder structure
- Build configuration for .NET 9

[Unreleased]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.6-alpha...HEAD
[0.3.6-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.5-alpha...v0.3.6-alpha
[0.3.5-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.4-alpha...v0.3.5-alpha
[0.3.4-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.3-alpha...v0.3.4-alpha
[0.3.3-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.2-alpha...v0.3.3-alpha
[0.3.2-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.1-alpha...v0.3.2-alpha
[0.3.1-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.0-alpha...v0.3.1-alpha
[0.3.0-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.2.7-alpha...v0.3.0-alpha
[0.2.7-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.2.6-alpha...v0.2.7-alpha
[0.2.6-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.2.5-alpha...v0.2.6-alpha
[0.2.5-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.2.4-alpha...v0.2.5-alpha
[0.2.4-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.2.0-alpha...v0.2.4-alpha
[0.2.0-alpha]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.1.0-alpha...v0.2.0-alpha
[0.1.0-alpha]: https://github.com/MaxBotNet/MaxBotNet/releases/tag/v0.1.0-alpha

