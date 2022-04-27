using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.Module
{
    public class CF : ATAPIBase
    {

        public CF(): base()
        {
        }

        protected override ushort BasePort => 0x10;

        public override byte GetData(ushort port)
        {
            if (Trace)
                Debug.WriteLine("CF: 0x{0:X2}: Reading port... ", port - HandledPorts[0]);

            return base.GetData(port);
        }

        public override void SetData(ushort port, byte value)
        {
            if (Trace)
                Debug.WriteLine("CF: 0x{0:X2} set to 0x{1:X2}", port - HandledPorts[0], value);

            base.SetData(port, value);
        }

    }
}
