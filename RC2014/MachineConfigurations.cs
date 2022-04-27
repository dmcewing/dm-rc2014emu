using RC2014.Core;
using RC2014.Core.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014
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
                            new PageableROM("ROMS/24886009.ROM",0),
                            new RAM32(), //0x8000
                            new SIO() { debugLevel = SIO.DebugLevel.None}
                        };

                case ConfigurationEnum.RC2014Plus64k:
                    return new IModule[]
                        {
                            new PageableROM("ROMS/24886009.ROM",1,0x2000),
                            new RAM64(0x2000),
                            new SIO() { debugLevel = SIO.DebugLevel.None }
                        };

                case ConfigurationEnum.RC2014PlusMonitor:
                    return new IModule[]
                        {
                            new PageableROM("ROMS/24886009.ROM",7,0x2000),
                            new RAM64(0x2000),
                            new SIO() { debugLevel = SIO.DebugLevel.None }
                        };
                case ConfigurationEnum.RC2014Plus88:
                    return new IModule[]
                        {
                            new PageableROM("ROMS/24886009.ROM",2,0x4000),
                            new RAM64(0x4000),
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
                    return new IModule[]
                        {
                            new RAM512("ROMS/RCZ80_std.rom"),
                            new SIO() { debugLevel = SIO.DebugLevel.None },
                            new DSRTC(),
                            new CF()
                            //new PortMonitor()
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
