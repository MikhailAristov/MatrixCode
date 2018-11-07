using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Timers;

namespace MatrixCode
{
    class DisplayScreen
    {
        public const int MaxDropsOnTopEdge = 7;
        public const int MaxDropsOnScreen = 50;

        public readonly int Width;
        public readonly int Height;

        private const int MarginLeft = 1;
        private const int MarginRight = 2;
        private const int MarginTop = 1;
        private const int MarginBottom = 2;

        public const ConsoleColor TextColor = ConsoleColor.Green;
        public const ConsoleColor GlowingTextColor = ConsoleColor.White;
        public const ConsoleColor BackgroundColor = ConsoleColor.Black;

        private string OldConsoleTitle;
        private ConsoleColor OldBG;
        private ConsoleColor OldFG;

        private SortedSet<int> FreeLanes;
        private SortedSet<CodeDrop> Drops;

        public Random RNG;
                
        public DisplayScreen()
        {
            // Display parameters
            Width = Console.WindowWidth - MarginLeft - MarginRight;
            Height = Console.WindowHeight - MarginTop - MarginBottom;
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
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = TextColor;
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
            Timer myTimer = new Timer((CodeDrop.MinTimerInterval + CodeDrop.MaxTimerInterval) / 2);
            myTimer.Elapsed += new ElapsedEventHandler(Update);
            myTimer.AutoReset = true;
            myTimer.Enabled = true;
            // Wait for escape command
            while (!Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Escape);
            // Stop the timer and tear down the environment
            myTimer.Enabled = false;
            TeardownEnvironment();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Update(Object source, ElapsedEventArgs e) {
            // Check if there are enough drops touching the top edge, if not, add some more
            int dropBudget = Math.Min(MaxDropsOnScreen - Drops.Count, MaxDropsOnTopEdge - Drops.Where(d => d.TouchesTopEdge).Count());
            for (int i = 0; i < dropBudget; i++)
            {
                AddDrop();
            }
        }

        public void WriteChar(int XPos, int YPos, char Payload, ConsoleColor ForegroundColor)
        {
            // Cast inputs
            short x = (short)(XPos + MarginLeft);
            short y = (short)(YPos + MarginTop);
            byte ch = (byte)Payload;
            short att = (short)ForegroundColor;
            // Call API
            ConsoleAPI.WriteChar(x, y, ch, att);
        }
    }
}
