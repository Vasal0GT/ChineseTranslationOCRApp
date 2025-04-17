using Gu.Wpf.UiAutomation.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChineseTranslationTryCatchApp
{
    //создал этот класс просто для потенциальных проверок,
    internal class Checking
    {
        public static bool ContainsChinese(string input)
        {
            // Регулярное выражение для поиска китайских иероглифов (основной диапазон китайских символов)
            string pattern = @"[\u4e00-\u9fff]";

            // Проверяем, соответствует ли строка шаблону
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        public static string CheckScpacesAndRemove(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            char[] result = new char[str.Length];
            int index = 0;

            foreach (char c in str)
            {
                if (c != ' ')
                {
                    result[index] = c;
                    index++;
                }
            }

            return new string(result, 0, index);
        }
    }
}
