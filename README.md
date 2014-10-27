Библиотека-клиент GS на C#
===

Описание протокола: http://dengionline.com/dev/protocol/gsg_protocol

##Примеры использования:

### check
```C#
var check = new GSG(1290, "d9b1d7db4cd6e70935368a1efb10e3771")
            {
                txn_id = "test1",
                account = "9123456789",
                paysystem = 1,
                amount = 10,
                currency = "RUB"
            };
Check result = check.GetCheck();
```
После выполнения метода, в result будут доступны следующие данные:
Имя параметра | Описание
--- | --- 
amount |	Сумма платежа в валюте баланса.Будет списана в момент оплаты
amount_currency |	Валюта платежа/баланса проекта
income |	Запрошенная сумма
income_currency |	Валюта запрошенной суммы
invoice |	Номер платежа в системе ДеньгиOnline
outcome |	Сумма платежа в валюте платежной системы
outcome_currency |	Валюта выплаты в ПС
rate_income |	Конвертация из валюты выставленного счета в валюту баланса
rate_outcome |	Конвертация из валюты баланса в валюту ПС
rate_total |	Итоговый курс конвертации
reference |	Номер запроса
status |	Статус запроса
timestamp |	Время запроса


### 
