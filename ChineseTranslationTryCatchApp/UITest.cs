using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Point = System.Drawing.Point;

namespace ChineseTranslationTryCatchApp
{
    public partial class UITest
    {
        static Point generalPosition;
        private int rowNumber = 3;// надо передать
        private static int collumNumber = 2;
        private static string[,] translations;
        private static Grid mainGrid;
        private ScrollViewer MainScrollViewer;


    }
}
