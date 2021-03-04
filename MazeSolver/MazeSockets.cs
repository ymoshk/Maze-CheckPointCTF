using System;
using System.IO;
using System.Text.RegularExpressions;
using static MazeSolver.MazeSolver;

namespace MazeSolver
{
    public class MazeSockets : IDisposable
    {
        private readonly SocketConnetion r_Connection;
        private readonly string r_StatingString;

        public MazeSockets(string i_IpAddress, int i_Port)
        {
            this.r_Connection = new SocketConnetion(i_IpAddress, i_Port);

            if(this.r_Connection.Connect())
            {
                this.r_StatingString = fetchStartingString();
            }
            else
            {
                throw new Exception("Connection to socket machine has failed.");
            }
        }

        public string StartingString { get { return this.r_StatingString; } }

        public void Dispose()
        {
            this.r_Connection.Close();
        }

        private string fetchStartingString()
        {
            return this.r_Connection.ReceiveUntil("command?");
        }

        public AvailableSteps GetPossibleSteps()
        {
            this.r_Connection.Send('i');
            string received = this.r_Connection.ReceiveUntil("command?");
            received = received.Replace("What is your command?", "");

            if(string.IsNullOrEmpty(received))
            {
                throw new Exception("Couldn't fetch the current string");
            }
            else if(!received.Contains("l="))
            {
                throw new Exception("Couldn't fetch the current string");
            }
            else
            {
                return new AvailableSteps(received);
            }
        }

        public bool ReceiveG_Value(out string o_Value)
        {
            o_Value = "";
            bool result = true;
            this.r_Connection.Send('g');
            string received = this.r_Connection.ReceiveUntil("command?");
            received = received.Replace("What is your command?", "");

            if (string.IsNullOrEmpty(received))
            {
                throw new Exception("Couldn't fetch the current string");
            }
            else
            {
                if (received.Contains("far far away"))
                {
                    result = false;
                }
                else
                {
                    using (StreamWriter st = new StreamWriter(File.Open(@"C:\Users\97254\Desktop\CheckPoint\Maze\G.txt", FileMode.Append)))
                    {
                        Position pos = GetCurrentPosition();
                        st.WriteLine("Cell: ({0},{1}) Msg: {2}", pos.AxisLeftRight, pos.AxisUPDown, received);
                        Console.WriteLine("Cell: ({0},{1}) Msg: {2}", pos.AxisLeftRight, pos.AxisUPDown, received);
                    }                   
                }

                o_Value = received;
            }

            return result;
        }

        public bool ReceiveH_Value(out string o_Value)
        {
            o_Value = "";
            bool result = true;
            this.r_Connection.Send('h');
            string received = this.r_Connection.ReceiveUntil("command?");
            received = received.Replace("What is your command?", "");

            if (string.IsNullOrEmpty(received))
            {
                throw new Exception("Couldn't fetch the current string");
            }
            else
            {
                if (received.Contains("just use your brain") || received.Contains("I wish I could help") ||
                    received.Contains("Really???") || received.Contains("try harder") || received.Contains("breakfast"))
                {
                    result = false;
                }
                else
                {
                    using(StreamWriter st = new StreamWriter(File.Open(@"C:\Users\97254\Desktop\CheckPoint\Maze\G.txt", FileMode.Append)))
                    {
                        Position pos = GetCurrentPosition();
                        st.WriteLine("Cell: ({0},{1}) Msg: {2}", pos.AxisLeftRight, pos.AxisUPDown, received);
                        Console.WriteLine("Cell: ({0},{1}) Msg: {2}", pos.AxisLeftRight, pos.AxisUPDown, received);
                    }
                }

                o_Value = received;
            }

            return result;
        }

        public bool IsMovementSucceed()
        {
            bool result = false;
            string received = this.r_Connection.ReceiveUntil("command?");
            received = received.Replace("What is your command?", "");

            if (string.IsNullOrEmpty(received))
            {
                throw new Exception("Couldn't fetch the current string");
            }
            else
            {
                if (received.Contains("1"))
                {
                    result = true;
                }
            }
         
            return result;
        }

        public bool Move(eMovementDirection i_Dir)
        {
            switch(i_Dir)
            {
                case eMovementDirection.Up:
                    this.r_Connection.Send('u');
                    break;

                case eMovementDirection.Down:
                    this.r_Connection.Send('d');
                    break;

                case eMovementDirection.Left:
                    this.r_Connection.Send('l');
                    break;

                case eMovementDirection.Right:
                    this.r_Connection.Send('r');
                    break;
            }

            return IsMovementSucceed();
        }

        public MazeSolver.Position GetCurrentPosition()
        {
            int intLeftRight;
            int intUpDown;
            this.r_Connection.Send('c');
            string received = this.r_Connection.ReceiveUntil("command?");
            received = received.Replace("What is your command?", "");

            if (string.IsNullOrEmpty(received))
            {
                throw new Exception("Couldn't fetch the current string");
            }
            else
            {
                Regex rgx = new Regex(@"\d+");
                Match match = rgx.Match(received);

                if (match.Success)
                {
                    intLeftRight = int.Parse(match.Value);
                }
                else
                {
                    throw new Exception("Couldn't parse current position");
                }

                match = match.NextMatch();

                if (match.Success)
                {
                    intUpDown = int.Parse(match.Value);
                }
                else
                {
                    throw new Exception("Couldn't parse current position");
                }

                Console.WriteLine("({0},{1})", intLeftRight, intUpDown);
                return new MazeSolver.Position(intLeftRight, intUpDown);
            }

        }

        public void EnterSolution(string i_Solution)
        {
            bool result = true;
            this.r_Connection.Send('s');
            Console.WriteLine('s');
            string received = this.r_Connection.ReceiveUntil("solution?");
            Console.WriteLine(received);

            this.r_Connection.Send(i_Solution);
            received = this.r_Connection.ReceiveUntil("!", "}");
            Console.WriteLine(received);
        }

        public class AvailableSteps
        {
            public bool Up { get; }
            public bool Down { get; }
            public bool Left { get; }
            public bool Right { get; }

            public AvailableSteps(string i_InformationString)
            {
                this.Up = this.Down = this.Left = this.Right = false;

                if(i_InformationString.Contains("r=1"))
                {
                    this.Right = true;
                }

                if (i_InformationString.Contains("l=1"))
                {
                    this.Left = true;
                }

                if (i_InformationString.Contains("u=1"))
                {
                    this.Up = true;
                }

                if (i_InformationString.Contains("d=1"))
                {
                    this.Down = true;
                }
            }
        }

        public enum eMovementDirection
        {
            Up,
            Down,
            Left,
            Right,
        }
    }
}
