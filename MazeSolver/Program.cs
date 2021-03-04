using System;

namespace MazeSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MazeSolver maze = new MazeSolver("maze.csa-challenge.com", 80))
            {
                maze.Solve();
            }
        }
    }
}
