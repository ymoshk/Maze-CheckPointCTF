using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static MazeSolver.MazeSolver;

namespace MazeSolver
{
    public class HintsProcessor
    {
        private readonly string r_Path = @"C:\Users\97254\Desktop\CheckPoint\Maze\G.txt";
        private readonly string r_EqPath = @"C:\Users\97254\Desktop\CheckPoint\Maze\EQ.txt";
        private List<CellInfo> m_PositionsList;
        private List<Position> m_OptionalPositions;
        public event Action<Position> PositionFoundEvenHandler;
        public readonly int r_MazeWidth;
        public readonly int r_MazeHeight;

        public HintsProcessor(int i_MazeWidth, int i_MazeHeight, bool i_ReadFromFile = false)
        {
            this.m_PositionsList = new List<CellInfo>();
            this.m_OptionalPositions = new List<Position>();
            this.r_MazeHeight = i_MazeHeight;
            this.r_MazeWidth = i_MazeWidth;

            //if (i_ReadFromFile)
            //{
            //    ReadFromFile();
            //}        
        }

        public void AddHintMethod2(string i_HintAsSting, Position i_Position, bool i_Debug = false)
        {
            if(i_Debug)
            {
                Console.WriteLine("Adding Hint");
            }

            CellInfo hintPosition = new CellInfo(i_Position, i_HintAsSting);

            if(this.m_OptionalPositions.Count == 0)
            {
                CheckAllOptions(hintPosition);

                if (i_Debug)
                {
                    Console.WriteLine("Current possibles count is {0}", this.m_OptionalPositions.Count);
                }
            }
            else
            {
                RemoveImpossibles(hintPosition);

                if (i_Debug)
                {
                    Console.WriteLine("Current possibles count is {0}", this.m_OptionalPositions.Count);
                }
            }

            if(this.m_OptionalPositions.Count == 1)
            {
                if (i_Debug)
                {
                    Console.WriteLine("One Solution Left!");
                }

                OnSolutionFound(this.m_OptionalPositions[0]);
            }
            else if(this.m_OptionalPositions.Count == 0)
            {
                if (i_Debug)
                {
                    Console.WriteLine("Impossible to find solution");
                }
                
                Environment.Exit(-1);
            }
        }

        private void OnSolutionFound(Position hintPosition)
        {
            this.PositionFoundEvenHandler?.Invoke(hintPosition);
        }

        private void RemoveImpossibles(CellInfo hintPosition)
        {
            List<Position> toRemove = new List<Position>();

            foreach(Position pos in this.m_OptionalPositions)
            {
                double part1 = Math.Pow((hintPosition.Position.AxisLeftRight - pos.AxisLeftRight), 2);
                double part2 = Math.Pow((hintPosition.Position.AxisUPDown - pos.AxisUPDown), 2);
                double addition = part1 + part2;
                int sumAsInt = (int)Math.Floor(addition);

                if (sumAsInt != hintPosition.OrginalDistance)
                {
                    toRemove.Add(pos);
                }
            }

            foreach(Position pos in toRemove)
            {
                this.m_OptionalPositions.Remove(pos);
            }
        }

        private void CheckAllOptions(CellInfo hintPosition)
        {
            int distance = hintPosition.OrginalDistance;

            for (int i = 0; i < this.r_MazeHeight; i++) 
            {
                for (int j = 0; j < this.r_MazeWidth; j++)
                {
                    double part1 = Math.Pow((hintPosition.Position.AxisLeftRight - j), 2);
                    double part2 = Math.Pow((hintPosition.Position.AxisUPDown - i), 2);
                    double addition = part1 + part2;
                    int sumAsInt = (int)Math.Floor(addition);

                    if(sumAsInt == distance)
                    {
                        this.m_OptionalPositions.Add(new Position(j, i));
                    }
                }
            }
        }

        //public void AddHint(string i_HitAsString)
        //{
        //    CellInfo newCell = new CellInfo(i_HitAsString);
        //    this.m_PositionsList.Add(newCell);
        //    sortList();

        //    using(StreamWriter wr = new StreamWriter(
        //        File.Open(@"C:\Users\97254\Desktop\CheckPoint\Maze\ProcessedHints.txt",FileMode.Append,FileAccess.Read)))
        //    {
        //        wr.WriteLine(newCell.ToString());
        //    }
        //}

        //private void ReadFromFile()
        //{
        //    using (StreamReader reader = new StreamReader(File.OpenRead(this.r_Path)))
        //    {
        //        string fileAsString = reader.ReadToEnd();

        //        foreach (string line in fileAsString.Split('\n'))
        //        {
        //            try
        //            {
        //                this.m_PositionsList.Add(new CellInfo(line));
        //            }
        //            catch { }
        //        }
        //    }
        //    sortList();
        //}

        public void PrintInfo()
        {
            foreach(CellInfo cell in this.m_PositionsList)
            {
                Console.WriteLine(cell.ToString());
            }
        }

        public void SaveAsLinearEqMatrix()
        {
            StringBuilder builder = new StringBuilder();

            using (StreamWriter wr = new StreamWriter(this.r_EqPath))
            {
                foreach(CellInfo cell in this.m_PositionsList)
                {
                    builder.AppendLine(cell.ToEq());
                }

                wr.WriteLine(builder.ToString());
            }

        }

        private void sortList()
        {
            this.m_PositionsList.Sort((pos1, pos2) => comparePositions(pos1, pos2));
        }

        private int comparePositions(CellInfo pos1, CellInfo pos2)
        {
            double sub = pos1.SqrtDistance - pos2.SqrtDistance;

            return (int)Math.Floor(sub);
        }

        private class CellInfo
        {
            private readonly Position r_Position;
            private int m_OriginalDistance;
            private double m_SqrtDistance;

            public int OrginalDistance
            {
                get
                {
                    return this.m_OriginalDistance;
                }
            }

            public double SqrtDistance
            {
                get
                {
                    return this.m_SqrtDistance;
                }
            }

            public Position Position
            {
                get
                {
                    return this.r_Position;
                }
            }

            public CellInfo(Position i_Position, string i_SourceString)
            {
                this.r_Position = i_Position;
                this.m_OriginalDistance = parseDistanceFromString(i_SourceString);
                this.m_SqrtDistance = Math.Sqrt(this.m_OriginalDistance);
            }

            private int parseDistanceFromString(string i_SourceString)
            {
                string[] subStrings = i_SourceString.Split("???", 2);
                int result = 0;

                if(subStrings.Length > 1)
                {
                    string secondString = subStrings[1];
                    string intString = "";
                    foreach(char ch in secondString)
                    {
                        if(ch >= '0' && ch <= '9')
                        {
                            intString += ch;
                        }
                    }
                    result = int.Parse(intString);
                }

                return result;
            }

            public override string ToString()
            {
                return string.Format("Position: {0} \t Sqrt distance: {1} \t Normal Distance: {2}",
                    this.r_Position.ToString(), this.m_SqrtDistance, this.m_OriginalDistance);
            }

            public string ToEq()
            {
                return string.Format("{0}x + {1}y = {2}",
                    this.r_Position.AxisLeftRight, this.r_Position.AxisUPDown, this.m_SqrtDistance.ToString("0.##"));
            }
        }
    }
}
