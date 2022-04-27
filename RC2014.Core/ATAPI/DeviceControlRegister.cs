using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
	/// <summary>
	/// Struct to support the Device Control Register.
	/// </summary>
	/// <remarks>
	/// +-------+-------+-------+-------+-------+-------+-------+-------+
	/// |   X	|   X	|   X	|   X	|   1	| SRST	|  ~IEN |   0	|
	/// +-------+-------+-------+-------+-------+-------+-------+-------+
	///    7        6       5       4       3       2       1       0 
	/// SRST:	SOFTWARE RESET
	/// ~IEN:	INTERRUPT ENABLE
	/// </remarks>
	public struct DeviceControlRegister
	{
		public DeviceControlRegister()
		{
		}

		public DeviceControlRegister(byte value)
		{
			SoftwareReset = (value & 0x04) != 0;
			InterruptEnable = (value & 0x02) != 0;
		}

		public byte Value
		{
			get
			{
				var status = 0x8
						+ (SoftwareReset ? 0x04 : 0)
						+ (InterruptEnable ? 0x02 : 0);
				return (byte)status;
			}
		}

		public bool SoftwareReset { get; set; } = false;
		public bool InterruptEnable { get; set; } = false;
	}
}
