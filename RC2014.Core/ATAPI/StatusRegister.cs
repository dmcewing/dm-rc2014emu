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
	public struct StatusRegister
	{
		public StatusRegister()
		{
		}

		public StatusRegister(byte value)
		{
			Busy = (value & 0x80) != 0;
			DriveReady = (value & 0x40) != 0;
			DriveWriteFault = (value & 0x20) != 0;
			DriveSeekComplete = (value & 0x10) != 0;
			DataRequest = (value & 0x08) != 0;
			CorrectedData = (value & 0x04) != 0;
			Index = (value & 0x02) != 0;
			Error = (value & 0x01) != 0;
		}

		public byte Value
		{
			get
			{
				var status = (Busy ? 0x80 : 0)
						+ (DriveReady ? 0x40 : 0)
						+ (DriveWriteFault ? 0x20 : 0)
						+ (DriveSeekComplete ? 0x10 : 0)
						+ (DataRequest ? 0x08 : 0)
						+ (CorrectedData ? 0x04 : 0)
						+ (Index ? 0x02 : 0)
						+ (Error ? 0x01 : 0);
				return (byte)status;
			}
		}
		public bool Busy { get; set; } = false;
		public bool DriveReady { get; set; } = false;
		public bool DriveWriteFault { get; set; } = false;
		public bool DriveSeekComplete { get; set; } = false;
		public bool DataRequest { get; set; } = false;
		public bool CorrectedData { get; set; } = false;
		public bool Index { get; set; } = false;
		public bool Error { get; set; } = false;
	}

}
