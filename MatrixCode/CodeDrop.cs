using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace MatrixCode
{

    class CodeDrop : IComparable<CodeDrop>
    {
        public const int MinDropLength = 5;
        public const int MaxDropLength = 45;

        public const int MinTimerInterval = 50; // in milliseconds
        public const int MaxTimerInterval = 150;

        private const double GlowingPercentile = 0.5; // percentage of the fastest drops that should glow
        private const double MaxGlowingInterval = 1.0 / (1.0 / MaxTimerInterval + (1.0 - GlowingPercentile) * (1.0 / MinTimerInterval - 1.0 / MaxTimerInterval));

        private const int MeanIntervalBetweenRandomChanges = 250;// in milliseconds
        private double ProbabilityOfRandomChange;

        private static char[] Symbols;

        private DisplayScreen MyScreen;
        public readonly int ID;
        public int XPos { get; private set; }
        private int YPos;
        private int Size;
        private bool Glow;
        private char LastChar;

        private Timer MyTimer;

        public bool TouchesTopEdge
        {
            get
            {
                return YPos < Size;
            }
        }

        public bool TouchesBottomEdge
        {
            get
            {
                return YPos > MyScreen.Height;
            }
        }

        public bool HasLeftTheScreen
        {
            get
            {
                return YPos - Size > MyScreen.Height;
            }
        }

        public CodeDrop(DisplayScreen Caller, int NewID, int Lane)
        {
            Debug.Assert(Caller.RNG != null);
            Debug.Assert(NewID >= 0);
            Debug.Assert(Lane >= 0);
            // Set allowed drop symbols íf not yet set
            if (Symbols == null)
            {
                Symbols = Enumerable.Concat(Enumerable.Range(33, 94), Enumerable.Concat(Enumerable.Range(128, 91), Enumerable.Range(224, 30))).Select(i => (char)i).ToArray();
            }
            // Set parameters
            MyScreen = Caller;
            ID = NewID;
            // Set the timer
            MyTimer = new Timer();
            MyTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            MyTimer.AutoReset = true;
            // Reset state
            Reset(Lane);
        }

        public void Reset(int NewLane)
        {
            // Reset position
            XPos = NewLane;
            YPos = -1;
            // Sample a new length
            Size = MyScreen.RNG.Next(MinDropLength, MaxDropLength);
            // Reset timer
            MyTimer.Interval = MyScreen.RNG.Next(MinTimerInterval, MaxTimerInterval);
            MyTimer.Enabled = true;
            // Check whether to glow
            Glow = (MyTimer.Interval < MaxGlowingInterval);
            // Recalculate random change probability
            ProbabilityOfRandomChange = (double)MyTimer.Interval / MeanIntervalBetweenRandomChanges;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (!HasLeftTheScreen)
            {
                UpdateState();
                Display();
            }
            else
            {
                MyTimer.Enabled = false;
            }
        }

        public void UpdateState()
        {
            YPos += 1;
        }

        public void Display()
        {
            // If the first character of this drop glows, write the character above again but without the glow
            if (Glow && YPos > 0 && YPos <= MyScreen.Height + 1)
            {
                MyScreen.WriteChar(XPos, YPos - 1, LastChar, DisplayScreen.TextColor);
            }
            // Generate a new char and let it glow if necessary
            if (!TouchesBottomEdge)
            {
                LastChar = Symbols[MyScreen.RNG.Next(Symbols.Length)];
                MyScreen.WriteChar(XPos, YPos, LastChar, Glow ? DisplayScreen.GlowingTextColor : DisplayScreen.TextColor);
            }
            // If the drop no longer touches the top edge, erase the symbol right above it to create the illusion of movement
            if (!TouchesTopEdge)
            {
                MyScreen.WriteChar(XPos, YPos - Size, ' ', DisplayScreen.BackgroundColor);
            }
            // Randomly alter a previously placed character
            if (MyScreen.RNG.NextDouble() < ProbabilityOfRandomChange)
            {
                int MinY = Math.Max(0, YPos - Size + 1), MaxY = Math.Min(YPos - 1, MyScreen.Height + 1);
                MyScreen.WriteChar(XPos, MyScreen.RNG.Next(MinY, MaxY), Symbols[MyScreen.RNG.Next(Symbols.Length)], DisplayScreen.TextColor);
            }
        }

        public int CompareTo(CodeDrop other)
        {
            return this.XPos - other.XPos;
        }
    }
}
