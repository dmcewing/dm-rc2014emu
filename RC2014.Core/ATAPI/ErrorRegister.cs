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
	[Flags]
    public enum ErrorRegisterEnum
    {
		CLEAR = 0,
		/// <summary>
		/// Address Mark Not FOund
		/// </summary>
		AMNF = 1,
		/// <summary>
		/// Track 0 Not Found
		/// </summary>
		TKNONF = 2,
		/// <summary>
		/// Aborted Command
		/// </summary>
		ABRT = 4,
		/// <summary>
		/// Media Change Requested
		/// </summary>
		MCR = 8,
		/// <summary>
		/// ID Not Found
		/// </summary>
		IDNF = 16,
		/// <summary>
		/// Media Changed
		/// </summary>
		MC = 32,
		/// <summary>
		/// Uncorrectable Data Error
		/// </summary>
		UNC = 64,
		/// <summary>
		/// Bad Block Detected
		/// </summary>
		BBK = 128
    }
}
