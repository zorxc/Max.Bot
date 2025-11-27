# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


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

## [Unreleased]

### Added
- GitHub Actions workflow `ci.yml` с форматированием, анализаторами, покрытием и упаковкой артефактов.
- Workflow `release.yml`, автоматически публикующий NuGet-пакеты по тегам `v*`.
- `LoopbackSampleRuntime` и рефакторинг `SampleBotsTests`, которые прогоняют все сценарии `SampleRegistry` офлайн.
- Фикстуры API для `/subscriptions` и `/updates`, используемые в интеграционных тестах.

### Changed
- README и новый `RELEASING.md`, описывающие CI/CD и инструкции по релизу.

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

[Unreleased]: https://github.com/MaxBotNet/MaxBotNet/compare/v0.3.3-alpha...HEAD
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

