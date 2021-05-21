using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU
{
    public interface IPort: IModule
    {
        ushort[] HandledPorts { get; }

        byte GetData(ushort port);
        void SetData(ushort port, byte value);
    }
}
