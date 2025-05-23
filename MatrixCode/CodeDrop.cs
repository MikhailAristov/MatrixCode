using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace MatrixCode
{

    class CodeDrop : IComparable<CodeDrop>
    {
        public const int MinDropLength = 5;
        public const int MaxDropLength = 45;

        private const int MinUpdateInterval = 3 * DisplayScreen.UpdateTimerInterval; // in milliseconds
        private const int MaxUpdateInterval = 8 * DisplayScreen.UpdateTimerInterval;

        private const double GlowingPercentile = 0.5; // percentage of the fastest drops that should glow
        private const double MaxGlowingInterval = 1.0 / ( 1.0 / MaxUpdateInterval + ( 1.0 - GlowingPercentile ) * ( 1.0 / MinUpdateInterval - 1.0 / MaxUpdateInterval ) );

        private const int MeanIntervalBetweenRandomChanges = 250;// in milliseconds
        private double ProbabilityOfRandomChange;

        private static char[] Symbols;

        private readonly DisplayScreen MyScreen;
        public readonly int ID;
        public int XPos { get; private set; }
        private int YPos;
        private int Size;
        private bool Glow;
        private char LastChar;

        private int MyUpdateInterval;
        private DateTime NextUpdateTime;
        private bool IsUpdating;

        public bool TouchesTopEdge => YPos < Size;

        public bool TouchesBottomEdge => YPos > MyScreen.Height;

        public bool HasLeftTheScreen => YPos - Size > MyScreen.Height;

        public CodeDrop(DisplayScreen Caller, int NewID, int Lane)
        {
            Debug.Assert(Caller.RNG != null);
            Debug.Assert(NewID >= 0);
            Debug.Assert(Lane >= 0);
            // Set allowed drop symbols íf not yet set
            Symbols ??= [.. Enumerable.Concat(Enumerable.Range(33, 94), Enumerable.Concat(Enumerable.Range(128, 91), Enumerable.Range(224, 30))).Select(i => (char)i)];
            // Set parameters
            MyScreen = Caller;
            ID = NewID;
            // Reset state
            Reset(Lane);
            // Finally, hear for the update 
            Caller.UpdateTimer.Elapsed += new ElapsedEventHandler(Update);
        }

        public void Reset(int NewLane)
        {
            // Reset position
            XPos = NewLane;
            YPos = -1;
            // Sample a new length
            Size = MyScreen.RNG.Next(MinDropLength, MaxDropLength);
            // Reset timer
            MyUpdateInterval = MyScreen.RNG.Next(MinUpdateInterval, MaxUpdateInterval);
            NextUpdateTime = DateTime.Now.AddMilliseconds(MyUpdateInterval);
            // Check whether to glow
            Glow = ( MyUpdateInterval < MaxGlowingInterval );
            // Recalculate random change probability
            ProbabilityOfRandomChange = (double)MyUpdateInterval / MeanIntervalBetweenRandomChanges;
        }

        private void Update(Object source, ElapsedEventArgs e)
        {
            if(!IsUpdating && !HasLeftTheScreen && NextUpdateTime < DateTime.Now)
            {
                IsUpdating = true;
                try
                {
                    YPos += 1;
                    NextUpdateTime = NextUpdateTime.AddMilliseconds(MyUpdateInterval);
                    Display();
                }
                catch
                {
                    DisplayScreen.WriteChar(XPos, YPos, '!', DisplayScreen.ErrorColor);
                }
                finally
                {
                    IsUpdating = false;
                }
            }
        }

        public void Display()
        {
            // If the first character of this drop glows, write the character above again but without the glow
            if(Glow && YPos > 0 && YPos <= MyScreen.Height + 1)
            {
                DisplayScreen.WriteChar(XPos, YPos - 1, LastChar, DisplayScreen.TextColor);
            }
            // Generate a new char and let it glow if necessary
            if(!TouchesBottomEdge)
            {
                LastChar = Symbols[MyScreen.RNG.Next(Symbols.Length)];
                DisplayScreen.WriteChar(XPos, YPos, LastChar, Glow ? DisplayScreen.GlowingTextColor : DisplayScreen.TextColor);
            }
            // If the drop no longer touches the top edge, erase the symbol right above it to create the illusion of movement
            if(!TouchesTopEdge)
            {
                DisplayScreen.WriteChar(XPos, YPos - Size, ' ', DisplayScreen.BackgroundColor);
            }
            // Randomly alter a previously placed character
            if(MyScreen.RNG.NextDouble() < ProbabilityOfRandomChange)
            {
                int MinY = Math.Max(0, YPos - Size + 1), MaxY = Math.Min(YPos - 1, MyScreen.Height + 1);
                if(MinY < MaxY)
                {
                    DisplayScreen.WriteChar(XPos, MyScreen.RNG.Next(MinY, MaxY), Symbols[MyScreen.RNG.Next(Symbols.Length)], DisplayScreen.TextColor);
                }
            }
        }

        public int CompareTo(CodeDrop other)
        {
            return XPos - other.XPos;
        }
    }
}
