using RC2014.Core.ATAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core
{
	/// <summary>
	/// Provide a base for interpretting ATAPI control interface.
	/// </summary>
	/// <remarks>
	///  +----------------------------------------------------------------------+
	///  | CONTROL BLOCK REGISTERS												|
	///  +----------------------+-------+-------+-------------------------------+
	///  | REGISTER				| PORT	| DIR	| DESCRIPTION					|
	///  +----------------------+-------+-------+-------------------------------+
	///  | IDE_IO_ALTSTAT		| 0x0E	| R		| ALTERNATE STATUS REGISTER		|
	///  | IDE_IO_CTRL			| 0x0E	| W		| DEVICE CONTROL REGISTER		|
	///  | IDE_IO_DRVADR		| 0x0F	| R		| DRIVE ADDRESS REGISTER		|
	///  +----------------------+-------+-------+-------------------------------+
	///  
	///  +----------------------+-------+-------+-------------------------------+
	///  | COMMAND BLOCK REGISTERS												|
	///  +----------------------+-------+-------+-------------------------------+
	///  | REGISTER				| PORT	| DIR	| DESCRIPTION					|
	///  +----------------------+-------+-------+-------------------------------+
	///  | IDE_IO_DATA			| 0x00	| R/W	| DATA INPUT/OUTPUT				|
	///  | IDE_IO_ERR			| 0x01	| R		| ERROR REGISTER				|
	///  | IDE_IO_FEAT			| 0x01	| W		| FEATURES REGISTER				|
	///  | IDE_IO_COUNT			| 0x02	| R/W	| SECTOR COUNT REGISTER			|
	///  | IDE_IO_SECT			| 0x03	| R/W	| SECTOR NUMBER REGISTER		|
	///  | IDE_IO_CYLLO			| 0x04	| R/W	| CYLINDER NUM REGISTER (LSB)	|
	///  | IDE_IO_CYLHI			| 0x05	| R/W	| CYLINDER NUM REGISTER (MSB)	|
	///  | IDE_IO_DRVHD			| 0x06	| R/W	| DRIVE/HEAD REGISTER			|
	///  | IDE_IO_LBA0*			| 0x03	| R/W	| LBA BYTE 0 (BITS 0-7)			|
	///  | IDE_IO_LBA1*			| 0x04	| R/W	| LBA BYTE 1 (BITS 8-15)		|
	///  | IDE_IO_LBA2*			| 0x05	| R/W	| LBA BYTE 2 (BITS 16-23)		|
	///  | IDE_IO_LBA3*			| 0x06	| R/W	| LBA BYTE 3 (BITS 24-27)		|
	///  | IDE_IO_STAT			| 0x07	| R		| STATUS REGISTER				|
	///  | IDE_IO_CMD			| 0x07	| W		| COMMAND REGISTER (EXECUTE)	|
	///  +----------------------+-------+-------+-------------------------------+
	///  * LBA0-3 ARE ALTERNATE DEFINITIONS OF SECT, CYL, AND DRVHD PORTS
	/// </remarks>
	public abstract class ATAPIBase : IPort
	{
		public bool Trace { get; set; } = true;

		protected virtual ushort BasePort { get; }

		public ATAPIBase()
		{
			//Hook ports...
			var portList = new List<ushort>();
			var basePort = BasePort;
			for (ushort i = 0; i < 16; i++)
			{
				portList.Add((ushort)(basePort + i));
			}
			HandledPorts = portList.ToArray();
		}

		public ushort[] HandledPorts { get; private set; }

		public virtual byte GetData(ushort port)
		{
			ReadRegisters register = (ReadRegisters)(port - BasePort);
            switch (register)
            {
                case ReadRegisters.CTRL_ALTSTAT:
					return AlternateStatus;
                case ReadRegisters.CTRL_DRVADR:
					return DriveAddress;
                case ReadRegisters.CMD_DATA:
                    break;
                case ReadRegisters.CMD_ERR:
					return Error.Value;
                case ReadRegisters.CMD_COUNT:
					return SectorCount;

                case ReadRegisters.CMD_SECT:
                case ReadRegisters.CMD_CYLLO:
                case ReadRegisters.CMD_CYLHI:
				case ReadRegisters.CMD_DRVHD:
					return GetLBAData(register);

                case ReadRegisters.CMD_STAT:
					return Status.Value;
			}

			return 0x00;
		}


        public virtual void SetData(ushort port, byte value)
		{
			WriteRegisters register = (WriteRegisters)(port - BasePort);
            switch (register)
            {
                case WriteRegisters.CTRL_CTRL:
					DeviceControl = new DeviceControlRegister(value);
                    break;

                case WriteRegisters.CMD_DATA:
                    break;

                case WriteRegisters.CMD_FEAT:
                    break;

                case WriteRegisters.CMD_COUNT:
					SectorCount = value;
                    break;

                case WriteRegisters.CMD_SECT:
                case WriteRegisters.CMD_CYLLO:
                case WriteRegisters.CMD_CYLHI:
                case WriteRegisters.CMD_DRVHD:
					SetLBAData(register, value);
                    break;

                case WriteRegisters.CMD_CMD:
                    break;
            }
        }

        private void SetLBAData(WriteRegisters register, byte value)
        {
			if (register == WriteRegisters.CMD_DRVHD)
			{
				LBA3 = value;
				return;
			}

			if (AddressingMode == AddressingType.CHS_ADDRESSING)
			{
				switch (register)
				{
					case WriteRegisters.CMD_SECT:
						SectorNumber = value;
						break;
					case WriteRegisters.CMD_CYLLO:
						CylinderNumber = (ushort)((CylinderNumber & 0xFF00) + (value & 0xFF));
						break;
					case WriteRegisters.CMD_CYLHI:
						CylinderNumber = (ushort)((CylinderNumber & 0x00FF) + (value & 0xFF) * 0x0100);
						break;
				}
			}
			else
            {
                switch (register)
                {
                    case WriteRegisters.CMD_SECT:
						LBA0 = value;
                        break;
                    case WriteRegisters.CMD_CYLLO:
						LBA1 = value;
						break;
                    case WriteRegisters.CMD_CYLHI:
						LBA2 = value;
						break;
                }
            }
		}

        private byte GetLBAData(ReadRegisters register)
        {
			byte[] dataArray;
			if (AddressingMode == AddressingType.CHS_ADDRESSING)
				dataArray = new byte[4] {
						SectorNumber,
						(byte)(CylinderNumber & 0xFF),
						(byte)(CylinderNumber & 0xFF00 >> 0x8),
						LBA3
				};
			else
				dataArray = new byte[4] {
					LBA0, LBA1, LBA2, LBA3
				};

			return dataArray[(int)register - 3];
        }

		public virtual void LoadState(IFormatter formatter, Stream loadStream)
		{
		}

		public virtual void SaveState(IFormatter formatter, Stream saveStream)
		{
		}

		public  byte Drive { get; set; } = 0;

		public AddressingType AddressingMode { get; set; }

		public StatusRegister Status { get; set; } = new StatusRegister();
		public ErrorRegister Error { get; set; } = new ErrorRegister();
		public DeviceControlRegister DeviceControl { get; set; } = new DeviceControlRegister();

		public byte AlternateStatus { get;set; } = 0;
		public byte DriveAddress { get; set; } = 0;
		public byte Features { get; set; } = 0;
		public byte SectorCount { get; set; } = 0;
		public byte SectorNumber { get; set; } = 0;
		public ushort CylinderNumber { get; set; } = 0;
		public byte DriveHead { get; set; } = 0;

		protected uint LBA { get; set; } = 0;

		public byte LBA0 
		{ 
			get { return (byte)(LBA & 0x000000FF >> 0); } 
			set { LBA |= (byte)(value * 0x00000001); }
		}
		public byte LBA1
		{
			get { return (byte)(LBA & 0x0000FF00 >> 0x8); }
			set { LBA |= (byte)(value * 0x00000000); }
		}
		public byte LBA2
		{
			get { return (byte)(LBA & 0x00FF0000 >> 0x10); }
			set { LBA |= (byte)(value * 0x00000000); }
		}

		/// <summary>
		/// LBA3 Register
		/// </summary>
		/// <remarks>
		/// === DRIVE/HEAD / LBA3 REGISTER ===
		/// 
		/// 7	    6	    5	    4	    3	    2	    1	    0
		/// +-------+-------+-------+-------+-------+-------+-------+-------+
		/// |   1	|   L	|   1	|  DRV	|  HS3	|  HS2	|  HS1	|  HS0	|
		/// +-------+-------+-------+-------+-------+-------+-------+-------+
		/// 
		/// L:	 0 = CHS ADDRESSING, 1 = LBA ADDRESSING
		/// DRV: 0 = DRIVE 0 (PRIMARY) SELECTED, 1 = DRIVE 1 (SLAVE) SELECTED
		/// HS:	 CHS = HEAD ADDRESS (0-15), LBA = BITS 24-27 OF LBA
		/// </remarks>
		public virtual byte LBA3
		{
			get
			{
				return (byte)(
					  0b_1010_0000
					+ (byte)AddressingMode
					+ (Drive * (byte)0x10)
					+ ((AddressingMode == AddressingType.CHS_ADDRESSING)
                        ? DriveHead //head
                        : (byte)(LBA & 0x0F000000 >> 24)
						)
					);
			}
			set
			{
				if (AddressingMode == AddressingType.CHS_ADDRESSING)
					DriveHead = (byte)(value & 0xF);
				else
					LBA = (LBA & 0x00FFFFFFU) + ((value & 0xFU) * 0x01000000U);
            }
		}
        
    }
}
