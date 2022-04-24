using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RC2014.Core
{
    public interface IConsoleFeed
    {
        void Initalise(Func<ConsoleKeyInfo, bool> handleKey, Func<bool> isStopping);

        void Write(byte value);
        void Write(string value);
        void Write(char value);

    }
}
