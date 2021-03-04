using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeUX
{
    public partial class Maze : Form
    {
        private const int k_MazeHeight = 250;
        private const int k_MazeWidth = 250;
        private const int k_CellSize = 4;
        private Cell[,] m_MazeMatrix;

        public Maze()
        {
            InitializeComponent();
            this.m_MazeMatrix = new Cell[k_MazeHeight, k_MazeWidth];
            createMaze();
        }  

        private void createMaze()
        {
            int currentWidth = 0;
            int currentHeight = 0;

            for (int i = 0; i < k_MazeHeight; i++)
            {
                for (int j = 0; j < k_MazeWidth; j++)
                {
                    Cell newCell = new Cell();
                    this.m_MazeMatrix[i, j] = newCell;
                    this.Controls.Add(newCell);
                    newCell.SetBounds(currentWidth, currentHeight, k_CellSize, k_CellSize);
                    currentWidth += 4;
                }
                currentHeight += 4;
                currentWidth = 0;
            }
        }
    }
}
