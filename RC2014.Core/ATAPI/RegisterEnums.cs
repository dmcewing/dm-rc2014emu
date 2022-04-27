using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
	internal enum ReadRegisters
	{
		CTRL_ALTSTAT = 0x0E,
		CTRL_DRVADR = 0x0F,
		CMD_DATA = 0x00,
		CMD_ERR = 0x01,
		CMD_COUNT = 0x02,
		CMD_SECT = 0x03,
		CMD_CYLLO = 0x04,
		CMD_CYLHI = 0x05,
		CMD_DRVHD = 0x06,
		//* LBA0-3 ARE ALTERNATE DEFINITIONS OF SECT, CYL, AND DRVHD PORTS
		//CMD_LBA0 = 0x03,
		//CMD_LBA1 = 0x04,
		//CMD_LBA2 = 0x05,
		//CMD_LBA3 = 0x06,
		CMD_STAT = 0x07
	}

	internal enum WriteRegisters
	{
		CTRL_CTRL = 0x0E,
		CMD_DATA = 0x00,
		CMD_FEAT = 0x01,
		CMD_COUNT = 0x02,
		CMD_SECT = 0x03,
		CMD_CYLLO = 0x04,
		CMD_CYLHI = 0x05,
		CMD_DRVHD = 0x06,
		//* LBA0-3 ARE ALTERNATE DEFINITIONS OF SECT, CYL, AND DRVHD PORTS
		//CMD_LBA0 = 0x03,
		//CMD_LBA1 = 0x04,
		//CMD_LBA2 = 0x05,
		//CMD_LBA3 = 0x06,
		CMD_CMD = 0x07
	}

}
