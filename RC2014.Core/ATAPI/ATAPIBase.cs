using RC2014.Core.ATAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	///
	/// LBA3 Register
	///
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
	public abstract class ATAPIBase : IDrivePort
	{
        public bool Trace { get; set; } = false;

		protected virtual ushort BasePort { get; }

		public IDEDrive[] Drive = 
		{
			/*0*/ new IDEDrive(), 
			/*1*/ new IDEDrive()
		};

		public int SelectedDrive { get; set; } = 0;

		public ushort data_latch;

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
			IDEDrive drive = Drive[SelectedDrive];
			IDETaskFile taskFile = drive.TaskFile;
            switch (register)
            {
                case ReadRegisters.CTRL_DRVADR:
					//return DriveAddress;
					//break;
					return 0xFF;

                case ReadRegisters.CMD_DATA:
					return drive.ReadByte();

                case ReadRegisters.CMD_ERR:
					return (byte)taskFile.error;

                case ReadRegisters.CMD_COUNT:
					return taskFile.count;

                case ReadRegisters.CMD_SECT:
					return taskFile.lba1;

                case ReadRegisters.CMD_CYLLO:
					return taskFile.lba2;

                case ReadRegisters.CMD_CYLHI:
					return taskFile.lba3;

				case ReadRegisters.CMD_DRVHD:
					return taskFile.lba4;

                case ReadRegisters.CMD_STAT:
					drive.intrq = false;
					goto case ReadRegisters.CTRL_ALTSTAT; 

				case ReadRegisters.CTRL_ALTSTAT:
					return (byte)taskFile.Status;

			}
			
			return 0xFF;
		}


		public virtual void SetData(ushort port, byte value)
		{
			WriteRegisters register = (WriteRegisters)(port - BasePort);

			IDEDrive drive = Drive[SelectedDrive];
			IDETaskFile taskFile = drive.TaskFile;

			if (register != WriteRegisters.CTRL_CTRL)
			{
				if ((taskFile.Status & StatusRegisterEnum.Busy) > 0)
				{
					Debug.WriteLine("Command written while busy");
					return;
				}
				/* Not clear this is the right emulation */
				if (!drive.present && register != WriteRegisters.CMD_DRVHD)
				{
					Debug.WriteLine("Not Present");
					return;
				}
			}

			switch (register)
			{
				case WriteRegisters.CTRL_CTRL:
					/* ATA: "When the Device Control register is written, both devices
					   respond to the write regardless of which device is selected" */
					if (((value ^ taskFile.devctrl) & 0x4) > 0)  //DCL_SRST
					{
						if ((value & 0x4) > 0)
							SRSTBegin();
						else
							SRSTEnd();
					}
					Drive[0].TaskFile.devctrl = value; // Check versus real h/w does this end up cleared 
					Drive[1].TaskFile.devctrl = value;
					break;

				case WriteRegisters.CMD_DATA:
					drive.Write(value, 1);
					break;

				case WriteRegisters.CMD_FEAT:
					taskFile.feature = value;
					break;

				case WriteRegisters.CMD_COUNT:
					taskFile.count = value;
					break;

				case WriteRegisters.CMD_SECT:
					taskFile.lba1 = value;
					break;

				case WriteRegisters.CMD_CYLLO:
					taskFile.lba2 = value;
					break;

				case WriteRegisters.CMD_CYLHI:
					taskFile.lba3 = value;
					break;

				case WriteRegisters.CMD_DRVHD:
					SelectedDrive = (value & (byte)0x10) > 0 ? 1 : 0;
					Drive[SelectedDrive].TaskFile.lba4 = (byte)(value & (byte)0x5F); //v & (DEVH_HEAD | DEVH_DEV | DEVH_LBA);
					break;

				case WriteRegisters.CMD_CMD:
					taskFile.command = value.GetCommand();
					IssueCommand(taskFile, drive);
					break;
			}
		}

		internal void IssueCommand(IDETaskFile taskFile, IDEDrive drive)
		{
			taskFile.Status &= ~(StatusRegisterEnum.Error | StatusRegisterEnum.DriveReady);
			taskFile.Status |= StatusRegisterEnum.Busy;
			taskFile.error = ErrorRegisterEnum.CLEAR;
			drive.state = IDEDrive.StateEnum.CMD;

			/* We could complete with delays but don't do so yet */
			switch (taskFile.command)
			{
				case IDECommands.CMD_EDD:
					this.EDDComplete();
					break;

				case IDECommands.CMD_IDENTIFY:
					drive.Identify();
					break;

				case IDECommands.CMD_INTPARAMS:
					drive.InitParam();
					break;

				case IDECommands.CMD_READ:
				case IDECommands.CMD_READ_NR:
					drive.ReadSectors();
					break;

				case IDECommands.CMD_SETFEATURES:
					drive.SetFeatures();
					break;

				case IDECommands.CMD_VERIFY:
				case IDECommands.CMD_VERIFY_NR:
					drive.VerifySectors();
					break;

				case IDECommands.CMD_WRITE:
				case IDECommands.CMD_WRITE_NR:
					drive.WriteSectors();
					break;

				case IDECommands.CMD_CALIB:
				case IDECommands.CMD_SEEK:
				default:
					if (((int)taskFile.command & 0xF0) == (int)IDECommands.CMD_CALIB) /* 1x */
					{
						drive.Recalibrate();
					}
					else if (((int)taskFile.command & 0xF0) == (int)IDECommands.CMD_SEEK) /* 7x */
					{
						drive.SeekComplete();
					}
					else
					{
						/* Unknown */
						taskFile.Status |= StatusRegisterEnum.Error;
						taskFile.error |= ErrorRegisterEnum.ABRT;
						drive.Completed();
					}
					break;
			}
		}


		protected void Reset()
		{
			Drive[0].Reset();
			Drive[1].Reset();
			SelectedDrive = 0;
		}

		protected void SRSTBegin()
        {
			Reset();
			Drive[0].SetBusy();
			Drive[1].SetBusy();
		}

		protected void SRSTEnd()
		{
			Drive[0].Ready();
			Drive[1].Ready();
		}

		public virtual void LoadState(Stream loadStream)
		{
		}

		public virtual void SaveState(Stream saveStream)
		{
		}

        public void Attach(int drive, Stream fd)
		{
			IDEDrive d = Drive[drive];
			if (d.present)
			{
				throw new Exception("Disk already in drive!");
			}

			byte[] buffer = new byte[512];
			if (fd.Read(d.Data, 0, 512) != 512)
			{
				throw new Exception("I/O error on attach");
			}
			if (fd.Read(buffer, 0, 512) != 512)  //First block of 1024 bytes read.
            {
				throw new Exception("I/O error 2 on attach");
			}

			byte[] magic_in = d.Data[0..8];
			if (!IDEDrive.CompareMagic(magic_in))
			{
				throw new Exception("Bad magic");
			}

			d.fd = fd;
			d.present = true;
			d.identify = buffer.ToShortArray();
			d.Heads = d.identify[3].LittleEndian();
			d.Sectors = d.identify[6].LittleEndian();
			d.Cylinders = d.identify[1].LittleEndian();
			if (d.identify[49].LittleEndian() != 0 & ((ushort)(1 << 9)).LittleEndian() != 0)
				d.lba = true;
			else
				d.lba = false;

			d.Setup();
		}

		public void Detatch(int drive)
		{
			IDEDrive d = Drive[drive];
			d.fd.Close();
			d.present = false;
		}

        public void Close()
        {
            for (int i = 0; i < Drive.Length; i++)
            {
				if (Drive[i].present)
					Detatch(i);
            }
        }
    }
}
