# BudgetTracker

[![Build Status](https://semaphoreci.com/api/v1/***REMOVED***/budgettracker/branches/master/shields_badge.svg)](https://semaphoreci.com/***REMOVED***/budgettracker)
[![Docker Pulls](https://img.shields.io/docker/pulls/***REMOVED***/budgettracker.svg)](https://hub.docker.com/r/***REMOVED***/budgettracker)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FDiverOfDark%2FBudgetTracker.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FDiverOfDark%2FBudgetTracker?ref=badge_shield)

## Описание
BudgetTracker - это персональное self-hosted решение для управления личными финансами и инвестициями. 
Основная задачу, которую решает BT - это ежедневный автоматический сбор данных и построение отчетности.

## Как запустить
Для запуска нужен аккаунт в MSFT Azure Storage.
Рекомендуемый способ запуска - Docker. Пример docker-compose файла:

#### docker-compose.yml
``` 
version: "3.3"
services:
  budgettracker:
    image: ***REMOVED***/budgettracker:master
    restart: unless-stopped
    environment:
      Properties__IsProduction: 'true' # true если необходимо сохранять изменения в базу. false для локального запуска/отладки.
      ConnectionStrings__AzureStorage: 'DefaultEndpointsProtocol=https;AccountName=...;AccountKey=ABC==;EndpointSuffix=core.windows.net' # Строка подключения к Azure Storage. 
      # ApplicationInsights__InstrumentationKey: '' # Ключ к Azure Application Insights, если нужна аналитика
    ports:
      - "80:80"
    networks:
      public: {}

networks:
  public:
    driver: bridge
```


## Источники данных:
На данный момент поддерживаются следующие источники данных:
- **АльфаБанк**
- **АльфаКапитал** _требуется SMS-интеграция_
- **АльфаПоток**
- **Райффайзен банк**
- **МодульБанк** _требуется SMS-интеграция_
- **МодульДеньги**
- **FX**: Биржевые курсы валют USD/RUB, EUR/RUB, индекса S&P 500
- **LiveCoin**
- **Penenza**
- **API** (POST-endpoint)
  Пример запроса:
  ```
  POST /post-data
  name=Название+счета&value=1000.0&ccy=RUB
  ```

## Табличное представление (история)

Из каждого источника данных ежедневно собираются данные в общее табличное представление.
Способ сбора данных - Selenium + ChromeWebDriver.

![Пример](docs/images/history.jpg)

В случае нехватки каких-то данных(например - неуспешный парсинг) соответствующая ячейка таблицы подсвечивается черным фоном, а в подсказке видно каких данных не хватает.

Каждое значение характеризуется провайдером и названием счёта через знак ```/```. Например - _Альфа-Банк/Блиц-доход-USD_ или _FX/USD/RUB_
На основе этих данных можно строить свои вычисляемые стобцы - например посчитать сумму всего капитала из разных источников данных с конвертацией курса валют.

Примеры таких функций:
```
[Альфа-Банк/Блиц-доход-USD] * [FX/USD/RUB] + [Альфа-Банк/Блиц-доход-EUR] * [FX/EUR/RUB]
```
![Вычисляемые столбцы](docs/images/computed-columns.jpg)

## Дашборд (отчёт)

На основной странице доступна система виджетов, которые берут свои значения из табличного представления.
![Пример](docs/images/dashboard.jpg)


## Интеграция с SMS:
В настоящее время проверена интеграция только с Android телефонами с использованием IFTTT и Tasker.
Для IFTTT используется простой рецепт с получением Android SMS и отправкой на **/sms**.
Для Tasker используется отправка на **/sms-tasker**. Подробнее см. код _ApiController.cs_.

Для смс есть различные правила - на текущий момент они описываются регулярными выражениями для того чтобы автоматически скрывать ненужные смс (например с кодами подтверждений) и учитывать траты из SMS, полученных от банка.

![Скриншот](docs/images/sms.jpg)


## Учет расходов

### **В ближайших планах переписать это на учет движения денежных средств напрямую из банковских выписок**

В настоящий момент учет расходов производится из SMS, полученных с телефона.
Для этого надо в раздел с SMS добавить правило обработки вида "траты" с регулярным выражением для разбора текста сообщений, например для Raiffeisen:
```
Karta \*3436;\s+Pokupka:(?<sum>[\d.]*) (?<ccy>[A-Z]{3});\s+(?<what>.*);\s+
```
Обязательно наличие именованных групп **sum**, **ccy**, **what** с суммой траты, валютой, и описанием траты соответственно.

Для дружелюбного представления и группировки трат есть понятие категорий. Категории также используя регулярное выражение группируют траты по описанию трат. Пример использования категорий - группировка трат на такси:

```
| Категория | Щаблон           |
| --------- | ---------------- | 
| Taxi	    | .*GETT.*         |
| Taxi    	| .*UBER.*	       |
| Taxi	    | .*Yandex\.Taxi.* |
```


## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FDiverOfDark%2FBudgetTracker.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FDiverOfDark%2FBudgetTracker?ref=badge_large)