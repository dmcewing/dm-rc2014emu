using RC2014.EMU;
using RC2014.EMU.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014VM.UI
{
    public static class MachineConfigurations
    {
        public static IModule[] RC2014Plus32k => new IModule[]
            {
                new RAM32(),
                new PageableROM("24886009.BIN",0),
                new SIO() { debugLevel = SIO.DebugLevel.None }
            };

        public static IModule[] RC2014Plus64k => new IModule[]
            {
                new RAM64(0x2000, 0xFFFF),
                new PageableROM("24886009.BIN",1,0x1FFF),
                new SIO() { debugLevel = SIO.DebugLevel.None }
            };
        public static IModule[] RC2014PlusMonitor => new IModule[]
            {
                new RAM64(0x2000, 0xFFFF),
                new PageableROM("24886009.BIN",7,0x1FFF),
                new SIO() { debugLevel = SIO.DebugLevel.None }
            };

        public static IModule[] RC2014Plus88 => new IModule[]
          {
                new RAM64(0x4000, 0xFFFF),
                new PageableROM("24886009.BIN",2,0x3FFF),
                new SIO() { debugLevel = SIO.DebugLevel.None }
          };

        public static IModule[] RC2014Pro => new IModule[]
            {
                new RAM512("RCZ80_std.rom"),
                new SIO() { debugLevel = SIO.DebugLevel.None },
                new DSRTC()
                //new PortMonitor()
            };

        public static IModule[] RC2014ProBqrtc => new IModule[]
            {
                new RAM512("RCZ80_std_BQRTC.rom"),
                new SIO() { debugLevel = SIO.DebugLevel.None },
                new BQRTC()
                //new PortMonitor()
            };
    }
}
