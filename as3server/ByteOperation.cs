/*
 * This file is part of the as3server Project. http://www.as3server.org
 *
 * Copyright (c) 2010 individual as3server contributors. See the CONTRIB file
 * for details.
 *
 * This code is licensed to you under the terms of the Apache License, version
 * 2.0, or, at your option, the terms of the GNU General Public License,
 * version 2.0. See the APACHE20 and GPL2 files for the text of the licenses,
 * or the following URLs:
 * http://www.apache.org/licenses/LICENSE-2.0
 * http://www.gnu.org/licenses/gpl-2.0.txt
 *
 * If you redistribute this file in source form, modified or unmodified, you
 * may:
 *   1) Leave this header intact and distribute it under the same terms,
 *      accompanying it with the APACHE20 and GPL20 files, or
 *   2) Delete the Apache 2.0 clause and accompany it with the GPL2 file, or
 *   3) Delete the GPL v2 clause and accompany it with the APACHE20 file
 * In all cases you must keep the copyright notice intact and include a copy
 * of the CONTRIB file.
 *
 * Binary distributions must follow the binary distribution requirements of
 * either License.
 */

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
