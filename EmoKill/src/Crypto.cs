/*
 *  IMPORTANT NOTICE
 *      Copyright (C) 2020 ZiMADE. All Rights Reserved.
 *
 *  LICENSE
 *      Please refer to the LICENSE.txt of https://github.com/ZiMADE/EmoKill/
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace ZiMADE.EmoKill
{
    internal static class Crypto
    {
        internal static string MD5Hash(string value)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }
    }
}
