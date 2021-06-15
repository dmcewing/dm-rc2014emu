using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Helpers
{
    public static class ConsoleHelper
    {
        private const int STD_INPUT_HANDLE = -10;

        private const int STD_OUTPUT_HANDLE = -11;

        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        // ReSharper restore InconsistentNaming

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        public static void SetVT100Mode()
        {
            SetVT100In();
            SetVT100Out();
        }

        public static void SetVT100Out()
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                Debug.WriteLine($"failed to get output console mode, error code: {GetLastError()}");
                return;
            }
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                Debug.WriteLine($"failed to set output console mode, error code: {GetLastError()}");
                return;
            }
        }

        public static void SetVT100In()
        {
            var iStdIn = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(iStdIn, out uint inConsoleMode))
            {
                Debug.WriteLine($"failed to get input console mode, error code: {GetLastError()}");
                return;
            }
            inConsoleMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;
            if (!SetConsoleMode(iStdIn, inConsoleMode))
            {
                Debug.WriteLine($"failed to set input console mode, error code: {GetLastError()}");
                return;
            }
        }
    }
}
