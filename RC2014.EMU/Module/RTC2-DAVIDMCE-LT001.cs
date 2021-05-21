using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    public class RTC2 : IPort
    {
        public ushort[] HandledPorts => new ushort[] { 0xC0 };

        [DllImport("rtchandler.dll")]
        public static extern dynamic rtc_create(dynamic rtc);

        [DllImport("rtchandler.dll")]
        public static extern void rtc_free(dynamic rtc);

        [DllImport("rtchandler.dll")]
        public static extern void rtc_reset(dynamic rtc);


        [DllImport("rtchandler.dll")]
        public static extern void rtc_write(dynamic rtc, byte val);

        [DllImport("rtchandler.dll")]
        public static extern byte rtc_read(dynamic rtc);

        //void rtc_trace(struct rtc *rtc, int onoff);

        private dynamic rtc = rtc_create();

        public byte GetData(ushort port)
        {
            throw new NotImplementedException();
        }

        public void SetData(ushort port, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
