using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MatrixCode
{

    class CodeDrop : IComparable<CodeDrop>
    {
        public const int MinDropLength = 5;
        public const int MaxDropLength = 45;

        public static char[] Symbols;

        private DisplayScreen MyScreen;
        public readonly int ID;
        public int XPos { get; private set; }
        private int YPos;
        private int Size;

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
            YPos = 0;
            // Sample a new length
            Size = MyScreen.RNG.Next(MinDropLength, MaxDropLength);
        }

        public void Update()
        {
            YPos += 1;
        }

        public void Display()
        {
            if (!TouchesBottomEdge)
            {
                MyScreen.WriteChar(XPos, YPos, Symbols[MyScreen.RNG.Next(Symbols.Length)]);
            }
            // If the drop no longer touches the top edge, erase the symbol right above it
            // to create the illusion of movement
            if (!TouchesTopEdge)
            {
                MyScreen.WriteChar(XPos, YPos - Size, ' ');
            }
        }

        public int CompareTo(CodeDrop other)
        {
            return this.XPos - other.XPos;
        }
    }
}
