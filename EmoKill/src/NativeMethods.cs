/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ZiMADE.EmoKill
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern long GetVolumeInformation(
                                    string PathName,
                                    StringBuilder VolumeNameBuffer,
                                    UInt32 VolumeNameSize,
                                    ref UInt32 VolumeSerialNumber,
                                    ref UInt32 MaximumComponentLength,
                                    ref UInt32 FileSystemFlags,
                                    StringBuilder FileSystemNameBuffer,
                                    UInt32 FileSystemNameSize);
    }
}
