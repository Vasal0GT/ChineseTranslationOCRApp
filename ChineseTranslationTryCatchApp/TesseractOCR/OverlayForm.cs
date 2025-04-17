using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChineseTranslationTryCatchApp
{
    public class OverlayForm : Form
    {
        private Rectangle _selection;

        public OverlayForm()
        {
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.Enabled = false;            
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
        }

        public void UpdateSelection(Rectangle selection)
        {
            _selection = selection;
            this.Invalidate();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.BlueViolet, 2))
            {
                e.Graphics.DrawRectangle(pen, _selection);
            }
        }
    }
}
