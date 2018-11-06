using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MatrixCode
{

    class CodeDrop
    {
        public const int MinDropLength = 5;
        public const int MaxDropLength = 45;

        public static char[] Symbols;

        private DisplayScreen MyScreen;
        private readonly int ID;
        public int XPos { get; private set; }
        private int YPos;
        private int Size;
        private List<char> Content;

        public bool IsOnScreen
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
                return YPos <= Size;
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
            YPos = -1;
            // Sample a new length
            Size = MyScreen.RNG.Next(MinDropLength, MaxDropLength);
            // Clear old content and replace it with content of new length
            Content.Clear();
            for(int i = 0; i < Size; i++)
            {
                Content.Add(Symbols[MyScreen.RNG.Next(Symbols.Length)]);
            }
        }
    }
}
