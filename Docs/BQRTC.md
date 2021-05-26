# Module: BQRTC

## Description
This module is a basic interface emulating the [BQ4845](https://www.ti.com/lit/ds/symlink/bq4845.pdf) range of Real Time Clocks.
Writes have not been implemented or rather, writes to set the time are ignored, as are battery control regsiters, and alarm interrupts.

## Ports
This module acts on ports **0x50** to **0x5F**.

## Interrupts
None.

## Further Notes
The BQ4845 provides a provides 16 bytes of clock and control status registers presented through the same interface for clock/calendar and control information as standard SRAM and therefore is really straight forward to emulate.  (From a hardware perspective this means that it is implemented exactly like RAM but watches for the IORQ signal from the CPU rather than the MREQ signal.)

Missing from this emulator is the Alarm Interrupt and the ability to set the clock.  Though the basis is there for implementing it.

The following table (in code for formatting purposes) shows how the date and time information is encoded into the registers.  More information can be found in the [BQ4845 datasheet](https://www.ti.com/lit/ds/symlink/bq4845.pdf).

```csharp
/*
+---+-----+--------------+-------------------+------------------+----------------+
|PORT|  D7 | D6 | D5 | D4 | D3 | D2 | D1 | D0 | RANGE            | REGISTER       |
+----+-----+--------------+-------------------+------------------+----------------+
| 50 |  0  |    10-Second |          1-Second |            00-59 | Seconds        |
+----+-----+----+---------+-------------------+------------------+----------------+
|    | ALM1|ALM2|         |                   |                  |                |
| 51 |     |    10-Second |          1-Second |            00-59 | Seconds Alarm  |
+----+-----+--------------+-------------------+------------------+----------------+
| 52 |  0  |    10-Minute |          1-Minute |            00-59 | Minutes        |
+----+-----+----+---------+-------------------+------------------+----------------+
|    | ALM1|ARM0|         |                   |                  |                |
| 53 |     |    10-Minute |          1-Minute |            00-59 | Minutes Alarm  |
+----+-----+----+---------+-------------------+------------------+----------------+
| 54 |PM/AM|  0 | 10-Hour |            1-Hour |01-12 AM/81-92 PM | Hours          |
+----+-----+----+----+----+-------------------+------------------+----------------+
|   | ALM1|    |         |                   |                  |                |
| 55 |PM/AM|ALM0| 10-Hour |            1-Hour |01-12 AM/81-92 PM | Hours Alarm    |
+----+-----+----+----+----+-------------------+------------------+----------------+
| 56 |  0  |  0 |  10-Day |             1-Day |            01-31 | Day            |
+----+-----+----+----+----+-------------------+------------------+----------------+
| 57 | ALM1|ALM0|  10-day |             1-Day |            01-31 | Day Alarm      |
+----+-----+----+----+----+----+--------------+------------------+----------------+
| 58 |  0  |  0 |  0 |  0 |  0 |  Day Of Week |            01-07 | Day Of Week    |
+----+-----+----+----+----+----+--------------+------------------+----------------+
| 59 |  0  |  0 |  0 |10Mo|           1-Month |            01-12 | Month          |
+----+-----+----+----+----+-------------------+------------------+----------------+
| 5A |            10-Year |            1-Year |            00-99 | Year           |
+----+-----+----+----+----+----+----+----+----+------------------+----------------+
| 5B |  *  | WD2| WD1| WD0| RS3| RS2| RS1| RS0|                  | Rates          |
+----+-----+----+----+----+----+----+----+----+------------------+----------------+
| 5C |  *  |  * |  * |  * | AIE| PIE|PWRE| ABE|                  | Interrupt      |
+----+-----+----+----+----+----+----+----+----+------------------+----------------+
| 5D |  *  |  * |  * |  * | AF | PF |PWRF| BVF|                  | Flags          |
+----+-----+----+----+----+----+----+----+----+------------------+----------------+
| 5E |  *  |  * |  * |  * | UTI|STOP|2412| DSE|                  | Control        |
+----+-----+----+----+----+----+----+----+----+------------------+----------------+
| 5F |  *  |  * |  * |  * |  * |  * |  * |  * |                  | Unused         |
+----+-----+----+----+----+----+----+----+----+------------------+----------------+
*/
```
 
* = Unused bits; unwritable and read as 0.
 0 = should be set to 0 for valid time/calendar range.
 Clock calendar data is BCD. Automatic leap year adjustment.
 PM/AM = 1 for PM; PM/AM = 0 for AM.
 DSE = 1 enable daylight savings adjustment.
 24/12 = 1 enable 24-hour data representation; 24/12 = 0 enables 12-hour data representation.
 Day-Of-Week coded as Sunday = 1 through Saturday = 7.
 BVF = 1 for valid battery.
 STOP = 1 turns the RTC on; STOP = 0 stops the RTC in back-up mode.