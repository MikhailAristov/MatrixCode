using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace MatrixCode
{
    class DisplayScreen
    {
        public const int UpdateTimerInterval = 20; // in milliseconds
        private const int RepopulateInterval = 5 * UpdateTimerInterval;

        private readonly int MaxDropsOnTopEdge;
        private readonly int MaxDropsOnScreen;

        private readonly int Width;
        public readonly int Height;

        private const int MarginLeft = 0;
        private const int MarginRight = 0;
        private const int MarginTop = 0;
        private const int MarginBottom = 0;

        public const ConsoleColor TextColor = ConsoleColor.Green;
        public const ConsoleColor GlowingTextColor = ConsoleColor.White;
        public const ConsoleColor BackgroundColor = ConsoleColor.Black;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;

        private string OldConsoleTitle;
        private ConsoleColor OldBG;
        private ConsoleColor OldFG;

        private readonly SortedSet<int> FreeLanes;
        private readonly SortedSet<CodeDrop> Drops;

        public Random RNG;
        public Timer UpdateTimer;
        private DateTime NextRepopulationTime;

        public DisplayScreen()
        {
            // Display parameters
            Width = Console.WindowWidth - MarginLeft - MarginRight;
            Height = Console.WindowHeight - MarginTop - MarginBottom;
            MaxDropsOnScreen = (int)Math.Min(Width * Height / ( CodeDrop.MinDropLength + CodeDrop.MaxDropLength ), Width * 0.45);
            MaxDropsOnTopEdge = MaxDropsOnScreen / 2;
            // Initialize free lane set
            FreeLanes = [.. Enumerable.Range(0, Width)];
            Drops = [];
            RNG = new Random();
            UpdateTimer = new Timer(UpdateTimerInterval) { AutoReset = true };
            NextRepopulationTime = DateTime.Now;
            // Setup environment
            SetupEnvironment();
        }

        private void SetupEnvironment()
        {
            // Save old setttings
            OldBG = Console.BackgroundColor;
            OldFG = Console.ForegroundColor;
            // Customize console
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = TextColor;
            Console.CursorVisible = false;
            // Customize console title on Windows
            if(OperatingSystem.IsWindows())
            {
                OldConsoleTitle = Console.Title;
                Console.Title = "What is the Matrix?";
            }
        }

        private void TeardownEnvironment()
        {
            // Restore old settings
            Console.Title = OldConsoleTitle;
            Console.BackgroundColor = OldBG;
            Console.ForegroundColor = OldFG;
            Console.CursorVisible = true;
        }

        private CodeDrop AddDrop()
        {
            // Check if we can recycle an old drop
            CodeDrop drop = Drops.Where(d => d.HasLeftTheScreen).FirstOrDefault();
            if(drop != null)
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
            UpdateTimer.Elapsed += new ElapsedEventHandler(Update);
            NextRepopulationTime = DateTime.Now.AddMilliseconds(RepopulateInterval);
            UpdateTimer.Enabled = true;
            // Wait for escape command
            while(!Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                // Do nothing
            }
            // Stop the timer and tear down the environment
            UpdateTimer.Enabled = false;
            TeardownEnvironment();
        }

        private void Update(Object source, ElapsedEventArgs e)
        {
            if(NextRepopulationTime < DateTime.Now)
            {
                // Check if there are enough drops touching the top edge, if not, add some more
                int dropBudget = Math.Min(MaxDropsOnScreen - Drops.Where(d => !d.HasLeftTheScreen).Count(), MaxDropsOnTopEdge - Drops.Where(d => d.TouchesTopEdge).Count());
                for(int i = 0; i < dropBudget; i++)
                {
                    AddDrop();
                }
                NextRepopulationTime = NextRepopulationTime.AddMilliseconds(RepopulateInterval);
            }
        }

        public static void WriteChar(int XPos, int YPos, char Payload, ConsoleColor ForegroundColor)
        {
            // Cast inputs
            short x = (short)( XPos + MarginLeft );
            short y = (short)( YPos + MarginTop );
            byte ch = (byte)Payload;
            short att = (short)ForegroundColor;
            // Call API
            ConsoleAPI.WriteChar(x, y, ch, att);
        }
    }
}
