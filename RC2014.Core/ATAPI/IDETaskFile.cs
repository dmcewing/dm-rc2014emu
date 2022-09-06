using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
    public class IDETaskFile
    {
        public IDETaskFile()
        {
            Setup();
        }

        public ushort data;
        public byte feature;
        public byte count;
        public byte lba1;
        public byte lba2;
        public byte lba3;
        public byte lba4;

        public ErrorRegisterEnum error;
        public StatusRegisterEnum Status { get; set; } = 0;

        public IDECommands command;
        public byte devctrl;

        internal void Setup()
        {
            error = ErrorRegisterEnum.AMNF;       /* All good */
            lba1 = 0x01;        /* EDD always updates drive 0 */
            lba2 = 0x00;
            lba3 = 0x00;
            lba4 = 0x00;
            count = 0x01;
        }        
    }
}
