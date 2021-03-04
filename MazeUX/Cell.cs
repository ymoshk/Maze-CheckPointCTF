using System;
using System.Drawing;
using System.Windows.Forms;

namespace MazeUX
{
    public class Cell : Button
    {
        public Cell()
        {
            this.BackColor = Color.Gray;
            this.Width = 4;
            this.Height = 4;
        }

        public void MarkWall()
        {
            this.BackColor = Color.Black;
        }

        public void MarkVisited()
        {
            this.BackColor = Color.Green;
        }

        public void MarkCurrent()
        {
            this.BackColor = Color.Red;
        }

        public void MarkUnique()
        {
            this.BackColor = Color.Blue;
        }
    }
}
