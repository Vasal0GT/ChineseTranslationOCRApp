using ChineseTranslationTryCatchApp;
using ChineseTranslationTryCatchApp.ParseComponent;
using ChineseTranslationTryCatchApp.TesseractOCR;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Point = System.Drawing.Point;
using System.Threading;
using System.Timers;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using Brushes = System.Windows.Media.Brushes;




namespace ChineseTranslationThrougOCR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string CapturedChineseCherecter = "";
        private static DispatcherTimer reLoadTimer;
        private NotifyIcon trayIcon;
        private static string lastCapturedText = "";
        private static int TimerFromMilisecond = 100;
        private static string[,] translations;// тут лежит основной массив, надо его передавать
        private static int rowNumber;
        private static int collumNumber = 2;
        private static Grid mainGrid;
        private ScrollViewer MainScrollViewer;
        static Point generalPosition;
        private static int emptyRowCount;
        private bool isLeftMouseButtonDown = false;
        private LoadWindow loadingWindow;

        public MainWindow()
        {
            InitializeComponent();
            StartTimerAndCheckClipBoard();
        }
        private void StartTimerAndCheckClipBoard()
        {
            reLoadTimer = new DispatcherTimer();
            reLoadTimer.Interval = TimeSpan.FromMilliseconds(TimerFromMilisecond);
            reLoadTimer.Tick += CheckActiveField;          
            reLoadTimer.Start();
                this.MouseEnter += Window_MouseEnter;
                this.MouseLeave += Window_MouseLeave;
        }

        private void CheckActiveField(object sender, EventArgs e)
        {
            string selectedText = GetSelectedTextFromActiveElement();
            CapturedChineseCherecter = Checking.CheckScpacesAndRemove(selectedText);
            Console.WriteLine("Selected Text: " + selectedText);
            selectedText = Checking.CheckScpacesAndRemove(selectedText);

            if (Checking.ContainsChinese(selectedText))
            {
                CreateLoadingWindowGuts();
                this.Hide();
                loadingWindow.Show();
                
                System.Windows.Application.Current.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(delegate { })
    );

                string[,] parseElements = Parse.FindTranslationReturnStringArray(selectedText);// раскидывает текст по двумерному массиву строк
                Parse.CheckArray(parseElements);
                translations = RemoveEmptyRows(parseElements, out emptyRowCount);
                rowNumber = Parse.findNNumber(selectedText);
                FillWindow();
                loadingWindow.Hide();
                this.Left = SystemParameters.PrimaryScreenWidth - this.Width - 8;
                this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 80;
                this.Topmost = true;
                this.Show();
            }

            if (!string.IsNullOrEmpty(selectedText))
            {
                Console.WriteLine("Text has changed, updating label.");
                lastCapturedText = selectedText;
            }            
        }

        private void CreateLoadingWindowGuts()
        {
            loadingWindow = new LoadWindow();
            loadingWindow.Height = this.Height;
            loadingWindow.Width = this.Width;
            loadingWindow.Left = SystemParameters.PrimaryScreenWidth - this.Width - 8;
            loadingWindow.Top = SystemParameters.PrimaryScreenHeight - this.Height - 80;
            loadingWindow.Topmost = true;

            // Создаем Label
            var loadingLabel = new System.Windows.Controls.Label
            {
                Content = "Загрузка, подождите...",
                FontSize = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Black,
                Background = Brushes.White // Убедимся, что фон виден
            };

            // Используем новый Grid и задаем фон
            var grid = new Grid
            {
                Background = Brushes.White // Добавляем фон
            };
            grid.Children.Add(loadingLabel);

            loadingWindow.Content = grid; // Явно устанавливаем Content
        }
        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //this.MouseLeave -= Window_MouseLeave;
            reLoadTimer.Start();
            Console.WriteLine("TIMER STARTED!!!");
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //this.MouseEnter -= Window_MouseEnter;
            reLoadTimer.Stop();
            Console.WriteLine("TIMER STOPPED!!!");
        }


        static string[,] RemoveEmptyRows(string[,] array, out int emptyRowCount)
        {
            List<string[]> filteredRows = new List<string[]>();
            emptyRowCount = 0; // Инициализируем счётчик

            for (int i = 0; i < array.GetLength(0); i++)
            {
                if (!string.IsNullOrWhiteSpace(array[i, 0]) || !string.IsNullOrWhiteSpace(array[i, 1]))
                {
                    filteredRows.Add(new string[] { array[i, 0], array[i, 1] });
                }
                else
                {
                    emptyRowCount++; // Увеличиваем счётчик, если строка пустая
                }
            }

            // Создаём новый двумерный массив нужного размера
            string[,] newArray = new string[filteredRows.Count, 2];
            for (int i = 0; i < filteredRows.Count; i++)
            {
                newArray[i, 0] = filteredRows[i][0];
                newArray[i, 1] = filteredRows[i][1];
            }

            return newArray;
        }
        void FillWindow()
        {
            CreateCanvasWithRow();
            FillWindowWithString();
        }
        private void CreateCanvasWithRow()
        {
                mainGrid = new Grid()
                {
                    Background = System.Windows.Media.Brushes.LightGray,
                    ShowGridLines = true,
                };
                
                MainScrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = mainGrid
                };



                this.Content = MainScrollViewer;

            // Очистка перед добавлением новых строк и колонок
            mainGrid.RowDefinitions.Clear();
            mainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < rowNumber; i++)
            {
                mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            for (int i = 0; i < collumNumber; i++)
            {
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition() );
            }
        }

        private void FillWindowWithString()
        {
            mainGrid.Children.Clear(); // Очищаем только содержимое, не пересоздавая Grid

            for (int i = 0; i < rowNumber - emptyRowCount; ++i)
            {
                for (int j = 0; j < collumNumber; j++)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = translations[i, j],
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5)
                    };

                    Grid.SetRow(textBlock, i);
                    Grid.SetColumn(textBlock, j);
                    mainGrid.Children.Add(textBlock);
                }
            }
        }

        static string GetSelectedTextFromActiveElement()
        {
            var grabber = new StealthTextGrabber();
            return grabber.CaptureSelectedAreaAsync().GetAwaiter().GetResult();
        }
        #region UI

        public static Point GetMousePositionWindowsForms()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            generalPosition.X = point.X;
            generalPosition.Y = point.Y;
            return new Point(point.X, point.Y);
        }
        private void SetWindowPosition(object sender, EventArgs e)
        {
            GetMousePositionWindowsForms();
            double dpiScale = GetDpiForWindow(this) / 96.0;

            CreateCollumAndRow(); // Теперь не создаёт новый Grid, а обновляет существующий
            FillGrid(); // Заполняем без пересоздания

            this.Left = (generalPosition.X / dpiScale) + 5;
            this.Top = (generalPosition.Y / dpiScale) + 5;
            
        }
        private void CreateCollumAndRow()
        {
            if (mainGrid == null) // Создаём только если ещё не создано
            {
                mainGrid = new Grid()
                {
                    Background = System.Windows.Media.Brushes.LightGray,
                    ShowGridLines = true,
                };

                MainScrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = mainGrid
                };

                this.Content = MainScrollViewer;
            }

            // Очистка перед добавлением новых строк и колонок
            mainGrid.RowDefinitions.Clear();
            mainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < rowNumber; i++)
            {
                mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            for (int i = 0; i < collumNumber; i++)
            {
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void FillGrid()
        {
            mainGrid.Children.Clear(); // Очищаем только содержимое, не пересоздавая Grid

            for (int i = 0; i < rowNumber - emptyRowCount; i++)
            {
                for (int j = 0; j < collumNumber; j++)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = translations[i, j],
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5)
                    };

                    Grid.SetRow(textBlock, i);
                    Grid.SetColumn(textBlock, j);
                    mainGrid.Children.Add(textBlock);
                }
            }
        }

        private static int GetDpiForWindow(System.Windows.Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            return GetDpiForWindow(hwnd);
        }


        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hwnd);


        #endregion

    }
}

