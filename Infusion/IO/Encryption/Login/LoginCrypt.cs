﻿using System;
using System.IO;

namespace Infusion.IO.Encryption.Login
{

    internal sealed class LoginCrypt
    {
        private readonly uint[] key;
        private readonly uint key1;
        private readonly uint key2;
        private readonly uint key3;

        public LoginCrypt(uint seed, LoginEncryptionKey loginKey)
        {
            key = new uint[2];

            key[0] = ((~seed ^ 0x00001357) << 16) | ((seed ^ 0xffffaaaa) & 0x0000ffff);
            key[1] = ((seed ^ 0x43210000) >> 16) | ((~seed ^ 0xabcdffff) & 0xffff0000);

            key1 = loginKey.Key1;
            key2 = loginKey.Key2;
            key3 = loginKey.Key3;
        }

        public void Encrypt(byte[] input, byte[] output, long len)
        {
            for (var i = 0; i < len; i++)
            {
                output[i] = (byte)(input[i] ^ (byte)key[0]);

                var table0 = key[0];
                var table1 = key[1];

                key[1] = (((((table1 >> 1) | (table0 << 31)) ^ key1) >> 1)
                            | (table0 << 31)) ^ key2;
                key[0] = ((table0 >> 1) | (table1 << 31)) ^ key3;
            }
        }

        public void Decrypt(byte[] input, byte[] output, long len)
            => Encrypt(input, output, len);
    }
}