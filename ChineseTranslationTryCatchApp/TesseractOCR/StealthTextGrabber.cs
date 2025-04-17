using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tesseract;

namespace ChineseTranslationTryCatchApp.TesseractOCR
{
    public class StealthTextGrabber
    {
        private Point _startPoint;
        private Rectangle _selection;
        private bool _isSelecting;
        private OverlayForm _overlay;
        private TaskCompletionSource<bool> _selectionCompletition;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        public async Task<string> CaptureSelectedAreaAsync()
        {

            _selectionCompletition = new TaskCompletionSource<bool>();
            using (var hook = new LowLevelMouseHook())
            {
                hook.LeftButtonDown += (s, e) => StartSelection();
                hook.MouseMove +=  (s, e) => UpdateSelection();
                hook.LeftButtonUp +=  (s, e) => EndSelection();

                hook.Install();

                await _selectionCompletition.Task;

            }


            return RecognizeSelectionThrougTesseract();
        }

        private void StartSelection()
        {
            _overlay = new OverlayForm();
            _overlay.Show();

            _isSelecting = true;
            GetCursorPos(ref _startPoint);

        }

        private void UpdateSelection()
        {
            if (!_isSelecting) return;

            Point currentPoint = new Point();
            GetCursorPos(ref currentPoint);

            _selection = new Rectangle(
                Math.Min(_startPoint.X, currentPoint.X),
                Math.Min(_startPoint.Y, currentPoint.Y),
                Math.Abs(currentPoint.X - _startPoint.X),
                Math.Abs(currentPoint.Y - _startPoint.Y));

            _overlay.UpdateSelection(_selection);
        }
        // костыльный вариант с отжатием LMouseButton вроде это фиксит баг, но надо уже проверять с китайским языком
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private void EndSelection()
        {
            _isSelecting = false;

            if (_selectionCompletition != null && !_selectionCompletition.Task.IsCompleted)
            {
                _selectionCompletition.SetResult(true);
            }

            if (_overlay != null)
            {
                _overlay.Close();
                _overlay.Dispose();
            }

            // Программно "отжимаем" левую кнопку мыши
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        //
        /*
        private void EndSelection()
        {
            _isSelecting = false;

            if (_selectionCompletition != null && !_selectionCompletition.Task.IsCompleted)
            {
                _selectionCompletition.SetResult(true); // Сообщаем, что выделение завершено
            }

            if (_overlay != null)
            {
                _overlay.Close();
                _overlay.Dispose();
            }
            
        }
        */

        private string RecognizeSelectionThrougTesseract()
        {
            if (_selection.Width < 1 || _selection.Height < 1)
            {
                return "Invalid Selection";
            }

            using (var bitmap = new Bitmap(_selection.Width, _selection.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(_selection.Location, Point.Empty, _selection.Size);
                }

                using (var engine = new TesseractEngine("C:\\Users\\Sasha\\source\\repos\\ChineseTranslationThrougOCR\\ChineseTranslationTryCatchApp\\TesseractOCR\\tessdata\\", "chi_sim", EngineMode.LstmOnly))
                {
                    engine.SetVariable("tessedit_pageseg_mode", "6");
                    using (var pix = PixConverter.ToPix(bitmap))
                    {
                        using (var page = engine.Process(pix))
                        {
                            return page.GetText().Trim();
                        }
                    }
                }
            }
        }
    }

    public class LowLevelMouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelMouseProc _proc;

        public event EventHandler LeftButtonDown;
        public event EventHandler LeftButtonUp;
        public event EventHandler MouseMove;

        private LowLevelMouseProc _hookProc;// ща чекнум работает или нет, я не помню нужна ли эта хуйня в итоге или нет
        private GCHandle _gcHandle;

        public LowLevelMouseHook()
        {
            _hookProc = HookCallback;
            _gcHandle = GCHandle.Alloc(_hookProc);
        }

        public void Install()
        {
            if (_hookID == IntPtr.Zero)
            _hookID = SetHook(_hookProc);
        }

        public void Dispose()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (var curModule = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, _hookProc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                switch ((MouseMessage)wParam)
                {
                    case MouseMessage.WM_LBUTTONDOWN:
                        LeftButtonDown?.Invoke(this, EventArgs.Empty);
                        break;
                    case MouseMessage.WM_LBUTTONUP:
                        LeftButtonUp ?.Invoke(this, EventArgs.Empty);
                        break;
                    case MouseMessage.WM_MOUSEMOVE:
                        MouseMove?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        #region WinApi

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, 
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private enum MouseMessage
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }
}
// пропало окно выбора в иконке
