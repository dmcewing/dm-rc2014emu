using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    /*
;  +---+-----+--------------+-------------------+------------------+----------------+
;  |ADR|  D7 | D6 | D5 | D4 | D3 | D2 | D1 | D0 | RANGE            | REGISTER       |
;  +---+-----+--------------+-------------------+------------------+----------------+
;  | 0 |  0  |    10-Second |          1-Second |            00-59 | Seconds        |
;  +---+-----+----+---------+-------------------+------------------+----------------+
;  |   | ALM1|ALM2|         |                   |                  |                |
;  | 1 |     |    10-Second |          1-Second |            00-59 | Seconds Alarm  |
;  +---+-----+--------------+-------------------+------------------+----------------+
;  | 2 |  0  |    10-Minute |          1-Minute |            00-59 | Minutes        |
;  +---+-----+----+---------+-------------------+------------------+----------------+
;  |   | ALM1|ARM0|         |                   |                  |                |
;  | 3 |     |    10-Minute |          1-Minute |            00-59 | Minutes Alarm  |
;  +---+-----+----+---------+-------------------+------------------+----------------+
;  | 4 |PM/AM|  0 | 10-Hour |            1-Hour |01-12 AM/81-92 PM | Hours          |
;  +---+-----+----+----+----+-------------------+------------------+----------------+
;  |   | ALM1|    |         |                   |                  |                |
;  | 5 |PM/AM|ALM0| 10-Hour |            1-Hour |01-12 AM/81-92 PM | Hours Alarm    |
;  +---+-----+----+----+----+-------------------+------------------+----------------+
;  | 6 |  0  |  0 |  10-Day |             1-Day |            01-31 | Day            |
;  +---+-----+----+----+----+-------------------+------------------+----------------+
;  | 7 | ALM1|ALM0|  10-day |             1-Day |            01-31 | Day Alarm      |
;  +---+-----+----+----+----+----+--------------+------------------+----------------+
;  | 8 |  0  |  0 |  0 |  0 |  0 |  Day Of Week |            01-07 | Day Of Week    |
;  +---+-----+----+----+----+----+--------------+------------------+----------------+
;  | 9 |  0  |  0 |  0 |10Mo|           1-Month |            01-12 | Month          |
;  +---+-----+----+----+----+-------------------+------------------+----------------+
;  | A |            10-Year |            1-Year |            00-99 | Year           |
;  +---+-----+----+----+----+----+----+----+----+------------------+----------------+
;  | B |  *  | WD2| WD1| WD0| RS3| RS2| RS1| RS0|                  | Rates          |
;  +---+-----+----+----+----+----+----+----+----+------------------+----------------+
;  | C |  *  |  * |  * |  * | AIE| PIE|PWRE| ABE|                  | Interrupt      |
;  +---+-----+----+----+----+----+----+----+----+------------------+----------------+
;  | D |  *  |  * |  * |  * | AF | PF |PWRF| BVF|                  | Flags          |
;  +---+-----+----+----+----+----+----+----+----+------------------+----------------+
;  | E |  *  |  * |  * |  * | UTI|STOP|2412| DSE|                  | Control        |
;  +---+-----+----+----+----+----+----+----+----+------------------+----------------+
;  | F |  *  |  * |  * |  * |  * |  * |  * |  * |                  | Unused         |
;  +---+-----+----+----+----+----+----+----+----+------------------+----------------+

;  * = Unused bits; unwritable and read as 0.
;  0 = should be set to 0 for valid time/calendar range.
;  Clock calendar data is BCD. Automatic leap year adjustment.
;  PM/AM = 1 for PM; PM/AM = 0 for AM.
;  DSE = 1 enable daylight savings adjustment.
;  24/12 = 1 enable 24-hour data representation; 24/12 = 0 enables 12-hour data representation.
;  Day-Of-Week coded as Sunday = 1 through Saturday = 7.
;  BVF = 1 for valid battery.
;  STOP = 1 turns the RTC on; STOP = 0 stops the RTC in back-up mode.
*/
    public class BQRTC : IPort
    {
        protected internal readonly byte[] Registers = new byte[16];

        protected internal int _CurrentRegister = 0;
        protected internal DateTime _LockedDate;

        protected internal bool Stop => (Registers[0xE] & 4) > 0;
        protected internal bool HR24 => (Registers[0xE] & 2) > 0;

        public ushort[] HandledPorts => new ushort[] { 0x50,0x51,0x52,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5A,0x5B,0x5C,0x5D,0x5E,0x5F };

        public byte GetData(ushort port)
        {
            Debug.WriteLine("Get:{0:X2}", port);
            incTime();
            int register = port - HandledPorts[0];

            if (register <= 10 && ((register % 2)==0 || register ==9))
            {
                switch (register)
                {
                    case 0: //Seconds
                        var s = _LockedDate.Second;
                        var val = (s % 10) + ((s / 10) << 4);
                        return (byte)val;          
                    case 2: //Minutes
                        var m = _LockedDate.Minute;
                        return (byte)((m % 10)  + ((m / 10) << 4));
                    case 4: //Hours
                        var hr = _LockedDate.Hour;
                        if (!HR24 && hr > 12)
                        {
                            hr = (hr - 12);
                            return (byte)(0x80 + (hr % 10) + ((hr / 10) << 4));
                        }
                        return (byte)((hr % 10) + ((hr / 10) << 4));
                    case 6: //Day
                        var day = _LockedDate.Day;
                        return (byte)((day % 10) + ((day / 10) << 4));
                    case 8: // Day ofWeek.
                        return (byte)_LockedDate.DayOfWeek;
                    case 9: // Month
                        var month = _LockedDate.Month;
                        return (byte)((month % 10) + ((month / 10) << 4));
                    case 10:  //Year
                        var year = _LockedDate.Year - (_LockedDate.Year / 100)* 100;
                        return (byte)((year % 10)  + ((year / 10) << 4));                
                }
            }

            return Registers[port - HandledPorts[0]];
        }

        public void SetData(ushort port, byte value)
        {
            incTime();
            Registers[port - HandledPorts[0]] = value;
        }

        private void incTime()
        {
            if (!Stop)
                _LockedDate = DateTime.Now;
        }
    }
}
