using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU
{
    //public interface IConsole
    //{
    //    void Write(char c);
    //    void Read(char c);
    //}

    public interface IConsoleFeed
    {
        void SetOutput(TextWriter output);
        void Write(byte value);
        void Write(string value);
        void Write(char value);
    }
}
