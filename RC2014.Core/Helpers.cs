using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core
{
    public static class Helpers
    {
        public static byte[] ToByteArray(this ushort[] shorts)
        {
            byte[] bytes = new byte[shorts.Length * 2];

            for (int i = 0; i < shorts.Length; i++)
            {
                bytes[i*2] = (byte)((shorts[i] & 0xFF00) >> 8);
                bytes[i*2 + 1] = (byte)(shorts[i] & 0xFF);
            }
            return bytes;
        }

        internal static ushort LittleEndian(this ushort v)
        {
            byte lo = (byte)(v & 0xFF);
            byte hi = (byte)(v >> 8);

            return (ushort)(hi + (lo << 8));
        }

        internal static ushort[] MakeASCII(this ushort[] shorts, int start, string text, int length)
        {
            var textBytes = Encoding.ASCII.GetBytes(text);
            var shortArray = textBytes.ToShortArray();
            Array.Copy(shortArray, 0, shorts, start, Math.Min(length, shortArray.Length));

            if (shortArray.Length < length)
                for (int i = shortArray.Length; i < length; i++)
                {
                    shorts[i] = 0;
                }

            return shorts;
        }

        internal static ushort[] ToShortArray(this byte[] bytes)
        {
            var len = bytes.Length;
            if (bytes.Length % 2 != 0)
                len++;

            byte[] paddedBytes = new byte[len];
            Array.Copy(bytes, 0, paddedBytes, 0, bytes.Length);

            ushort[] shorts = new ushort[len / 2];

            for (int i = 0; i < shorts.Length; i++)
            {
                shorts[i] = (ushort)((paddedBytes[i*2] << 8) + paddedBytes[i*2+1]);
            }
            return shorts;
        }

    }
}
