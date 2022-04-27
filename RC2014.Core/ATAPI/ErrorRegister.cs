using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
	/// <summary>
	/// Struct to support the error register
	/// </summary>
	/// <remarks>
	/// +-------+-------+-------+-------+-------+-------+-------+-------+
	///	| BBK	|  UNC	|  MC	|  IDNF |  MCR	| ABRT	| TK0NF |  AMNF |
	///	+-------+-------+-------+-------+-------+-------+-------+-------+
	///	(VALID WHEN ERR BIT IS SET IN STATUS REGISTER)
	///	
	///	7	BBK:	BAD BLOCK DETECTED
	///	6	UNC:	UNCORRECTABLE DATA ERROR
	///	5	MC:		MEDIA CHANGED
	///	4	IDNF:	ID NOT FOUND
	///	3	MCR:	MEDIA CHANGE REQUESTED
	///	2	ABRT:	ABORTED COMMAND
	///	1	TK0NF:	TRACK 0 NOT FOUND
	///	0	AMNF:	ADDRESS MARK NOT FOUND
	/// </remarks>
	public struct ErrorRegister
	{
		public ErrorRegister()
		{

		}
		public ErrorRegister(byte value)
		{
			BadBlock = (value & 0x80) != 0;
			UncorrectableData = (value & 0x40) != 0;
			MediaChanged = (value & 0x20) != 0;
			IDNotFound = (value & 0x10) != 0;
			MediaChangeRequested = (value & 0x08) != 0;
			AbortedCommand = (value & 0x04) != 0;
			Track0NotFound = (value & 0x02) != 0;
			AddressMarkNotFound = (value & 0x01) != 0;
		}

		public byte Value
		{
			get
			{
				var status = (BadBlock ? 0x80 : 0)
						+ (UncorrectableData ? 0x40 : 0)
						+ (MediaChanged ? 0x20 : 0)
						+ (IDNotFound ? 0x10 : 0)
						+ (MediaChangeRequested ? 0x08 : 0)
						+ (AbortedCommand ? 0x04 : 0)
						+ (Track0NotFound ? 0x02 : 0)
						+ (AddressMarkNotFound ? 0x01 : 0);
				return (byte)status;
			}
		}

		public bool BadBlock { get; set; } = false;
		public bool UncorrectableData { get; set; } = false;
		public bool MediaChanged { get; set; } = false;
		public bool IDNotFound { get; set; } = false;
		public bool MediaChangeRequested { get; set; } = false;
		public bool AbortedCommand { get; set; } = false;
		public bool Track0NotFound { get; set; } = false;
		public bool AddressMarkNotFound { get; set; } = false;

	}
}
