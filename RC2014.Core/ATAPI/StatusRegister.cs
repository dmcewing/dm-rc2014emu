namespace RC2014.Core.ATAPI
{
	/// <summary>
	/// Struct to support the status register
	/// </summary>
	/// <remarks>
	///	+-------+-------+-------+-------+-------+-------+-------+-------+
	///	|  BSY	| DRDY	|  DWF	|  DSC	|  DRQ	| CORR	|  IDX	|  ERR	|
	///	+-------+-------+-------+-------+-------+-------+-------+-------+
	///	7 BSY:	BUSY
	///	6 DRDY:	DRIVE READY
	///	5 DWF:	DRIVE WRITE FAULT
	///	4 DSC:	DRIVE SEEK COMPLETE
	///	3 DRQ:	DATA REQUEST
	///	2 CORR:	CORRECTED DATA
	///	1 IDX:	INDEX
	///	0 ERR:	ERROR
	/// </remarks>
	[Flags]
	public enum StatusRegisterEnum
	{
		Error = 0x01,
		Index = 0x02,
		CorrectedData = 0x04,
		DataRequest = 0x08,
		DriveSeekComplete = 0x10,
		DriveWriteFault = 0x20,
		DriveReady = 0x40,
		Busy = 0x80
	}

}
