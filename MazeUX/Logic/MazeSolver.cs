using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MazeSolver
{
    public class MazeSolver: IDisposable
    {
        private readonly MazeSockets r_Socket;
        private readonly Position r_StartingPosition;
        //private Position m_CurrentPosition;
        private MazeCell[,] m_MazeMatrix;
        private const int k_MazeHeigh = 250;
        private const int k_MazeWidth = 250;
        private int m_NumberOfCalls = 0;
        private int NumberOfCalls
        {
            get
            {
                return this.m_NumberOfCalls;
            }
            set
            {
                this.m_NumberOfCalls = value;
                Console.WriteLine("Number of calls: " + this.m_NumberOfCalls);
            }
        }

        private MazeCell currentMazeCell
        {
            get
            {
                Position current = this.r_Socket.GetCurrentPosition();
                return this.m_MazeMatrix[current.AxisY, current.AxisX];
            }
        }

        public MazeSolver(string i_IpAddress, int i_Port)
        {
            this.r_Socket = new MazeSockets(i_IpAddress, i_Port);
            this.r_StartingPosition = fetchStartingPos(this.r_Socket.StartingString);
            //this.m_CurrentPosition = new Position(this.r_StartingPosition);
            InitMatrix();
        }

        private void recursiveStep(MazeCell i_Source)
        {
            this.NumberOfCalls++;

            Position currentPos = this.r_Socket.GetCurrentPosition();
            MazeCell currentCell = getCellFromPosition(currentPos);
            currentCell.State = eCellState.Visited;
            MazeSockets.AvailableSteps steps = this.r_Socket.GetPossibleSteps();
            markWalls(steps);
            fillCurrentMessages();
            Position tempCur = currentPos;
            
            if(steps.Up)
            {
                if(StepUp())
                {
                    recursiveStep(currentCell);
                    steps = this.r_Socket.GetPossibleSteps();
                    markWalls(steps);
                }
            }

            if (steps.Down)
            {
                if (StepDown())
                {
                    recursiveStep(currentCell);
                    steps = this.r_Socket.GetPossibleSteps();
                    markWalls(steps);
                }
            }

            if (steps.Left)
            {
                if (StepLeft())
                {
                    recursiveStep(currentCell);
                    steps = this.r_Socket.GetPossibleSteps();
                    markWalls(steps);
                }
            }

            if (steps.Right)
            {
                if (StepRight())
                {
                    recursiveStep(currentCell);
                    steps = this.r_Socket.GetPossibleSteps();
                    markWalls(steps);
                }
            }

            if (i_Source != null)
            {
                returnStep(i_Source);
            }

            this.NumberOfCalls--;
        }

        private void returnStep(MazeCell i_Source)
        {
            Position sourcePos = i_Source.Position;
            Position current = this.r_Socket.GetCurrentPosition();

            Position currentCopy = current;
            currentCopy.StepUp();
            if (currentCopy.Equals(sourcePos))
            {
                if(!StepUp(true))
                {
                    throw new Exception("Returning up failed");
                }
                return;
            }

            currentCopy = current;
            currentCopy.StepDown();
            if (currentCopy.Equals(sourcePos))
            {
                if (!StepDown(true))
                {
                    throw new Exception("Returning down failed");
                }
                return;
            }

            currentCopy = current;
            currentCopy.StepRight();
            if (currentCopy.Equals(sourcePos))
            {
                if (!StepRight(true))
                {
                    throw new Exception("Returning right failed");
                }
                return;
            }

            currentCopy = current;
            currentCopy.StepLeft();
            if (currentCopy.Equals(sourcePos))
            {
                if (!StepLeft(true))
                {
                    throw new Exception("Returning left failed");
                }
                return;
            }
        }

        public void Solve()
        {
            try
            {
                recursiveStep(null);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //printMaze();
            printCellsInfo();
        }

        private void markWalls(MazeSockets.AvailableSteps i_Options)
        {
            Position temp;
            Position current = this.r_Socket.GetCurrentPosition();


            if(!i_Options.Up)
            {
                temp = current;
                temp.StepUp();
                getCellFromPosition(temp).State = eCellState.Wall;
            }

            if (!i_Options.Down)
            {
                temp = current;
                temp.StepDown();
                getCellFromPosition(temp).State = eCellState.Wall;
            }

            if (!i_Options.Left)
            {
                temp = current;
                temp.StepLeft();
                getCellFromPosition(temp).State = eCellState.Wall;
            }

            if (!i_Options.Right)
            {
                temp = current;
                temp.StepRight();
                getCellFromPosition(temp).State = eCellState.Wall;
            }
        }

        private void fillCurrentMessages()
        {
            string msgG;
            string msgH;
            MazeCell currentCell = this.currentMazeCell;

            if (this.r_Socket.ReceiveG_Value(out msgG))
            {
                currentCell.InfoG = msgG;
            }
            else
            {
                currentCell.InfoG = "";
            }

            if (this.r_Socket.ReceiveH_Value(out msgH))
            {
                currentCell.InfoH = msgH;
            }
            else
            {
                currentCell.InfoH = "";
            }
        }

        private MazeCell getCellFromPosition(Position i_Position)
        {
            return this.m_MazeMatrix[i_Position.AxisY, i_Position.AxisX];
        }

        private void InitMatrix()
        {
            this.m_MazeMatrix = new MazeCell[k_MazeHeigh, k_MazeWidth];

            for (int i = 0; i < k_MazeHeigh; i++)
            {
                for (int j = 0; j < k_MazeWidth; j++)
                {
                    this.m_MazeMatrix[i, j] = new MazeCell(new Position(i, j));
                }
            }
        }

        private bool StepUp(bool isReturn = false)
        {
            bool result = false;
            Position current = this.r_Socket.GetCurrentPosition();
            Position newPos = new Position(current);
            newPos.StepUp();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Up))
                {
                    result = true;
                }
            }

            return result;
        }

        private bool StepDown(bool isReturn = false)
        {
            bool result = false;
            Position current = this.r_Socket.GetCurrentPosition();
            Position newPos = new Position(current);
            newPos.StepDown();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Down))
                {
                    result = true;
                }
            }

            return result;
        }

        private bool StepRight(bool isReturn = false)
        {
            bool result = false;
            Position current = this.r_Socket.GetCurrentPosition();
            Position newPos = new Position(current);
            newPos.StepRight();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Right))
                {
                    result = true;
                }
            }

            return result;
        }

        private bool StepLeft(bool isReturn = false)
        {
            bool result = false;
            Position current = this.r_Socket.GetCurrentPosition();
            Position newPos = new Position(current);
            newPos.StepLeft();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Left))
                {
                    result = true;
                }
            }

            return result;
        }

        private Position fetchStartingPos(string i_StringPos)
        {
            (int, int) tempRes = fetchIntsFromTuple(findTupleInAString(i_StringPos));
            return new Position(tempRes.Item1, tempRes.Item2); 
        }

        private (int,int) fetchIntsFromTuple(string i_TupleString)
        {
            (int, int) result = default;

            Regex rgx = new Regex(@"\d+");
            Match match = rgx.Match(i_TupleString);

            if(match.Success)
            {
                result.Item1 = int.Parse(match.Value);
            }

            match = match.NextMatch();

            if (match.Success)
            {
                result.Item2 = int.Parse(match.Value);
            }

            return result;
        }

        private string findTupleInAString(string i_StringToParse)
        {
            Regex rgx = new Regex(@"\(\d+,\d+\)");
            Match match = rgx.Match(i_StringToParse);
            string res = string.Empty;

            if (match.Success)
            {
                res = match.Value;
            }

            return res;
        }

        private bool isPositionLegal(Position i_PositionToCheck)
        {
            int x = i_PositionToCheck.AxisX;
            int y = i_PositionToCheck.AxisY;
            bool AxisXisLegal = x >= 0 && x < k_MazeHeigh;
            bool AxisYisLegal = y >= 0 && y < k_MazeWidth;

            return AxisXisLegal && AxisYisLegal &&
                (this.m_MazeMatrix[y, x].State != eCellState.Wall && this.m_MazeMatrix[y, x].State != eCellState.Trap && 
                this.m_MazeMatrix[y, x].State != eCellState.Visited);
        }
        
        private void printCellsInfo()
        {
            StringBuilder st = new StringBuilder();

            Map(current =>
            {
                if (current.State != eCellState.Init)
                {
                    st.AppendLine(string.Format("Cell ({0},{1}), Cell State: {4}, G Button Info: {2}, H Button Info: {3}",
                        current.Position.AxisY, current.Position.AxisX, current.InfoG, current.InfoH, current.State.ToString()));
                }
            });

            using(StreamWriter wr = new StreamWriter(File.OpenWrite(@"C:\Users\97254\Desktop\CheckPoint\Maze\out.txt")))
            {
                wr.WriteLine(st.ToString());
            }
        }

        private void printMaze()
        {
            Map(current =>
            {
                switch(current.State)
                {
                    case eCellState.Tip:
                        Console.Write("! ");
                        break;

                    case eCellState.Init:
                        Console.Write("  ");
                        break;

                    case eCellState.Trap:
                        Console.Write("X ");
                        break;

                    case eCellState.Visited:
                        Console.Write("V ");
                        break;

                    case eCellState.Wall:
                        Console.Write("* ");
                        break;
                }

                if(current.Position.AxisX == k_MazeWidth - 1)
                {
                    Console.WriteLine();
                }
            });
        }

        private void Map(Action<MazeCell> i_MapFunction)
        {
            MazeCell current;

            for (int i = 0; i < k_MazeHeigh; i++)
            {
                for (int j = 0; j < k_MazeWidth; j++)
                {
                    current = this.m_MazeMatrix[i, j];

                    i_MapFunction.Invoke(current);
                }
            }
        }

        public void Dispose()
        {
            this.r_Socket.Dispose();
        }

        public struct Position
        {
            int x;
            int y;

            public Position(int i_Y, int i_X)
            {
                this.x = i_X;
                this.y = i_Y;
            }

            public Position(Position i_PositionToCopy)
            {
                this.x = i_PositionToCopy.AxisX;
                this.y = i_PositionToCopy.AxisY;
            }

            public int AxisX { get { return this.x; } }
            public int AxisY { get { return this.y; } }

            public void StepUp()
            {
                this.x += 1;
            }

            public void StepDown()
            {
                this.x -= 1;
            }

            public void StepLeft()
            {
                this.y -= 1;
            }

            public void StepRight()
            {
                this.y += 1;
            }
        }

        private class MazeCell
        {
            private eCellState m_CellState;
            private Position m_Position;
            private string m_MessageForG;
            private string m_MessageForH;
            public string InfoG
            {
                get
                {
                    return this.m_MessageForG;
                }
                set
                {
                    this.m_MessageForG = value;
                }
            }
            public string InfoH
            {
                get
                {
                    return this.m_MessageForH;
                }
                set
                {
                    this.m_MessageForH = value;
                }
            }
            public Position Position { get { return this.m_Position; } }

            public MazeCell(Position i_Position)
            {
                this.m_Position = i_Position;
                this.m_CellState = eCellState.Init;
                this.m_MessageForG = "";
                this.m_MessageForH = "";
            }

            public eCellState State
            {
                get
                {
                    return this.m_CellState;
                }
                set
                {
                    this.m_CellState = value;
                }
            }

            public override string ToString()
            {
                return string.Format("({0},{1})", this.Position.AxisY, this.Position.AxisX);
            }
        }

        private enum eCellState
        {
            Init,
            Visited,
            Wall,
            Trap,
            Tip,
        }
    }
}
