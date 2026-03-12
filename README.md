# 🔍 ChineseTranslationOCR

> WPF-приложение для мгновенного перевода китайских иероглифов прямо с экрана.  
> Выдели область — получи пиньинь и перевод, не переключая окно.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET_Framework-4.8-purple)
![Language](https://img.shields.io/badge/language-C%23-green)

---

## ✨ Как это работает

1. Запускаешь приложение — оно сворачивается и ждёт в фоне
2. Зажимаешь **ЛКМ** и выделяешь область экрана с иероглифами
3. Приложение делает скриншот выделенной области
4. **TesseractOCR** распознаёт иероглифы с изображения
5. **Selenium** открывает [BKRS](https://bkrs.info) и получает пиньинь + перевод
6. Результат появляется в маленьком окошке рядом с курсором

---

## 🛠️ Технологии

| Библиотека | Назначение |
|---|---|
| **Tesseract 5.2** | OCR-распознавание иероглифов (chi_sim, eng, rus) |
| **Selenium WebDriver 4.28** + ChromeDriver | Парсинг BKRS для пиньиня и перевода |
| **HtmlAgilityPack 1.11** | Парсинг HTML-ответа |
| **WPF / .NET Framework 4.8** | Desktop UI, overlay-окно у курсора |
| **System.Drawing** | Захват и обработка скриншота области |
| **Gu.Wpf.UiAutomation** | UI Automation для глобального хука мыши |

---

## 🚀 Запуск

### Требования
- Windows 10/11
- .NET Framework 4.8
- Google Chrome (актуальная версия)

### Установка

```bash
git clone https://github.com/Vasal0GT/ChineseTranslationOCRApp.git
cd ChineseTranslationOCRApp
