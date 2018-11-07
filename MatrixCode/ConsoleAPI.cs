using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MatrixCode
{
    // Source: https://stackoverflow.com/questions/2754518/how-can-i-write-fast-colored-output-to-console
    class ConsoleAPI
    {
        private static SafeFileHandle H;
        private static SmallRect SingleCharRect = new SmallRect();
        private static readonly CharInfo[] SingleCharBuf = new CharInfo[1];
        private static readonly Coord Zero = new Coord() { X = 0, Y = 0 };
        private static readonly Coord OneByOne = new Coord() { X = 1, Y = 1 };

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteChar(short XPos, short YPos, byte Payload, short Attributes) {
            // Get the handle if necessary
            if (H == null)
            {
                H = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            }
            if (!H.IsInvalid)
            {
                // Set position
                SingleCharRect.Left = XPos;
                SingleCharRect.Right = XPos;
                SingleCharRect.Top = YPos;
                SingleCharRect.Bottom = YPos;
                // Set output color and content
                SingleCharBuf[0].Attributes = Attributes;
                SingleCharBuf[0].Char.AsciiChar = Payload;
                // Write output
                if (!WriteConsoleOutput(H, SingleCharBuf, OneByOne, Zero, ref SingleCharRect)) {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }
    }
}
