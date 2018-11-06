using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MatrixCode
{
    class DisplayScreen
    {
        public const int MaxDropsOnTopEdge = 7;
        public const int MaxDropsOnScreen = 50;

        private readonly int Width;
        public int Height { get; private set; }

        private string OldConsoleTitle;
        private ConsoleColor OldBG;
        private ConsoleColor OldFG;

        private SortedSet<int> FreeLanes;
        private SortedSet<CodeDrop> Drops;

        public Random RNG;
                
        public DisplayScreen()
        {
            // Display parameters
            Width = Console.WindowWidth;
            Height = Console.WindowHeight;
            // Allowed drop symbols
            CodeDrop.Symbols = Enumerable.Concat(Enumerable.Range(33, 94), Enumerable.Range(161, 95)).Select(i => (char)i).ToArray();
            // Initialize free lane set
            FreeLanes = new SortedSet<int>(Enumerable.Range(0, Width));
            Drops = new SortedSet<CodeDrop>();
            RNG = new Random();
            // Setup environment
            SetupEnvironment();
        }

        private void SetupEnvironment()
        {
            // Save old setttings
            OldConsoleTitle = Console.Title;
            OldBG = Console.BackgroundColor;
            OldFG = Console.ForegroundColor;
            // Customize console
            Console.Title = "MATRIX";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
        }

        private void TeardownEnvironment()
        {
            // Restore old settings
            Console.Title = OldConsoleTitle;
            Console.BackgroundColor = OldBG;
            Console.ForegroundColor = OldFG;
        }

        private CodeDrop AddDrop() {
            Debug.Assert(Drops.Count < MaxDropsOnScreen);
            Debug.Assert(Drops.Where(d => d.TouchesTopEdge).Count() < MaxDropsOnTopEdge);
            // Check if we can recycle an old drop
            CodeDrop drop = Drops.Where(d => !d.IsOnScreen).FirstOrDefault();
            if (drop != null)
            {
                ReturnLane(drop.XPos);
                drop.Reset(ReserveLane());
            }
            else
            {
                drop = new CodeDrop(this, Drops.Count, ReserveLane());
                Drops.Add(drop);
            }
            return drop;
        }

        private int ReserveLane()
        {
            Debug.Assert(FreeLanes.Count > 0, "There are no free lanes left!");
            int result = FreeLanes.ElementAt(RNG.Next(FreeLanes.Count));
            FreeLanes.Remove(result);
            return result;
        }

        private void ReturnLane(int ID)
        {
            FreeLanes.Add(ID);
        }

        public void Run()
        {
            //Console.WriteLine(new String(DropSymbols));
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            TeardownEnvironment();
        }
    }
}
