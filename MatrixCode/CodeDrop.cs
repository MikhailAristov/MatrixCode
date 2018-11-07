using System;
using System.Diagnostics;

namespace MatrixCode
{

    class CodeDrop : IComparable<CodeDrop>
    {
        public const int MinDropLength = 5;
        public const int MaxDropLength = 45;

        public const double ProbabilityOfGlowing = 0.3;

        public static char[] Symbols;

        private DisplayScreen MyScreen;
        public readonly int ID;
        public int XPos { get; private set; }
        private int YPos;
        private int Size;
        private bool Glow;
        private char LastChar;

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
            Debug.Assert(Symbols != null && Symbols.Length > 0);
            Debug.Assert(NewID >= 0);
            Debug.Assert(Lane >= 0);
            // Set parameters
            MyScreen = Caller;
            ID = NewID;
            Reset(Lane);
        }

        public void Reset(int NewLane)
        {
            // Reset position
            XPos = NewLane;
            YPos = -1;
            // Sample a new length
            Size = MyScreen.RNG.Next(MinDropLength, MaxDropLength);
            // Sample whether to glow
            Glow = (MyScreen.RNG.NextDouble() < ProbabilityOfGlowing);
        }

        public void UpdateState()
        {
            YPos += 1;
        }

        public void Display()
        {
            if (!TouchesBottomEdge)
            {
                // Generate a new char and let it glow if necessary
                char newChar = Symbols[MyScreen.RNG.Next(Symbols.Length)];
                MyScreen.WriteChar(XPos, YPos, newChar, Glow ? DisplayScreen.GlowingTextColor : DisplayScreen.TextColor);
                // If the first character glows, write the character above again but without the glow
                if (Glow && YPos > 0)
                {
                    MyScreen.WriteChar(XPos, YPos - 1, LastChar, DisplayScreen.TextColor);
                }
                LastChar = newChar;
            }
            // If the drop no longer touches the top edge, erase the symbol right above it
            // to create the illusion of movement
            if (!TouchesTopEdge)
            {
                MyScreen.WriteChar(XPos, YPos - Size, ' ', DisplayScreen.BackgroundColor);
            }
        }

        public int CompareTo(CodeDrop other)
        {
            return this.XPos - other.XPos;
        }
    }
}
