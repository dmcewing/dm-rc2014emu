using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
    public enum IDECommands
    {
        UNKNOWN = 0x00,
        CMD_EDD = 0x90,
        CMD_IDENTIFY = 0xEC,
        CMD_INTPARAMS = 0x91,
        CMD_READ = 0x20,
        CMD_READ_NR = 0x21,
        CMD_SETFEATURES = 0xEF,
        CMD_VERIFY = 0x40,
        CMD_VERIFY_NR = 0x41,
        CMD_WRITE = 0x30,
        CMD_WRITE_NR = 0x31,
        CMD_CALIB = 0x10,  //actually all 0x1? range
        CMD_SEEK = 0x70,   //actually all 0x7? range
    }

    public static class IDECommandExtensions
    {
        public static IDECommands GetCommand(this byte value)
        {
            var command = Enum.GetValues<IDECommands>()
                            .FirstOrDefault(t => (int)t == value, IDECommands.UNKNOWN);
            return command;
        }

        public static void EDDComplete(this ATAPIBase c)
        {
            //There are only two drives, so faster to index than foreach/while.
            if (c.Drive[0].present)
                c.Drive[0].Setup();
            if (c.Drive[1].present)
                c.Drive[1].Setup();
            c.SelectedDrive = 0;
        }

        public static void Identify(this IDEDrive d)
        {
            Array.Copy(d.identify.ToByteArray(), d.Data, 512);
            d.DataInState();
            // Arrange to copy just the identify buffer 
            d.DataPtr = 0;
            d.length = 1;
        }

        public static void InitParam(this IDEDrive d)
        {
            // We only support the current mapping 
            IDETaskFile tf = d.TaskFile;
            if (tf.count != d.Sectors || (tf.lba4 & IDEDrive.DEVH_HEAD) + 1 != d.Heads)
            {
                tf.Status |= StatusRegisterEnum.Error;
                tf.error |= ErrorRegisterEnum.ABRT;
                d.failed = true;
                Debug.WriteLine($"geo is {d.Sectors} {d.Heads}, asked for {tf.count}, {(tf.lba4 & IDEDrive.DEVH_HEAD) + 1}");
            }
            else if (d.failed)
            {
                d.failed = false; //valid translation
            }
            d.Completed();
        }

        public static void ReadSectors(this IDEDrive d)
        {
            IDETaskFile tf = d.TaskFile;
            //Move to data xfer
            if (d.failed)
            {
                d.Failed();
                return;
            }
            d.offset = d.xlate_block();
            /* DRDY is not guaranteed here but at least one buggy RC2014 firmware
               expects it */
            tf.Status |= StatusRegisterEnum.DataRequest | StatusRegisterEnum.DriveSeekComplete | StatusRegisterEnum.DriveReady;
            tf.Status &= ~StatusRegisterEnum.Busy;
            /* 0 = 256 sectors */
            d.length = (tf.count == 0) ? 256 : tf.count;
            if (d.offset == -1 || d.fd.Seek(512 * d.offset, SeekOrigin.Begin) == -1)
            {
                tf.Status = StatusRegisterEnum.Error;
                tf.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                tf.error |= ErrorRegisterEnum.IDNF;
                d.Completed();
                return;
            }
            //do the xfer
            d.DataInState();
        }

        public static void SetFeatures(this IDEDrive d)
        {
            IDETaskFile tf = d.TaskFile;
            switch (tf.feature)
            {
                case 0x01:
                    d.eightbit = true;
                    break;
                case 0x03:
                    if ((tf.count & 0xF0) >= 0x20)
                    {
                        tf.Status |= StatusRegisterEnum.Error;
                        tf.error |= ErrorRegisterEnum.ABRT;
                    }
                    /* Silently accept PIO mode settings */
                    break;

                case 0x81:
                    d.eightbit = false;
                    break;

                default:
                    tf.Status |= StatusRegisterEnum.Error;
                    tf.error |= ErrorRegisterEnum.ABRT;
                    break;
            }
            d.Completed();

        }

        public static void VerifySectors(this IDEDrive d)
        {
            IDETaskFile tf = d.TaskFile;
            // Move to data xfer
            if (d.failed)
            {
                d.Failed();
                return;
            }
            d.offset = d.xlate_block();
            /* 0 = 256 sectors */
            d.length = (tf.count > 0) ? tf.count : 256;
            if (d.offset == -1 || d.fd.Seek(512 * (d.offset + d.length - 1), SeekOrigin.Begin) == -1)
            {
                tf.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                tf.Status |= StatusRegisterEnum.Error;
                tf.error |= ErrorRegisterEnum.IDNF;
            }
            tf.Status |= StatusRegisterEnum.DriveSeekComplete;
            d.Completed();
        }

        public static void WriteSectors(this IDEDrive d)
        {
            IDETaskFile tf = d.TaskFile;
            /* Move to data xfer */
            if (d.failed) {
                d.Failed();
                return;
            }
            d.offset = d.xlate_block();
            tf.Status |= StatusRegisterEnum.DataRequest;
            // 0 = 256 sectors
            d.length = (tf.count == 0) ? 256 : tf.count;
            Debug.WriteLine("WRITE {0} SECTORS @ {1}", d.length, d.offset); 
            if (d.offset == -1 || d.fd.Seek(512*d.offset, SeekOrigin.Begin) == -1)
            {
                tf.Status |= StatusRegisterEnum.Error;
                tf.error |= ErrorRegisterEnum.IDNF;
                tf.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                //return null data
                d.Completed();
                return;
            }
            //Do the xfer
            d.DataOutState();
        }

        public static void Recalibrate(this IDEDrive d)
        {
            IDETaskFile tf = d.TaskFile;
            if (d.failed)
                d.Failed();
            if (d.offset == -1 || d.xlate_block() != 0L)
            {
                tf.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                tf.Status |= StatusRegisterEnum.Error;
                tf.error |= ErrorRegisterEnum.ABRT;
            }
            tf.Status |= StatusRegisterEnum.DriveSeekComplete;
            d.Completed();
        }

        public static void SeekComplete(this IDEDrive d)
        {
            IDETaskFile tf = d.TaskFile;
            if (d.failed)
                d.Failed();
            d.offset = d.xlate_block();
            if (d.offset == -1 || d.fd.Seek(512*d.offset, SeekOrigin.Begin)==-1)
            {
                tf.Status &= ~StatusRegisterEnum.DriveSeekComplete;
                tf.Status |= StatusRegisterEnum.Error;
                tf.error |= ErrorRegisterEnum.IDNF;
            }
            tf.Status |= StatusRegisterEnum.DriveSeekComplete;
            d.Completed();
        }
    }
}
