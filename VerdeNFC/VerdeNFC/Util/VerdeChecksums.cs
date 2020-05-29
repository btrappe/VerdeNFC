using System;
using System.Linq;

namespace VerdeNFC.Util
{
    public class VerdeChecksums
    {
        public static UInt16 crc16_singleuse(byte[] param)
        {
            byte[] lookup_table = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x10, 0x12, 0x13, 0x16, 0x17,
                                      0x19, 0x1b, 0x1c, 0x1f, 0x20, 0x22, 0x25, 0x27, 0x28, 0x29, 0x2a, 0x2d};

            UInt16 checksum = Convert.ToUInt16((param[1] * param[2] * 5) & 0xffff);

            foreach (byte b in lookup_table.Skip(2).ToArray())
                checksum ^= param[b];

            return Convert.ToUInt16(checksum ^ 0xa1);
        }
        public static UInt16 crc16_multiuse(byte[] param)
        {
            byte[] lookup_table = { 0x00, 0x02, 0x03, 0x04, 0x06, 0x10, 0x11, 0x12, 0x14, 0x17, 0x18, 0x19,
                                      0x1b, 0x1e, 0x1f, 0x20, 0x21, 0x23, 0x24, 0x26, 0x28, 0x2a, 0x2b, 0x2c, 0x2d};

            UInt16 checksum = Convert.ToUInt16((param[0] * param[2] * 5) & 0xffff);

            foreach (byte b in lookup_table.Skip(2).ToArray())
                checksum ^= param[b];

            return Convert.ToUInt16(checksum ^ 0xd2);
        }
    }

    public static class Crc8
    {
        static byte[] table = new byte[256];
        // x8 + x7 + x6 + x4 + x2 + 1
        const byte poly = 0x07;

        public static byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = table[crc ^ b];
                }
            }
            return crc;
        }

        static Crc8()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                table[i] = (byte)temp;
            }
        }
    }
}
