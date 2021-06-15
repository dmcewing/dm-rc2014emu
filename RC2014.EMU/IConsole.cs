using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RC2014.EMU
{
    //public interface IConsole
    //{
    //    void Write(char c);
    //    void Read(char c);
    //}
    public delegate bool KeyPressHandler(ConsoleKeyInfo keyInfo);

    public interface IConsoleFeed
    {
        void Initalise();

        //void SetOutput(TextWriter output);
        void Write(byte value);
        void Write(string value);
        void Write(char value);

        void KeyboardHandler(CancellationToken cancellationToken, KeyPressHandler keyPressHandler);
        void Reset();
    }
}
