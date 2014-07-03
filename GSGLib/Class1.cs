using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GSG
{
    public class GSG
    {
        //входные переменные
        //entrance variables
        private int project;
        public string txn_id;
        public int paysystem;
        public string account;
        public double amount;
        public string currency;
        private string secret;
        public string date_from;
        public string date_to;
        public string curr_from;
        public string curr_to;
        public int invoice;
        public string expiry;
        public string name;
        public string phone;

        //конструктор класса
        // project- номер проекта
        // secret - секрет проекта
        // designer of a class
        // project-project number
        // secret - a project secret
        public GSG(int project, string secret)
        {
            this.project = project;
            this.secret = secret;
        }


        // check-запрос - для проверки возможности выплаты
        //обязательные данные можно посмотреть в протоколе
        // check-inquiry - for check of possibility of payment
        // it is possible to look at obligatory data in the protocol
        public Check GetCheck()
        {
            XElement parameters = new XElement("params",
                new XElement("account", this.account),
                new XElement("amount", this.amount.ToString().Replace(",", ".")),
                new XElement("currency", this.currency),
                new XElement("expiry", this.expiry),
                new XElement("name", this.name),
                new XElement("paysystem", this.paysystem),
                new XElement("phone", this.phone),
                new XElement("txn_id", this.txn_id)

                );

            Check check = new Check();
            //объявляем переменную для xml, которую вернет нам главный метод GSG_Start
            // we declare a variable for xml which will return us the main GSG_Start method
            string responce = GSG_Start("check", parameters);
            //разбираем xml по нодам и заполняем переменные нашего класса
            // we sort xml on notes and we fill variables of our class
            XDocument xml = XDocument.Parse(responce);
            check.status = Convert.ToInt16(xml.Element("response").Element("status").Value);
            check.reference = Convert.ToInt64(xml.Element("response").Element("reference").Value);
            check.timestamp = Convert.ToInt64(xml.Element("response").Element("timestamp").Value);

            if (xml.Element("response").Element("invoice") != null)
            {
                check.invoice = Convert.ToInt32(xml.Element("response").Element("invoice").Value);
            }
            if (xml.Element("response").Element("income") != null)
            {
                check.income = Convert.ToDouble(xml.Element("response").Element("income").Value.Replace(".", ","));
                check.income_currency = xml.Element("response").Element("income").Attribute("currency").Value;
            }
            if (xml.Element("response").Element("amount") != null)
            {
                check.amount = Convert.ToDouble(xml.Element("response").Element("amount").Value.Replace(".", ","));
                check.amount_cyrrency = xml.Element("response").Element("amount").Attribute("currency").Value;
            }
            if (xml.Element("response").Element("outcome") != null)
            {
                check.outcome = Convert.ToDouble(xml.Element("response").Element("outcome").Value.Replace(".", ","));
                check.outcome_cyrrency = xml.Element("response").Element("outcome").Attribute("currency").Value;
            }
            //возвращаем то что у нас получилось
            // we return that that at us turned out
            return check;
        }

        // Платежные  системы
        public PaySystem GetPaySystems()
        {
            // получение списка платежных систем
            // obtaining list of payment systems
            string responce;
            responce = GSG_Start("paysystems");
            int n = 10;

            while (responce == "")
            {
                if (n == 0) break;
                responce = GSG_Start("paysystems");
                n = n - 1;
            }

            PaySystem paySystem = new PaySystem();
            //парсим
            // parsing
            XDocument xml = XDocument.Parse(responce);

            paySystem.status = Convert.ToInt32(xml.Element("response").Element("status").Value);
            paySystem.reference = Convert.ToInt32(xml.Element("response").Element("reference").Value);
            paySystem.timestamp = Convert.ToInt32(xml.Element("response").Element("timestamp").Value);

            //если нет доп параметор, возвращаем данные
            // if there is no additional parametor, we return data
            if (xml.Element("response").Element("paysystems") == null) return paySystem;
            // разбор xml
            // sort xml
            List<PaySystems> elem = new List<PaySystems>();

            foreach (XElement xe in xml.Element("response").Element("paysystems").Nodes())
            {
                elem.Add(new PaySystems()
                {
                    id = Convert.ToInt32(xe.Element("id").Value),
                    tag = xe.Element("tag").Value,
                    title = xe.Element("title").Value,
                    jname = xe.Element("jname").Value,
                    region = xe.Element("region").Value,
                    min_amount = Convert.ToDouble(xe.Element("min_amount").Value.Replace(".", ",")),
                    max_amount = Convert.ToDouble(xe.Element("max_amount").Value.Replace(".", ",")),
                    account_name = xe.Element("account_name").Value,
                    account_regexp = xe.Element("account_regexp").Value,
                    currency_id = xe.Element("currency_id").Value,
                    _params = xe.Element("params") != null ? xe.Element("params").Value : null
                });
            }
            paySystem.PaySystems = elem;
            //возвращаем заполненый класс PaySystem
            // we return the filled class PaySystem
            return paySystem;
        }


        //получение курсов валют
        //receiving exchange rates
        public Rate GetRates()
        {
            //генерируем xml
            //generate xml
            XElement parameters = new XElement("params",
                 new XElement("curr_from", this.curr_from),
                new XElement("curr_to", this.curr_to),
                new XElement("date_from", this.date_from),
                new XElement("date_to", this.date_to)
                );
            //забираем данные
            // we take away data
            string responce = GSG_Start("rates", parameters);
            Rate rate = new Rate();
            List<Rates> rates = new List<Rates>();
            //парсим ответ
            // parsy answer
            XDocument xml = XDocument.Parse(responce);
            rate.status = Convert.ToInt16(xml.Element("response").Element("status").Value);
            rate.reference = Convert.ToInt64(xml.Element("response").Element("reference").Value);
            rate.timestamp = Convert.ToInt64(xml.Element("response").Element("timestamp").Value);

            if (xml.Element("response").Element("rates") == null) return rate;

            foreach (XElement xe in xml.Element("response").Element("rates").Nodes())
            {
                rates.Add(new Rates()
                {
                    date = xe.Element("date").Value,
                    curr_from = xe.Element("curr_from").Value,
                    curr_to = xe.Element("curr_to").Value,
                    conversion_rate = xe.Element("conversion_rate").Value
                });
            }
            rate.rates = rates;
            return rate;
        }

        //главный метод, который генерирует запросы в систему GSG, подписывает и забирает ответ
        // the main method which generates inquiries in GSG system, signs and takes away the answer
        private string GSG_Start(string action, XElement parameter = null)
        {
            //unix-время
            // unix-time
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string signString;
            XDocument XML;
            //генерация xml
            // xml generation
            XML = new XDocument(
                           new XDeclaration("1.0", "utf-8", "yes"),
                           new XElement("request",
                               new XElement("action", action),
                               new XElement("project", this.project),
                               new XElement("timestamp", unixTime)
                           )
                       );
            //проверяем есть ли параметры
            // whether we check there are parameters
            if (parameter != null)
            {
                string elem = "";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(parameter.ToString());
                // разираем данные для генерации подписи
                // sort the signatures given for generation
                foreach (XmlNode node in doc.SelectNodes("params"))
                {
                    elem += node.InnerText;
                }
                // создание подписи
                // signature creation
                signString = Convert.ToString(unixTime) + Convert.ToString(this.project) + action + elem + this.secret;
                XML.Element("request").Add(parameter);
                //Добавляем подпись к запросу
                // We add the signature to inquiry
                XML.Element("request").Add(new XElement("sign", GetMd5Hash(signString)));
            }
            else
            {
                signString = Convert.ToString(unixTime) + Convert.ToString(this.project) + action + this.secret;
                XML.Element("request").Add(new XElement("sign", GetMd5Hash(signString)));
            }
            //собираем POST
            //collect POST
            String postData = XML.Declaration + XML.ToString();
            //составляем запрос
            //make inquiry
            Byte[] byteDate = Encoding.GetEncoding("utf-8").GetBytes(postData);
            HttpWebRequest myHttpWebRequest;

            myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://gsg.dengionline.com/api");
            myHttpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;";
            myHttpWebRequest.ContentLength = byteDate.Length;
            myHttpWebRequest.Method = "POST";


            using (Stream requestStream = myHttpWebRequest.GetRequestStream())
            {
                requestStream.Write(byteDate, 0, byteDate.Length);
                requestStream.Flush();
                requestStream.Close();
            }

            HttpWebResponse postreqresponce = (HttpWebResponse)myHttpWebRequest.GetResponse();
            Stream stream = (Stream)myHttpWebRequest.GetResponse().GetResponseStream();
            StreamReader reader;
            //считываем ответ от сервиса GSG
            // read out the answer from the GSG service
            reader = new StreamReader(stream);

            string s;
            s = reader.ReadToEnd();
            stream.Close();
            postreqresponce.Close();
            //возвращаем ответ
            // return the answer
            return s;
        }
        // проверка основного баланса
        // check of the main balance
        public MainBalance GetMainBalance()
        {
            //забираем ответ
            // we take away the answer
            string responce = GSG_Start("main_balance");

            MainBalance mainBalance = new MainBalance();
            //парсим xml
            // we sort xml
            XDocument xml = XDocument.Parse(responce);
            mainBalance.status = Convert.ToInt16(xml.Element("response").Element("status").Value);
            mainBalance.reference = Convert.ToInt64(xml.Element("response").Element("reference").Value);
            mainBalance.timestamp = Convert.ToInt64(xml.Element("response").Element("timestamp").Value);
            //если нет данных возвращаем то что пришло
            // if there are no data return that that came
            if (xml.Element("response").Element("balance") == null && xml.Element("response").Element("currency") == null) return mainBalance;
            mainBalance.balance = Convert.ToDouble(xml.Element("response").Element("balance").Value.Replace(".", ","));
            mainBalance.currency = Convert.ToInt32(xml.Element("response").Element("currency").Value);
            return mainBalance;
        }

        //вывод всех возможных статусов ошибок
        // conclusion of all possible statuses of mistakes
        public Error GetErrors()
        {
            string responce = GSG_Start("errors");
            Error error = new Error();
            XDocument xml = XDocument.Parse(responce);
            error.status = Convert.ToInt16(xml.Element("response").Element("status").Value);
            error.reference = Convert.ToInt64(xml.Element("response").Element("reference").Value);
            error.timestamp = Convert.ToInt64(xml.Element("response").Element("timestamp").Value);
            //проверяем есть ли данные 
            // whether we check there are data
            if (xml.Element("response").Element("errors") == null) return error;

            List<Errors> errors = new List<Errors>();
            foreach (XElement xe in xml.Element("response").Element("errors").Nodes())
            {
                errors.Add(new Errors()
                {
                    id = Convert.ToInt32(xe.Element("id").Value),
                    descr = xe.Element("descr").Value
                });
            }
            error.errors = errors;
            //возвращаем
            //return
            return error;
        }

        //операция Pay, происходит после Check-операции для оплаты
        // the operation Pay, happens after Check-operation for payment
        public Pay GetPay()
        {
            //генерируем xml
            // we generate xml
            XElement parameters = new XElement("params",
                new XElement("amount", this.amount.ToString().Replace(",", ".")),
                new XElement("currency", this.currency),
                new XElement("invoice", this.invoice),
                new XElement("txn_id", this.txn_id)
                );
            //забираем ответ
            // we take away the answer
            string responce = GSG_Start("pay", parameters);

            Pay pay = new Pay();
            //заполняем наш класс
            // we fill our class
            XDocument xml = XDocument.Parse(responce);
            pay.status = Convert.ToInt16(xml.Element("response").Element("status").Value);
            pay.reference = Convert.ToInt64(xml.Element("response").Element("reference").Value);
            pay.timestamp = Convert.ToInt64(xml.Element("response").Element("timestamp").Value);
            //если нет данных, возвращаем то что есть
            // if there are no data, return that that is
            if (xml.Element("response").Element("invoice") == null) return pay;

            pay.invoice = Convert.ToInt32(xml.Element("response").Element("invoice").Value);
            pay.income = Convert.ToDouble(xml.Element("response").Element("income").Value.Replace(".", ","));
            pay.rate = Convert.ToDouble(xml.Element("response").Element("rate").Value.Replace(".", ","));
            pay.amount = Convert.ToDouble(xml.Element("response").Element("amount").Value.Replace(".", ","));
            pay.outcome = Convert.ToDouble(xml.Element("response").Element("outcome").Value.Replace(".", ","));
            pay.fee = Convert.ToDouble(xml.Element("response").Element("fee").Value.Replace(".", ","));

            return pay;

        }

        //проверка статуса платежа 
        // check of the status of payment
        public PayStatus GetPayStatus()
        {

            XElement parameters = new XElement("params",
                new XElement("invoice", this.invoice),
                new XElement("txn_id", this.txn_id)
                );

            string responce = GSG_Start("pay_status", parameters);
            XDocument xml = XDocument.Parse(responce);
            PayStatus payStatus = new PayStatus();

            payStatus.status = Convert.ToInt16(xml.Element("response").Element("status").Value);
            payStatus.reference = Convert.ToInt64(xml.Element("response").Element("reference").Value);
            payStatus.timestamp = Convert.ToInt64(xml.Element("response").Element("timestamp").Value);

            if (xml.Element("response").Element("pay_status") == null) return payStatus;

            payStatus.pay_status = xml.Element("response").Element("pay_status").Value;
            payStatus.income = Convert.ToDouble(xml.Element("response").Element("income").Value.Replace(".", ","));
            payStatus.rate = Convert.ToDouble(xml.Element("response").Element("rate").Value.Replace(".", ","));
            payStatus.amount = Convert.ToDouble(xml.Element("response").Element("amount").Value.Replace(".", ","));
            payStatus.outcome = Convert.ToDouble(xml.Element("response").Element("outcome").Value.Replace(".", ","));
            payStatus.fee = Convert.ToDouble(xml.Element("response").Element("fee").Value.Replace(".", ","));
            payStatus.ts_create = Convert.ToDateTime(xml.Element("response").Element("ts_create").Value);
            if (xml.Element("response").Element("ts_close").Value != "0000-00-00 00:00:00")
            {
                payStatus.ts_close = Convert.ToDateTime(xml.Element("response").Element("ts_close").Value);
            }
            //возвращаем данные
            // we return data
            return payStatus;
        }

        //генерация md5 из  строки
        // md5 generation from a line
        private static string GetMd5Hash(string input)
        {

            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

    }

    // Классы для удобного управления данными
    // Classes for a convenient data control
    public class Check
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public int invoice;
        public double income;
        public string income_currency;
        public double amount;
        public string amount_cyrrency;
        public double outcome;
        public string outcome_cyrrency;
        public double rate_income;
        public double rate_outcome;
        public double rate_total;
    }
    public class PayStatus
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public string pay_status;
        public double income;
        public double rate;
        public double amount;
        public double outcome;
        public double fee;
        public DateTime ts_create;
        public DateTime ts_close;
    }
    public class Pay
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public int invoice;
        public double income;
        public double rate;
        public double amount;
        public double outcome;
        public double fee;
    }
    public class Error
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public List<Errors> errors;
    }
    public class Errors
    {
        public int id;
        public string descr;
    }

    public class MainBalance
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public double balance;
        public int currency;
    }
    public class Rate
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public List<Rates> rates;
    }
    public class Rates
    {
        public string date;
        public string curr_from;
        public string curr_to;
        public string conversion_rate;
    }
    public class PaySystem
    {
        public int status;
        public Int64 reference;
        public Int64 timestamp;
        public List<PaySystems> PaySystems;
    }
    public class PaySystems
    {
        public int id;
        public string tag;
        public string title;
        public string jname;
        public string region;
        public double min_amount;
        public double max_amount;
        public string account_name;
        public string account_regexp;
        public string currency_id;
        public string _params;
    }
}
