using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
    public class IDEDrive
    {
        public enum StateEnum
        {
            IDLE = 0,
            CMD = 1,
            DATA_IN = 2,
            DATA_OUT = 3
        }
        
        public const byte DEVH_HEAD = 0xF;
        public const byte nDEVH_HEAD = 0xF0;
        public const byte DEVH_LBA = 64;

        public IDETaskFile TaskFile = new();

        public bool present = false;
        public bool intrq = false;
        public bool failed = false;
        public bool lba = false;
        public bool eightbit = false;

        public ushort Cylinders;
        public ushort Heads;
        public ushort Sectors; 

        public byte[] Data  = new byte[512];
        public ushort[] identify = new ushort[256];

        public int DataPtr;
        public StateEnum state;
        public Stream fd;
        public int offset;
        public int length;

        private static readonly byte[] ide_magic = {
            49, //1
			68, //D
			69, //E
			68, //D
			49, //1
			53, //5
			67, //C
			48  //0
		};

        public void Setup()
        {
            TaskFile.Setup();
            Ready();
        }

        public IDEDrive()
        {
        }

        public void Reset()
        {
            if (present)
            {
                TaskFile.Setup();
                Ready();
                /* A drive could clear busy then set DRDY up to 2 minutes later if its
				   mindnumbingly slow to start up ! We don't emulate any of that */

                TaskFile.Status = StatusRegisterEnum.DriveReady; //Ensures other flags are actually cleared.
                eightbit = false;
            }
        }

        internal void SetBusy()
        {
            if (present)
                TaskFile.Status |= StatusRegisterEnum.Busy;
        }

        internal void Ready()
        {
            TaskFile.Status &= ~(StatusRegisterEnum.Busy | StatusRegisterEnum.DataRequest);
            TaskFile.Status |= StatusRegisterEnum.DriveReady;
            state = StateEnum.IDLE;
        }

        public static bool CompareMagic(byte[] magic_in)
        {
            if (magic_in.Length != ide_magic.Length)
                return false;

            var same = true;
            for (int i = 0; i < ide_magic.Length && same; i++)
            {
                same &= (magic_in[i] == ide_magic[i]);
            }
            return same;

        }

        public static void MakeDrive(DriveType type, Stream fd)
        {
            uint s, h;
            uint c;
            uint sectors;
            ushort[] ident = new ushort[256];

            ident[0] = (ushort)(ide_magic[0] * 0x100 + ide_magic[1]);
            ident[1] = (ushort)(ide_magic[2] * 0x100 + ide_magic[3]);
            ident[2] = (ushort)(ide_magic[4] * 0x100 + ide_magic[5]);
            ident[3] = (ushort)(ide_magic[6] * 0x100 + ide_magic[7]);

            fd.Write(ident.ToByteArray());

            ident = new ushort[256];
            ident[0] = Helpers.LittleEndian((1 << 15) + (1 << 6)); //Non removable
            //Make a serial number....
            ident = ident.MakeASCII(10, Guid.NewGuid().ToString("N"), 20);
            ident[47] = 0; //no read multi
            ident[51] = Helpers.LittleEndian(240 /*PIO2 */ << 8); //pio Cycle time
            ident[52] = Helpers.LittleEndian(1);

            switch (type)
            {
                case DriveType.ACME_ROADRUNNER:
                    /* 504MB drive with LBA support */
                    c = 1024;
                    h = 16;
                    s = 63;
                    ident = ident.MakeASCII(23, "A001.001", 8);
                    ident = ident.MakeASCII(27, "ACME ROADRUNNER v0.1", 40);
                    ident[49] = Helpers.LittleEndian(1 << 9); /* LBA */
                    break;
                case DriveType.ACME_ULTRASONICUS:
                    /* 40MB drive with LBA support */
                    c = 977;
                    h = 5;
                    s = 16;
                    ident[49] = Helpers.LittleEndian(1 << 9); /* LBA */
                    ident = ident.MakeASCII(23, "A001.001", 8);
                    ident = ident.MakeASCII(27, "ACME ULTRASONICUS AD INFINITUM v0.1", 40);
                    break;
                case DriveType.ACME_NEMESIS:
                    /* 20MB drive with LBA support */
                    c = 615;
                    h = 4;
                    s = 16;
                    ident[49] = Helpers.LittleEndian(1 << 9); /* LBA */
                    ident = ident.MakeASCII(23, "A001.001", 8);
                    ident = ident.MakeASCII(27, "ACME NEMESIS RIDICULII v0.1", 40);
                    break;
                case DriveType.ACME_COYOTE:
                    /* 20MB drive without LBA support */
                    c = 615;
                    h = 4;
                    s = 16;
                    ident = ident.MakeASCII(23, "A001.001", 8);
                    ident = ident.MakeASCII(27, "ACME COYOTE v0.1", 40);
                    break;
                case DriveType.ACME_ACCELLERATTI:
                    c = 1024;
                    h = 16;
                    s = 16;
                    ident[49] = Helpers.LittleEndian(1 << 9); /* LBA */
                    ident = ident.MakeASCII(23, "A001.001", 8);
                    ident = ident.MakeASCII(27, "ACME ACCELLERATTI INCREDIBILUS v0.1", 40);
                    break;
                case DriveType.ACME_ZIPPIBUS:
                default:
                    c = 1024;
                    h = 16;
                    s = 32;
                    ident[49] = Helpers.LittleEndian(1 << 9); /* LBA */
                    ident = ident.MakeASCII(23, "A001.001", 8);
                    ident = ident.MakeASCII(27, "ACME ZIPPIBUS v0.1", 40);
                    break;
            }

            ident[1] = ((ushort)c).LittleEndian();
            ident[3] = Helpers.LittleEndian((ushort)h);
            ident[6] = Helpers.LittleEndian((ushort)s);
            ident[54] = ident[1];
            ident[55] = ident[3];
            ident[56] = ident[6];
            sectors = c * h * s;
            ident[57] = ((ushort)(sectors & 0xFFFF)).LittleEndian();
            ident[58] = ((ushort)(sectors >> 16)).LittleEndian();
            ident[60] = ident[57];
            ident[61] = ident[58];

            fd.Write(ident.ToByteArray());

            var zeroSector = new byte[512];
            for (int i = 0; i < 512; i++)
            {
                zeroSector[i] = 0xE5;
            }

            while (0 < sectors--)
                fd.Write(zeroSector);

        }

        private void xlate_errno(int len)
        {
            TaskFile.Status |= StatusRegisterEnum.Error;
            if (len == -1)
            {
                TaskFile.error = ErrorRegisterEnum.UNC;
                //    if (errno == EIO)  //Input output error
                //    t->error = ERR_UNC;
                //  else
                //    t->error = ERR_AMNF;
            }
            else
                TaskFile.error = ErrorRegisterEnum.AMNF;
        }


        private bool ReadSector()
        {
            DataPtr = 0;
            var length = fd.Read(Data, 0, Data.Length);
            if (length != Data.Length)
            {
                //perror("ide_read_sector");
                //    d->taskfile.status |= ST_ERR;
                TaskFile.Status |= StatusRegisterEnum.Error;
                //    d->taskfile.status &= ~ST_DSC;
                TaskFile.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                xlate_errno(length);
                return false;
            }
            offset += Data.Length;
            return true;
        }

        public byte ReadByte()
        {
            var v = Read(1);
            return (byte)v;
        }

        public ushort Read(int length)
        {
            ushort v;
            if (state == StateEnum.DATA_IN)
            {
                if (DataPtr == Data.Length)
                {
                    if (!ReadSector())
                    {
                        SetError();   /* Set the LBA or CHS etc */
                        return 0xFFFF;      /* and error bits set by read_sector */
                    }
                }
                v = Data[DataPtr];
                if (!eightbit)
                {
                    if (length == 2)
                        v |= (ushort)(Data[DataPtr + 1] << 8);
                    DataPtr += 2;
                }
                else
                    DataPtr++;

                TaskFile.data = v;
                if (DataPtr == Data.Length)
                {
                    length--;
                    intrq = true;
                    if (length == 0)
                    {
                        state = StateEnum.IDLE;
                        Completed();            
                    }
                }
            }
            else
            {
                Debug.WriteLine("Bad Data Read");
            }

            if (length == 1)
                return (ushort)(TaskFile.data & 0xFF);
            return TaskFile.data;
        }

        public void Write(ushort value, int length)
        {
            var valueToWrite = value;
            if (state != StateEnum.DATA_OUT)
            {
                Debug.WriteLine("Bad data write");
                TaskFile.data = value;
            }
            else
            {
                if (eightbit)
                    valueToWrite &= 0xFF;

                Data[DataPtr++] = (byte)valueToWrite;
                TaskFile.data = (byte)valueToWrite;
                if (!eightbit)
                {
                    Data[DataPtr++] = (byte)(value >> 8);
                    TaskFile.data = (byte)(value >> 8);
                }
                if (DataPtr == 512)
                {
                    if (!WriteSector() )
                    {
                        SetError();
                        return;
                    }
                    this.length--;
                    intrq = true;
                    if (this.length == 0)
                    {
                        state = StateEnum.IDLE;
                        TaskFile.Status |= StatusRegisterEnum.DriveSeekComplete;
                        Completed();
                    }
                }
            }
        }

        private bool WriteSector()
        { 
            DataPtr = 0;
            try
            {
                fd.Write(Data, 0, 512);
            }
            catch (IOException ex)
            {
                Debug.Write(ex.ToString());
                TaskFile.Status |= StatusRegisterEnum.Error;
                TaskFile.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                xlate_errno(-1);
                return false;
            }

            offset += 512;
            
            return true;
        }

        private void SetError()
        {
            TaskFile.lba4 &= nDEVH_HEAD; //d->taskfile.lba4 &= ~DEVH_HEAD;
            if ((TaskFile.lba4 & 0x40) > 0)
            {
                TaskFile.lba1 = (byte)(offset & 0xFF);
                TaskFile.lba2 = (byte)((offset >> 8) & 0xFF);
                TaskFile.lba3 = (byte)((offset >> 16) & 0xFF);
                TaskFile.lba4 |= (byte)((offset >> 24) & DEVH_HEAD);
            }
            else
            {
                TaskFile.lba1 = (byte)(offset % Sectors + 1);
                offset /= Sectors;
                TaskFile.lba4 |= (byte)(offset / (Cylinders * Sectors));
                offset %= (Cylinders * Sectors);
                TaskFile.lba2 = (byte)(offset & 0xFF);
                TaskFile.lba3 = (byte)((offset >> 8) & 0xFF);
            }
            TaskFile.count = (byte)length;
            TaskFile.Status |= StatusRegisterEnum.Error;
            state = StateEnum.IDLE;
            Completed();
        }

        internal void Completed()
        {
            Ready();
            intrq = true;
        }

        internal void DataInState()
        {
            state = StateEnum.DATA_IN;
            DataPtr = 512;  //  d->dptr = d->data + 512;
            /* We don't clear DRDY here, drives may well accept a command at this
               point and at least one firmware for RC2014 assumes this */
            TaskFile.Status &= ~(StatusRegisterEnum.Busy);
            TaskFile.Status |= StatusRegisterEnum.DataRequest;
            intrq = true;			/* Double check */
        }

        internal void DataOutState()
        {
            state = StateEnum.DATA_OUT;
            DataPtr = 0;
            TaskFile.Status &= ~(StatusRegisterEnum.Busy | StatusRegisterEnum.DriveReady);
            TaskFile.Status |= StatusRegisterEnum.DataRequest;
            intrq = true;			/* Double check */
        }

        internal void Failed()
        {
            TaskFile.Status |= StatusRegisterEnum.Error;
            TaskFile.error = ErrorRegisterEnum.IDNF;
            Ready();
        }

        internal int xlate_block()
        {
            IDETaskFile t = TaskFile;
            uint cyl;
            if ((TaskFile.lba4 & DEVH_LBA) > 0)
            {
                if (lba)
                    return 2 + (((int)(TaskFile.lba4 & DEVH_HEAD) << 24) 
                                    | (int)(TaskFile.lba3 << 16) 
                                    | (int)(TaskFile.lba2 << 8) 
                                    | (int)TaskFile.lba1);
                //Fault (LBA on non LBA Drive);
            }
            //Some well known software asks for 0/0/0 when it means 0/0/1. Drives appear
            //    to interpret sector 0 as sector 1 
            if (t.lba1 == 0)
            {
                t.lba1 = 1;
            }
            cyl = (uint)(t.lba3 << 8) | t.lba2;
            if (t.lba1 > Sectors || (t.lba4 & DEVH_HEAD) >= Heads || cyl >= Cylinders )
            {
                return -1;
            }
            // Sector 1 is first 
            // Images generally go cylinder/head/sector. This also matters if we ever
            //   implement more advanced geometry setting 
            return 1 + ((int)(cyl * Heads) + (t.lba4 & DEVH_HEAD)) * Sectors + t.lba1;
        }

        
    }
}
