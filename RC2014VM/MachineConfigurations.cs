using RC2014.EMU;
using RC2014.EMU.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014VM.UI
{
    public enum ConfigurationEnum
    {
        RC2014Plus32k,
        RC2014Plus64k,
        RC2014PlusMonitor,
        RC2014Plus88,
        RC2014Pro,
        RC2014ProBqrtc,
        RC2014ProCF
    }

    public static class MachineConfigurations
    {
        public static IModule[] GetConfigurations(ConfigurationEnum configuration)
        {
            switch (configuration)
            {
                case ConfigurationEnum.RC2014Plus32k:
                    return new IModule[]
                        {
                            new RAM32(),
                            new PageableROM("ROMS/24886009.ROM",0),
                            new SIO() { debugLevel = SIO.DebugLevel.None }
                        };

                case ConfigurationEnum.RC2014Plus64k:
                    return new IModule[]
                        {
                            new RAM64(0x2000, 0xFFFF),
                            new PageableROM("ROMS/24886009.ROM",1,0x1FFF),
                            new SIO() { debugLevel = SIO.DebugLevel.None }
                        };

                case ConfigurationEnum.RC2014PlusMonitor:
                    return new IModule[]
                        {
                            new RAM64(0x2000, 0xFFFF),
                            new PageableROM("ROMS/24886009.ROM",7,0x1FFF),
                            new SIO() { debugLevel = SIO.DebugLevel.None }
                        };
                case ConfigurationEnum.RC2014Plus88:
                    return new IModule[]
                        {
                            new RAM64(0x4000, 0xFFFF),
                            new PageableROM("ROMS/24886009.ROM",2,0x3FFF),
                            new SIO() { debugLevel = SIO.DebugLevel.None }
                        };

                case ConfigurationEnum.RC2014ProBqrtc:
                    return new IModule[]
                        {
                            new RAM512("ROMS/RCZ80_std_BQRTC.rom"),
                            new SIO() { debugLevel = SIO.DebugLevel.None },
                            new BQRTC()
                            //new PortMonitor()
                        };

                case ConfigurationEnum.RC2014ProCF:
                    var ram64 = new RAM64(0x0000, 0xFFFF);
                    
                    return new IModule[]
                        {
                            new PageableROM("ROMS/24886009.ROM", 4, 0x1FFF, ram64),
                            new SIO() { debugLevel = SIO.DebugLevel.None },
                            new DSRTC(),
                            new PortMonitor()
                        };

                case ConfigurationEnum.RC2014Pro:
                default:
                    return new IModule[]
                        {
                            new RAM512("ROMS/RCZ80_std.rom"),
                            new SIO() { debugLevel = SIO.DebugLevel.None },
                            new DSRTC(),
                            //new PortMonitor()
                        };
            }
        }

    }
}
