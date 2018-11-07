using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;

namespace MatrixCode
{
    class DisplayScreen
    {
        public const int MaxDropsOnTopEdge = 7;
        public const int MaxDropsOnScreen = 50;

        public const int UpdateInterval = 100; // in milliseconds

        public readonly int Width;
        public readonly int Height;

        private const int XMargin = 2;
        private const int YMargin = 2;

        private string OldConsoleTitle;
        private ConsoleColor OldBG;
        private ConsoleColor OldFG;

        private SortedSet<int> FreeLanes;
        private SortedSet<CodeDrop> Drops;

        public Random RNG;

        private bool UpdateRunning;
                
        public DisplayScreen()
        {
            // Display parameters
            Width = Console.WindowWidth - 2 * XMargin - 1;
            Height = Console.WindowHeight - 2 * YMargin;
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
            Console.Title = "What is the Matrix?";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorVisible = false;
        }

        private void TeardownEnvironment()
        {
            // Restore old settings
            Console.Title = OldConsoleTitle;
            Console.BackgroundColor = OldBG;
            Console.ForegroundColor = OldFG;
            Console.CursorVisible = true;
        }

        private CodeDrop AddDrop() {
            // Check if we can recycle an old drop
            CodeDrop drop = Drops.Where(d => d.HasLeftTheScreen).FirstOrDefault();
            if (drop != null)
            {
                ReturnLane(drop.XPos);
                drop.Reset(ReserveLane());
            }
            else
            {
                drop = new CodeDrop(this, Drops.Count, ReserveLane());
                Drops.Add(drop);
                //WriteChar(Drops.Count, 0, '*');
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
            // Run the timed events
            Timer aTimer = new Timer(UpdateInterval);
            aTimer.Elapsed += new ElapsedEventHandler(Update);
            //aTimer.AutoReset = true;
            aTimer.Enabled = true;
            // Wait for escape command
            while (!Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Escape);
            // Stop the timer and tear down the environment
            aTimer.Enabled = false;
            TeardownEnvironment();
        }

        public void Update(Object source, ElapsedEventArgs e) {
            if (!UpdateRunning)
            {
                UpdateRunning = true;
                // Update each drop
                foreach (CodeDrop d in Drops)
                {
                    d.Update();
                    d.Display();
                }
                // Check if there are enough drops touching the top edge, if not, add some more
                int dropBudget = Math.Min(MaxDropsOnScreen - Drops.Count, MaxDropsOnTopEdge - Drops.Where(d => d.TouchesTopEdge).Count());
                for (int i = 0; i < dropBudget; i++)
                {
                    AddDrop();
                }
                Console.SetCursorPosition(0, 0);
                UpdateRunning = false;
            }

        }

        public void WriteChar(int XPos, int YPos, char Payload)
        {
            Console.SetCursorPosition(XPos + XMargin, YPos + YMargin);
            Console.Write(Payload);
        }
    }
}
