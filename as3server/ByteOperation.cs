using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace as3server
{
    class ByteOperation
    {
        public static byte[] RSwapBytes(byte[] data, int offset = 0, int length = 0)
        {
            byte[] temp = new byte[data.Length];
            Buffer.BlockCopy(data, 0, temp, 0, data.Length);
            SwapBytes(temp, offset, length);
            return temp;
        }

        public static void SwapBytes(byte[] data, int offset = 0, int length = 0)
        {
            bool can_swap = true;
            if (length == 0)
            {
                if (data.Length == 0)
                {
                    can_swap = false;
                    Console.WriteLine("Error swaping bytes");
                }
                else
                {
                    length = data.Length;
                }
            }
            if (can_swap)
            {
                byte[] temp = new byte[length];
                Buffer.BlockCopy(data, offset, temp, 0, length);
                for (int i = 0; i < length; i++)
                {
                    Buffer.BlockCopy(temp, i, data, offset + (length - 1 - i), 1);
                }
                temp = null;
            }
        }
    }
}
