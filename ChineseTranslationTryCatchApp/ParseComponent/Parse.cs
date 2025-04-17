using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net.Http;
using System.Threading;

namespace ChineseTranslationTryCatchApp.ParseComponent
{
    internal class Parse
    {
        private static string globalKeyWord;
        private static readonly HttpClient client = new HttpClient();
        internal static string[,] FindTranslationReturnStringArray(string chinese)
        {
            var keyWord = chinese;
            globalKeyWord = keyWord;
            ChromeOptions option = new ChromeOptions();
            option.AddArgument("headless");
            option.AddArgument("--window-size=1920x1080");
            option.AddArgument("--disable-gpu");
            option.AddArgument("--log-level=3");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            // Скрытие консольного окна
            service.HideCommandPromptWindow = true;

            IWebDriver driver = new ChromeDriver(service, option);

            driver.Navigate().GoToUrl($"https://bkrs.info/slovo.php?ch={Uri.EscapeDataString(keyWord)}");

            Thread.Sleep(2000); // 2 секунды (для примера)

            string PinYin;
            string Cherecter;
            string translation;

            try// тут происходит тот вариант когда один иероглиф
            {
                var xpathToPinyin = "//div[@id='container']//div[@id='main']//div[@id='ajax_search']//div[@class='margin_left']//div[@class='py']//span[@class='pinYinWrapper']";
                var elementPinyin = driver.FindElement(By.XPath(xpathToPinyin));
                PinYin = elementPinyin.Text;

                var xpathToCherecter = "//div[@id='container']//div[@id='main']//div[@id='ajax_search']//div[@class='margin_left']//div[@id='ch']";
                var elementCherecter = driver.FindElement(By.XPath(xpathToCherecter));
                Cherecter = elementCherecter.Text;

                var xpathToTranslation = "//div[@id='container']//div[@id='main']//div[@id='ajax_search']//div[@class='margin_left']//div[@class='ru']";
                var elementTranslation = driver.FindElement(By.XPath(xpathToTranslation));
                translation = elementTranslation.Text;
                //создалим вручную и тут массив строк, чтоб было универсальнее

                //Дальше сохдаем двумерный массив из двух колоннок и одной строки
                string[,] bkrsTableTranslations =
                {
                    { Cherecter,  PinYin+translation}
                };
                driver.Quit();
                return bkrsTableTranslations;
            }

            catch // тут происходит вариант когда соотвественно не один иероглиф
            {
                int numberOfElements = (findNNumber(keyWord));
                // внутри находим количество элементов для массива

                //Дальше сохдаем двумерный массив из двух колоннок и найденного нами выше количества строк
                string[,] bkrsTableTranslations = new string[numberOfElements, 2];
                FillArray(numberOfElements, bkrsTableTranslations);
                driver.Quit();
                return bkrsTableTranslations;
            }
        }

        private static void FillArray(int numberOfElements, string[,] bkrsTableTranslations)
        {
            //заполняем первую колонку с иероглифами
            for (int i = 0; i < numberOfElements; i++)
            {
                string chineseCherecter = ToFindElementCherecterFromN(i + 1);
                bkrsTableTranslations[i, 0] = chineseCherecter;
            }
            // заполняем вторую колонку с основной инфой
            for (int i = 0; i < numberOfElements; i++)
            {
                string chineseCherecter = ToFindElementTranslationFromN(i + 1);
                bkrsTableTranslations[i, 1] = chineseCherecter;
            }
        }

        private static string ToFindElementTranslationFromN(int i)
        {
            string answer;
            var url = $"https://bkrs.info/slovo.php?ch={Uri.EscapeDataString(globalKeyWord)}";

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);

            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode($"//td[contains(@class, 'vtop td_trans')][@n='{i}']");

            if (node != null)
            {
                string text = node.InnerText.Trim();
                answer = text;
                return answer;
            }
            else
            {
                return "Error";
            }
        }

        private static string ToFindElementCherecterFromN(int i)
        {
            string answer;
            var url = $"https://bkrs.info/slovo.php?ch={Uri.EscapeDataString(globalKeyWord)}";

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);

            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode($"//td[contains(@class, 'pr25 ch3 len3')][@n='{i}']");

            if (node != null)
            {
                string text = node.InnerText.Trim();
                answer = text;
                return answer;
            }
            else
            {
                return "Error";
            }
        }

        internal static void CheckArray(string[,] bkrsTableTranslations)
        {
            for (int i = 0; i < bkrsTableTranslations.GetLength(0); i++) // Количество строк
            {
                for (int j = 0; j < bkrsTableTranslations.GetLength(1); j++) // Количество столбцов
                {
                    Console.Write(bkrsTableTranslations[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }

        public static int findNNumber(string keyWord)
        {
            int answer = 0;
            var html = $"https://bkrs.info/slovo.php?ch={Uri.EscapeDataString(keyWord)}";

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//td[contains(@class, 'pr25 ch3 len3')][@n]");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string nValue = node.GetAttributeValue("n", "нет данных");
                    answer++;
                }
            }
            else
            {
                return answer;
            }
            return answer;
        }
    }
}
