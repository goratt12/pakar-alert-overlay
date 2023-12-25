using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pakar_alert_overlay
{
    internal class AlertBanner : PictureBox
    {
        public string text;

        public AlertBanner(string text)
        {
            this.text = text;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); // Call the base class's OnPaint method

            // Get the Graphics object for the PictureBox
            Graphics g = e.Graphics;

            // Create a font and brush for the text
            Font textFont = new Font("Arial", 15, FontStyle.Bold);
            Brush textBrush = new SolidBrush(Color.White);

            // Calculate the position to center the text
            float x = 400 - g.MeasureString(this.text, textFont).Width - 10; // Adjust for padding
            float y = (40 - g.MeasureString(this.text, textFont).Height) / 2;

            // Draw the text on the PictureBox using the value of displayText
            g.DrawString(this.text, textFont, textBrush, x, y);
        }
    }
}
