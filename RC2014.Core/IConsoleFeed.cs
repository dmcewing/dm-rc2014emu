using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RC2014.Core
{
    public delegate bool KeyPressHandler(ConsoleKeyInfo keyInfo);

    public interface IConsoleFeed
    {
        void Initalise();

        void Write(byte value);
        void Write(string value);
        void Write(char value);

        void KeyboardHandler(KeyPressHandler keyPressHandler);
        void Reset();
    }
}
