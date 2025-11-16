# План разработки Max.Bot - C# библиотека для Max Messenger API

## 📋 Оглавление

1. [Обзор проекта](#обзор-проекта)
2. [Архитектура библиотеки](#архитектура-библиотеки)
3. [Этапы разработки](#этапы-разработки)
4. [Структура проекта](#структура-проекта)
5. [Технические решения](#технические-решения)
6. [Зависимости](#зависимости)
7. [Критерии приемки](#критерии-приемки)

---

## 🎯 Обзор проекта

**Цель:** Создать полнофункциональную библиотеку для работы с Max Messenger Bot API на C# (.NET 9).

**Референсные библиотеки:**
- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) - лучшие практики для .NET Bot API клиентов
- [VkNet](https://github.com/vknet/vk) - опыт работы с VK API на .NET

**Реализации для анализа:**
- [max-bot-api-client-ts](https://github.com/max-messenger/max-bot-api-client-ts) - TypeScript реализация
- [max-bot-api-client-go](https://github.com/max-messenger/max-bot-api-client-go) - Go реализация

**Документация:** [dev.max.ru/docs-api](https://dev.max.ru/docs-api) (скачать в `docs/` папку проекта)

---

## 🏗 Архитектура библиотеки

### Основные компоненты

```
Max.Bot (Основная библиотека)
├── MaxClient.cs                    // Главный клиент (точка входа)
├── Configuration/                  // Конфигурация и опции
│   ├── MaxBotOptions.cs            // Базовые опции бота (токен, базовый URL API, таймауты)
│   └── MaxBotClientOptions.cs      // Опции HTTP клиента (retry policy, rate limiting, логирование)
├── Api/                            // Группы API методов
│   ├── IMaxBotApi.cs              // Главный интерфейс
│   ├── BotApi.cs                  // Методы бота (getMe, getBotInfo)
│   ├── MessagesApi.cs             // Работа с сообщениями
│   ├── ChatsApi.cs                // Работа с чатами
│   ├── UsersApi.cs                // Работа с пользователями
│   ├── FilesApi.cs                // Работа с файлами
│   └── UpdatesApi.cs              // Получение обновлений
├── Types/                          // Модели данных API
│   ├── User.cs
│   ├── Chat.cs
│   ├── Message.cs
│   ├── Update.cs
│   ├── Response.cs
│   ├── Error.cs
│   └── Enums/
├── Networking/                     // HTTP клиент
│   ├── IHttpClient.cs             // Абстракция для тестирования
│   ├── MaxHttpClient.cs           // Реализация HTTP клиента
│   └── MaxApiRequest.cs           // Представление запроса
├── Exceptions/                     // Кастомные исключения
│   ├── MaxApiException.cs
│   ├── MaxNetworkException.cs
│   └── MaxRateLimitException.cs
└── Polling/                        // Механизмы получения обновлений
    ├── IUpdateHandler.cs
    ├── UpdatePoller.cs            // Long Polling
    └── WebhookController.cs       // Webhook (для ASP.NET Core)

Max.Bot.Tests (Тесты)
├── Unit/                          // Модульные тесты
├── Integration/                   // Интеграционные тесты
└── Examples/                      // Примеры использования
```

### Принципы проектирования

1. **Модульность** - API методы сгруппированы по функциональности
2. **Асинхронность** - Все методы async/await
3. **Типобезопасность** - Строгая типизация всех моделей
4. **Расширяемость** - Интерфейсы для всех компонентов
5. **Тестируемость** - Dependency Injection везде
6. **Производительность** - Правильное использование HttpClient
7. **Обработка ошибок** - Кастомные исключения на всех уровнях

---

## 📅 Этапы разработки

### ФАЗА 1: Подготовка инфраструктуры

**Задачи:**
- [x] 1.1. Создать решение `.sln` с проектами:
  - `Max.Bot` (основная библиотека)
  - `Max.Bot.Tests` (тесты)
  - `Max.Bot.Examples` (примеры использования, опционально)
- [x] 1.2. Настроить `Directory.Build.props` с общими настройками:
  - Target Framework: `net9.0`
  - Nullable: `enable`
  - LangVersion: `latest`
  - Common package references
- [x] 1.3. Добавить базовые файлы:
  - `.editorconfig` (стиль кода)
  - `.gitignore` (обновить для docs/)
  - `README.md` (базовый)
  - `LICENSE` (если нужно)
- [x] 1.4. Скачать документацию Max API в `docs/` и добавить в `.gitignore`
- [x] 1.4.1. Перед началом реализации желательно изучить документацию Max API (если она скачана в `docs/`) для понимания структуры эндпоинтов, моделей данных и методов API
- [x] 1.5. Настроить NuGet метаданные в `.csproj`:
  - PackageId: `Max.Bot`
  - Version: `0.1.0-alpha`
  - Authors, Description, Repository URL
- [x] 1.6. Добавить базовые зависимости:
  - `Microsoft.Extensions.Logging.Abstractions`
  - `Microsoft.Extensions.Options`
  - Примечание: `System.Net.Http` встроен в .NET 9, поэтому явная ссылка на `Microsoft.Extensions.Http` не требуется. Для HTTP клиента используется `System.Net.Http.HttpClient` напрямую или через `IHttpClientFactory` (если потребуется расширенная функциональность).

**Результат:** Готовая структура решения с настроенными проектами

---

### ФАЗА 2: Базовый HTTP клиент

**Задачи:**
- [x] 2.1. Создать интерфейс `IHttpClient` для абстракции HTTP вызовов
- [x] 2.2. Реализовать `MaxHttpClient`:
  - Базовая отправка HTTP запросов
  - Сериализация запросов (System.Text.Json)
  - Десериализация ответов
  - Обработка HTTP ошибок
- [x] 2.3. Реализовать `MaxApiRequest` для представления запросов к API
- [x] 2.4. Добавить механизм retry с exponential backoff:
  - Настраиваемое количество попыток
  - Обработка rate limiting
  - Обработка временных сетевых ошибок
- [x] 2.5. Добавить логирование через ILogger
- [x] 2.6. Написать unit тесты для HTTP клиента:
  - Моки для HttpClient
  - Тесты успешных запросов
  - Тесты обработки ошибок
  - Тесты retry механизма

**Результат:** Рабочий HTTP клиент с тестами

---

### ФАЗА 3: Базовые модели данных

**Задачи:**
- [x] 3.1. Создать базовые модели ответов:
  - `Response<T>` - обертка для успешного ответа
  - `Error` - модель ошибки API
  - `ErrorResponse` - ответ с ошибкой
- [x] 3.2. Реализовать основные сущности:
  - `User` (id, username, firstName, lastName, и т.д.)
  - `Chat` (id, type, title, members, и т.д.)
  - `Message` (id, chat, from, text, date, и т.д.)
  - `Update` (id, type, message, и т.д.)
- [x] 3.3. Создать Enums:
  - `ChatType` (private, group, channel, и т.д.)
  - `MessageType` (text, image, file, и т.д.)
  - `UpdateType` (message, callback_query, и т.д.)
- [x] 3.4. Настроить JSON сериализацию:
  - JsonPropertyName атрибуты
  - JsonConverter для Enums
  - Обработка nullable полей
  - Обработка вложенных объектов
- [x] 3.5. Добавить валидацию моделей (Data Annotations, опционально)
- [x] 3.6. Написать тесты сериализации/десериализации:
  - Тесты десериализации из JSON документации
  - Тесты сериализации в JSON
  - Тесты nullable полей

**Результат:** Полный набор базовых моделей с тестами

---

### ФАЗА 4: API методы - Core

**Задачи:**
- [x] 4.1. Создать интерфейс `IMaxBotApi` с основными методами
- [x] 4.2. Реализовать `BotApi`:
  - `GetMeAsync()` - информация о боте
  - `GetBotInfoAsync()` - детальная информация о боте
- [x] 4.3. Реализовать `MessagesApi`:
  - `SendMessageAsync(chatId, text, options)` - отправка сообщения
  - `GetMessagesAsync(chatId, options)` - получение сообщений
- [x] 4.4. Реализовать `ChatsApi`:
  - `GetChatAsync(chatId)` - информация о чате
  - `GetChatsAsync(options)` - список чатов
- [x] 4.5. Реализовать `UsersApi`:
  - `GetUserAsync(userId)` - информация о пользователе
- [x] 4.6. Создать главный класс `MaxClient`, реализующий `IMaxBotApi`:
  - Инициализация с токеном
  - Группировка API методов
  - Конфигурация через `MaxBotOptions`
- [x] 4.7. Написать базовые тесты для каждого метода:
  - Unit тесты с моками
  - Тесты валидации параметров
  - Тесты обработки ошибок

**Результат:** Работающие core API методы с тестами

---

### ФАЗА 5: Расширение моделей данных

**Задачи:**
- [x] 5.1. Реализовать все остальные модели из документации:
  - Медиа типы (Photo, Video, Audio, Document, и т.д.)
  - Вложения (Attachments)
  - Кнопки и клавиатуры (InlineKeyboard, ReplyKeyboard)
  - Callback queries
  - И другие сущности из API
- [x] 5.2. Добавить вложенные объекты:
  - Объекты внутри Message
  - Объекты внутри Chat
  - Объекты внутри Update
- [x] 5.3. Обработать все nullable поля из документации
- [x] 5.4. Добавить JSON converters для специфичных типов:
  - DateTime конвертеры
  - Enum конвертеры с fallback
  - Полиморфные типы (если есть)
- [x] 5.5. Обновить тесты сериализации для новых моделей

**Результат:** Полный набор моделей данных, покрывающий всю документацию

---

### ФАЗА 6: Все остальные API методы

**Задачи:**
- [ ] 6.1. Расширить `MessagesApi`:
  - `EditMessageAsync()` - редактирование сообщения
  - `DeleteMessageAsync()` - удаление сообщения
  - `ForwardMessageAsync()` - пересылка сообщения
  - `SendPhotoAsync()` - отправка фото
  - `SendDocumentAsync()` - отправка документа
  - И другие методы работы с сообщениями
- [ ] 6.2. Расширить `ChatsApi`:
  - `CreateChatAsync()` - создание чата
  - `UpdateChatAsync()` - обновление чата
  - `InviteUserAsync()` - приглашение пользователя
  - `LeaveChatAsync()` - выход из чата
  - И другие методы работы с чатами
- [ ] 6.3. Реализовать `FilesApi`:
  - `UploadFileAsync()` - загрузка файла
  - `GetFileAsync()` - получение файла
  - `DownloadFileAsync()` - скачивание файла
- [ ] 6.4. Реализовать все остальные группы методов из документации:
  - Все методы, описанные в API документации
- [ ] 6.5. Написать тесты для каждого нового метода:
  - Unit тесты
  - Тесты интеграции (с моками или реальным API)
  - Тесты edge cases

**Результат:** Полная реализация всех методов API из документации

---

### ФАЗА 7: Обработка событий

**Задачи:**
- [ ] 7.1. Создать интерфейс `IUpdateHandler`:
  - Методы для обработки разных типов событий
  - Поддержка async обработки
- [ ] 7.2. Реализовать `UpdatePoller` (Long Polling):
  - Механизм polling с настраиваемым интервалом
  - Поддержка CancellationToken
  - Обработка ошибок при polling
  - Автоматический retry при сбоях
  - Поддержка offset для получения пропущенных обновлений
- [ ] 7.3. Реализовать интеграцию с MaxClient:
  - `StartPollingAsync(handler, options)`
  - `StopPollingAsync()`
  - События для разных типов обновлений
- [ ] 7.4. Реализовать Webhook поддержку (для ASP.NET Core):
  - `WebhookController` атрибут
  - Маршрутизация webhook эндпоинта
  - Валидация webhook запросов
  - Интеграция с IUpdateHandler
- [ ] 7.5. Написать тесты:
  - Unit тесты для UpdatePoller
  - Тесты обработки разных типов обновлений
  - Тесты webhook контроллера

**Результат:** Рабочий механизм получения и обработки обновлений

---

### ФАЗА 8: Полировка и документация

**Задачи:**
- [ ] 8.1. Добавить XML документацию для всех публичных API:
  - Классы
  - Методы
  - Параметры
  - Возвращаемые значения
  - Примеры использования
- [ ] 8.2. Обновить `README.md`:
  - Описание библиотеки
  - Установка через NuGet
  - Быстрый старт (Quick Start)
  - Примеры использования
  - Документация API (ссылка на XML docs)
  - Contributing guidelines
- [ ] 8.3. Создать примеры использования:
  - Простой echo бот
  - Бот с командами
  - Бот с кнопками
  - Бот с файлами
- [ ] 8.4. Создать `CHANGELOG.md`:
  - Формат Keep a Changelog
  - Версионирование (SemVer)
- [ ] 8.5. Добавить NuGet метаданные:
  - Иконка пакета
  - Tags
  - Project URL
  - License

**Результат:** Полная документация и примеры использования

---

### ФАЗА 9: CI/CD и публикация

**Задачи:**
- [ ] 9.1. Настроить GitHub Actions для CI:
  - Build на push/PR
  - Запуск unit тестов
  - Запуск интеграционных тестов (опционально)
  - Проверка кода (linting)
  - Code coverage отчет
- [ ] 9.2. Настроить NuGet пакет:
  - Автоматическая сборка .nupkg
  - Автоматическая публикация при теге версии
  - Настройка NuGet.org feed
- [ ] 9.3. Создать пример проекта:
  - Рабочий пример бота
  - Инструкции по запуску
- [ ] 9.4. Подготовить первый релиз:
  - Версия 0.1.0-alpha
  - Теги в Git
  - Release notes

**Результат:** Автоматизированный CI/CD и готовый к публикации пакет

---

## 📁 Структура проекта

### Детальная структура файлов

```
MaxBotNet/
├── .editorconfig
├── .gitignore
├── .gitattributes
├── LICENSE
├── README.md
├── CHANGELOG.md
├── DEVELOPMENT_PLAN.md
├── Directory.Build.props          // Общие настройки для всех проектов
├── Max.Bot.sln                    // Solution file
│
├── src/
│   └── Max.Bot/
│       ├── Max.Bot.csproj
│       ├── MaxClient.cs           // Главный клиент
│       │
│       ├── Configuration/
│       │   ├── MaxBotOptions.cs
│       │   └── MaxBotClientOptions.cs
│       │
│       ├── Api/
│       │   ├── IMaxBotApi.cs
│       │   ├── BotApi.cs
│       │   ├── MessagesApi.cs
│       │   ├── ChatsApi.cs
│       │   ├── UsersApi.cs
│       │   ├── FilesApi.cs
│       │   └── UpdatesApi.cs
│       │
│       ├── Types/
│       │   ├── Response.cs
│       │   ├── Error.cs
│       │   ├── User.cs
│       │   ├── Chat.cs
│       │   ├── Message.cs
│       │   ├── Update.cs
│       │   ├── Photo.cs
│       │   ├── Video.cs
│       │   ├── Document.cs
│       │   ├── InlineKeyboard.cs
│       │   └── Enums/
│       │       ├── ChatType.cs
│       │       ├── MessageType.cs
│       │       └── UpdateType.cs
│       │
│       ├── Networking/
│       │   ├── IHttpClient.cs
│       │   ├── MaxHttpClient.cs
│       │   └── MaxApiRequest.cs
│       │
│       ├── Exceptions/
│       │   ├── MaxApiException.cs
│       │   ├── MaxNetworkException.cs
│       │   └── MaxRateLimitException.cs
│       │
│       └── Polling/
│           ├── IUpdateHandler.cs
│           ├── UpdatePoller.cs
│           └── WebhookController.cs
│
├── tests/
│   └── Max.Bot.Tests/
│       ├── Max.Bot.Tests.csproj
│       ├── Unit/
│       │   ├── Api/
│       │   ├── Networking/
│       │   ├── Types/
│       │   └── Polling/
│       ├── Integration/
│       │   └── ApiIntegrationTests.cs
│       └── Examples/
│           └── EchoBotExample.cs
│
├── examples/
│   └── Max.Bot.Examples/          // Опциональный проект с примерами
│       ├── Max.Bot.Examples.csproj
│       └── Program.cs
│
└── docs/                          // Документация API (добавить в .gitignore)
    └── max-api-docs/              // Скачанная документация
```

---

## 🔧 Технические решения

### .NET 9 особенности

- **Target Framework:** `net9.0` (минимальная версия)
- **Nullable Reference Types:** Включены для типобезопасности
- **Language Version:** `latest` для использования всех новых фич
- **System.Text.Json:** Основная библиотека для сериализации (встроена в .NET)
- **Source Generators:** Использовать для производительности (опционально)

### Архитектурные паттерны

1. **Repository/Service Pattern** - для группировки API методов
2. **Factory Pattern** - для создания MaxClient с разными конфигурациями
3. **Observer Pattern** - для обработки событий/обновлений
4. **Strategy Pattern** - для разных типов polling/webhook
5. **Dependency Injection** - для всех зависимостей

### HTTP клиент

- Использовать `IHttpClientFactory` для правильного управления HttpClient
- Поддержка retry с exponential backoff
- Обработка rate limiting (429 ошибки)
- Таймауты для запросов
- Логирование всех HTTP запросов/ответов (в debug режиме)

### Сериализация

- **System.Text.Json** как основная библиотека
- JsonSerializerOptions с настройками:
  - PropertyNamingPolicy = JsonNamingPolicy.CamelCase (или как в API)
  - DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
- Кастомные JsonConverters для специфичных типов

### Обработка ошибок

- Кастомные исключения:
  - `MaxApiException` - общая ошибка API
  - `MaxNetworkException` - сетевые ошибки
  - `MaxRateLimitException` - rate limiting
  - `MaxUnauthorizedException` - неавторизованный доступ
- Retry механизм для временных ошибок
- Логирование всех ошибок

### Тестирование

- **xUnit** как тестовый фреймворк
- **Moq** для моков
- **FluentAssertions** для читаемых assertions
- Unit тесты для всех компонентов
- Integration тесты для реального API (опционально, с тестовым ботом)

---

## 📦 Зависимости

### Основные зависимости (Max.Bot)

```xml
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
```

**Примечание:** `System.Net.Http` встроен в .NET 9, поэтому явная ссылка на `Microsoft.Extensions.Http` не требуется. Для HTTP клиента используется `System.Net.Http.HttpClient` напрямую. Если потребуется расширенная функциональность (например, `IHttpClientFactory`), можно добавить пакет `Microsoft.Extensions.Http` позже.

### Зависимости для тестов (Max.Bot.Tests)

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="FluentAssertions" Version="6.12.1" />
```

**Примечание:** Версии пакетов обновлены до актуальных для .NET 9. `xunit.runner.visualstudio` встроен в .NET 9, но оставлен для совместимости со старыми версиями Visual Studio.

### Опциональные зависимости

```xml
<!-- Для Webhook поддержки (если нужна) -->
<PackageReference Include="Microsoft.AspNetCore.Mvc" Version="2.2.0" />

<!-- Для интеграционных тестов с ASP.NET Core -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
```

---

## ✅ Критерии приемки

### Функциональные требования

- [ ] Все методы API из официальной документации реализованы
- [ ] Все модели данных из документации реализованы
- [ ] Long Polling механизм работает корректно
- [ ] Webhook поддержка работает (если реализована)
- [ ] Обработка ошибок работает на всех уровнях
- [ ] Retry механизм работает для временных ошибок
- [ ] Rate limiting обрабатывается корректно

### Технические требования

- [ ] Код покрыт unit тестами (минимум 80% coverage)
- [ ] Все публичные API имеют XML документацию
- [ ] README содержит примеры использования
- [ ] Проект компилируется без warnings
- [ ] Все тесты проходят успешно
- [ ] NuGet пакет собирается и публикуется

### Качество кода

- [ ] Код следует C# coding conventions
- [ ] Используется .editorconfig для единообразия
- [ ] Нет code smells (проверено анализаторами)
- [ ] Производительность адекватна (нет узких мест)
- [ ] Память используется эффективно (нет утечек)

---

## 📝 Примечания

### Порядок реализации

Этапы должны выполняться последовательно, так как каждый этап зависит от предыдущего. Однако внутри этапа задачи могут выполняться параллельно (например, разработка разных API групп).

### Итеративность

Каждый этап должен завершаться рабочим кодом с тестами. Это позволит:
- Быстро обнаруживать проблемы
- Иметь рабочую версию на каждом этапе
- Упростить отладку

### Версионирование

Использовать Semantic Versioning (SemVer):
- `MAJOR.MINOR.PATCH`
- Начальная версия: `0.1.0-alpha`
- Стабильная версия: `1.0.0`

### Документация

Все публичные API должны иметь XML документацию. Это позволит автоматически генерировать документацию через инструменты типа DocFX.

---

**Версия плана:** 1.0  
**Дата создания:** 2025-01-XX  
**Последнее обновление:** 2025-01-XX
