using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MazeSolver
{
    public class MazeSolver: IDisposable
    {
        private readonly MazeSockets r_Socket;
        private Position m_CurrentPosition;
        private MazeCell[,] m_MazeMatrix;
        private bool m_KeepWalking = true;
        private const int k_MazeHeight = 250;
        private const int k_MazeWidth = 250;
        private readonly HintsProcessor r_HitsProcessor;
        private Position m_FinalPos;

        public MazeSolver(string i_IpAddress, int i_Port)
        {
            this.r_HitsProcessor = new HintsProcessor(k_MazeWidth, k_MazeHeight);
            this.r_HitsProcessor.PositionFoundEvenHandler += R_HitsProcessor_PositionFoundEvenHandler;
            this.r_Socket = new MazeSockets(i_IpAddress, i_Port);
            this.m_CurrentPosition = new Position(this.r_Socket.StartingString);
            InitMatrix();
        }

        private void R_HitsProcessor_PositionFoundEvenHandler(Position i_Solution)
        {
            this.m_FinalPos = i_Solution;
            this.m_KeepWalking = false;
        }

        private void handleSolution()
        {
            this.r_Socket.GetCurrentPosition();

            Console.WriteLine("Breaking the process to enter a solution --> Enter yours now");
            this.r_Socket.EnterSolution(this.m_FinalPos.ToString());
        }

        private void recursiveStep(MazeCell i_Source)
        {
            if(!this.m_KeepWalking)
            {
                Console.WriteLine("ret");
                return;
            }

            //this.m_CurrentPosition = this.r_Socket.GetCurrentPosition();
            Console.WriteLine(this.m_CurrentPosition.ToString());
            MazeCell currentCell = getCellFromPosition();
            currentCell.State = eCellState.Visited;
            MazeSockets.AvailableSteps steps = this.r_Socket.GetPossibleSteps();
            markWalls(steps);
            fillCurrentMessages(currentCell);
            Position tempCur = this.m_CurrentPosition;          
            
            if(steps.Up && this.m_KeepWalking)
            {
                if(StepUp())
                {
                    recursiveStep(currentCell);
                }
            }

            if (steps.Down && this.m_KeepWalking)
            {
                if (StepDown())
                {
                    recursiveStep(currentCell);
                }
            }

            if (steps.Left && this.m_KeepWalking)
            {
                if (StepLeft())
                {
                    recursiveStep(currentCell);
                }
            }

            if (steps.Right && this.m_KeepWalking)
            {
                if (StepRight())
                {
                    recursiveStep(currentCell);
                }
            }

            if (i_Source != null && this.m_KeepWalking)
            {
                returnStep(i_Source);
            }
        }

        private void returnStep(MazeCell i_Source)
        {
            Position sourcePos = i_Source.Position;
            Position current = this.m_CurrentPosition;

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
                handleSolution();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //printMaze();
            printCellsInfo();
        }

        private void markWalls(MazeSockets.AvailableSteps i_Options)
        {
            Position temp;
            Position current = this.m_CurrentPosition;


            if(!i_Options.Up)
            {
                temp = current;
                temp.StepUp();
                getCellFromPosition().State = eCellState.Wall;
            }

            if (!i_Options.Down)
            {
                temp = current;
                temp.StepDown();
                getCellFromPosition().State = eCellState.Wall;
            }

            if (!i_Options.Left)
            {
                temp = current;
                temp.StepLeft();
                getCellFromPosition().State = eCellState.Wall;
            }

            if (!i_Options.Right)
            {
                temp = current;
                temp.StepRight();
                getCellFromPosition().State = eCellState.Wall;
            }
        }

        private void fillCurrentMessages(MazeCell i_CurrentCell)
        {
            string msgG;
            string msgH;
            MazeCell currentCell = i_CurrentCell;

            if (this.r_Socket.ReceiveG_Value(out msgG))
            {
                currentCell.InfoG = msgG;
                Position pos = this.r_Socket.GetCurrentPosition();
                this.r_HitsProcessor.AddHintMethod2(msgG, pos, true);
            }
            else
            {
                currentCell.InfoG = "";
            }

            if (false) //this.r_Socket.ReceiveH_Value(out msgH))
            {
                currentCell.InfoH = msgH;
            }
            else
            {
                currentCell.InfoH = "";
            }
        }

        private MazeCell getCellFromPosition()
        {
            return this.m_MazeMatrix[this.m_CurrentPosition.AxisUPDown, this.m_CurrentPosition.AxisLeftRight];
        }

        private void InitMatrix()
        {
            this.m_MazeMatrix = new MazeCell[k_MazeHeight, k_MazeWidth];

            for (int i = 0; i < k_MazeHeight; i++)
            {
                for (int j = 0; j < k_MazeWidth; j++)
                {
                    this.m_MazeMatrix[i, j] = new MazeCell(new Position(j, i));
                }
            }
        }

        private bool StepUp(bool isReturn = false)
        {
            bool result = false;
            Position current = this.m_CurrentPosition;
            Position newPos = new Position(current);
            newPos.StepUp();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Up))
                {
                    this.m_CurrentPosition = newPos;
                    result = true;
                }
            }

            return result;
        }

        private bool StepDown(bool isReturn = false)
        {
            bool result = false;
            Position current = this.m_CurrentPosition;
            Position newPos = new Position(current);
            newPos.StepDown();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Down))
                {
                    this.m_CurrentPosition = newPos;
                    result = true;
                }
            }

            return result;
        }

        private bool StepRight(bool isReturn = false)
        {
            bool result = false;
            Position current = this.m_CurrentPosition;
            Position newPos = new Position(current);
            newPos.StepRight();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Right))
                {
                    this.m_CurrentPosition = newPos;
                    result = true;
                }
            }

            return result;
        }

        private bool StepLeft(bool isReturn = false)
        {
            bool result = false;
            Position current = this.m_CurrentPosition;
            Position newPos = new Position(current);
            newPos.StepLeft();

            if (isPositionLegal(newPos) || isReturn)
            {
                if (this.r_Socket.Move(MazeSockets.eMovementDirection.Left))
                {
                    this.m_CurrentPosition = newPos;
                    result = true;
                }
            }

            return result;
        }

        private bool isPositionLegal(Position i_PositionToCheck)
        {
            int UpDown = i_PositionToCheck.AxisUPDown;
            int LeftRight = i_PositionToCheck.AxisLeftRight;
            bool AxisXisLegal = UpDown >= 0 && UpDown < k_MazeHeight;
            bool AxisYisLegal = LeftRight >= 0 && LeftRight < k_MazeWidth;

            return AxisXisLegal && AxisYisLegal &&
                (this.m_MazeMatrix[UpDown, LeftRight].State != eCellState.Wall && this.m_MazeMatrix[UpDown, LeftRight].State != eCellState.Trap && 
                this.m_MazeMatrix[UpDown, LeftRight].State != eCellState.Visited);
        }
        
        private void printCellsInfo()
        {
            StringBuilder st = new StringBuilder();

            Map(current =>
            {
                if (current.State != eCellState.Init)
                {
                    st.AppendLine(string.Format("Cell ({0},{1}), Cell State: {4}, G Button Info: {2}, H Button Info: {3}",
                        current.Position.AxisLeftRight, current.Position.AxisUPDown, current.InfoG, current.InfoH, current.State.ToString()));
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

                if(current.Position.AxisUPDown == k_MazeWidth - 1)
                {
                    Console.WriteLine();
                }
            });
        }

        private void Map(Action<MazeCell> i_MapFunction)
        {
            MazeCell current;

            for (int i = 0; i < k_MazeHeight; i++)
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
            int UpDown;
            int LeftRight;

            public Position(int i_LeftRight, int i_UpDown)
            {
                this.UpDown = i_UpDown;
                this.LeftRight = i_LeftRight;
            }

            public Position(Position i_PositionToCopy)
            {
                this.UpDown = i_PositionToCopy.AxisUPDown;
                this.LeftRight = i_PositionToCopy.AxisLeftRight;
            }

            public Position(string i_Source) : this(FetchPositionFromString(i_Source)) { }

            public int AxisUPDown { get { return this.UpDown; } }
            public int AxisLeftRight { get { return this.LeftRight; } }

            public void StepUp()
            {
                this.UpDown += 1;
            }

            public void StepDown()
            {
                this.UpDown -= 1;
            }

            public void StepLeft()
            {
                this.LeftRight -= 1;
            }

            public void StepRight()
            {
                this.LeftRight += 1;
            }

            public override string ToString()
            {
                return string.Format("({0},{1})", this.AxisLeftRight, this.AxisUPDown);
            }

            private static Position FetchPositionFromString(string i_StringPos)
            {
                (int, int) tempRes = fetchIntsFromTuple(findTupleInAString(i_StringPos));
                return new Position(tempRes.Item1, tempRes.Item2);
            }

            private static (int, int) fetchIntsFromTuple(string i_TupleString)
            {
                (int, int) result = default;

                Regex rgx = new Regex(@"\d+");
                Match match = rgx.Match(i_TupleString);

                if (match.Success)
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

            private static string findTupleInAString(string i_StringToParse)
            {
                Regex rgx = new Regex(@"\(\d+,\d+\)");
                Match match = rgx.Match(i_StringToParse);
                string res = string.Empty;

                if (match.Success)
                {
                    res = match.Value;
                }
                else
                {
                    throw new Exception("Couldn't parse position from the given string.");
                }

                return res;
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
                return string.Format("({0},{1})", this.Position.AxisLeftRight, this.Position.AxisUPDown);
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
