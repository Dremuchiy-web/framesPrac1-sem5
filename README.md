## Практика №1 — мини веб‑служба и конвейер обработки запросов

### Идея предметной области
Сервис хранит **учебные задачи** (title + points) в памяти процесса и даёт:
- `GET /api/items` — список
- `GET /api/items/{id}` — один элемент по id
- `POST /api/items` — создать элемент

### Конвейер обработки запроса (middleware)
Запрос проходит последовательно через обработчики:
- **`RequestIdMiddleware`**: назначает/пробрасывает `X-Request-Id`, кладёт его в контекст запроса
- **`RequestTimingMiddleware`**: измеряет время выполнения и пишет его в лог
- **`RequestLoggingMiddleware`**: логирует старт/финиш запроса (метод/путь/статус) + `requestId`
- **`ExceptionHandlingMiddleware`**: превращает исключения в единый JSON‑ответ об ошибке

Единый формат ошибки:

```json
{ "code": "...", "message": "...", "requestId": "..." }
```

`requestId` дублируется в заголовке ответа `X-Request-Id` — по нему можно найти запись в журнале.

### Валидация (минимум 2 правила)
При создании элемента проверяется:
- `title` не пустой (после trim), длина \(\le 80\)
- `points` неотрицательный, \(\le 10000\)

### Запуск

```bash
dotnet build Task1Framework.sln -c Release
dotnet run --project Task1.Framework
```

По умолчанию сервис стартует на URL, который напечатает в консоль (Kestrel).

### Быстрая ручная проверка (curl)
Предположим, сервис работает на `http://localhost:5000`.

Получить список:

```bash
curl -s http://localhost:5000/api/items | jq .
```

Создать элемент:

```bash
curl -i -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{"title":"Сделать мини сервис","points":7}'
```

Получить по id (подставьте id из ответа POST):

```bash
curl -s http://localhost:5000/api/items/1 | jq .
```

Проверка 404 в едином формате:

```bash
curl -i http://localhost:5000/api/items/999999
```

Проверка 400 при невалидных данных:

```bash
curl -i -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{"title":"   ","points":-1}'
```

### Автотесты
Интеграционные тесты проверяют:
- POST → затем GET по id возвращает созданный элемент
- GET по несуществующему id даёт 404 + единый формат ошибки и `X-Request-Id`
- POST с невалидными данными даёт 400 + единый формат ошибки

Запуск:

```bash
dotnet test Task1Framework.sln -c Release
```

### Наблюдения/измерения
В каждом запросе в логах есть `requestId` и время выполнения (ms) — это позволяет сравнивать сценарии (например, список vs создание) и связывать ответ об ошибке с записью в журнале.

