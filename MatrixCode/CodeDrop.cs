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
        private List<char> Content;

        public bool HasLeftTheScreen
        {
            get
            {
                return YPos - Size > MyScreen.Height;
            }
        }

        public bool TouchesTopEdge
        {
            get
            {
                return YPos < Size;
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
            Content = new List<char>();
            Reset(Lane);
        }

        public void Reset(int NewLane)
        {
            // Reset position
            XPos = NewLane;
            YPos = 0;
            // Sample a new length
            Size = MyScreen.RNG.Next(MinDropLength, MaxDropLength);
            // Clear old content and replace it with content of new length
            Content.Clear();
            for(int i = 0; i < Size; i++)
            {
                Content.Add(Symbols[MyScreen.RNG.Next(Symbols.Length)]);
            }
        }

        public void Update()
        {
            YPos += 1;
        }

        public void Display()
        {
            // if (IsOnScreen) {
            //(YPos >= 0) && (YPos - Size < MyScreen.Height)
            for (int i = 0; i < Size; i++)
            {
                int y = YPos - i;
                // Write the current symbol to its new position, if visible
                if (y >= 0 && y < MyScreen.Height)
                {
                    MyScreen.WriteChar(XPos, y, Content[i]);
                }
                // If the drop no longer touches the top edge, erase the symbol right above it
                // to create the illusion of movement
                if (!TouchesTopEdge)
                {
                    MyScreen.WriteChar(XPos, YPos - Size, ' ');
                }
            }
            //}
        }

        public int CompareTo(CodeDrop other)
        {
            return this.ID - other.ID;
        }
    }
}
