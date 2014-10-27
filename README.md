Библиотека-клиент GS на C#
===

Описание протокола: http://dengionline.com/dev/protocol/gsg_protocol

## Подключение и использование

Библиотека компилируется и подключается к проекту через **Проект -> Добавить ссылку**

### Свойства класса

Название параметра | Тип переменной | Описание | В каких методах используется
txn_id | string | Номер платежа в системе проекта | Check,Pay
paysystem | int | Номер платежной системы | Check
account | string | Аккаунт | Check,Pay
amount | 	double | 	Сумма платежа | 	Check,Pay
currency | 	string | 	Требуемая валюта | 	Check,Pay
invoice | 	int | 	Инвойс | 	Check,Pay
expiry | 	string | 	Дата истечения срока дейстия карты	 | Check,Pay
name | 	string | 	ФИО получателя платежа | 	Check,Pay
phone | 	string | 	Телефон | 	Check,Pay



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


### Pay

Запрос: 

```C#
var pay = new GSG(1290, "d9b1d7db4cd6e70935368a1efb10e3771")
            {
               invoice = 100
            };
Pay result = check.GetPay();
```

После выполнения метода, в result будут доступны следующие данные:

Параметр | Описание
income | сумма входящего платежа в его валюте
rate | использованный курс конвертации
amount | сумма входящего платежа в валюте основного баланса
outcome | сумма выплаты в систему-получатель
fee | комиссия в валюте получателя
invoice | ID инвойса


### Paystatus

Запрос: 

```C#
var paystatus= new GSG(1290, "d9b1d7db4cd6e70935368a1efb10e3771")
            {
               invoice = 100,
               txn_id = 100
            };
Paystatus result = paystatus.GetPayStatus();
```

После выполнения метода, в result будут доступны следующие данные:

Параметр | Описание
--- | ---
pay_status | Статус платежа
income | Запрошенная сумма
rate | Курс конвертации
amount | Сумма списания с баланса
outcome | Сумма в валюте ПС
fee | Комиссия
ts_create | Дата создания инвойса
ts_close | Дата закрытия инвойса

