﻿using System;
using System.IO;

namespace Infusion.Tests
{
    public class TestMemoryStream : MemoryStream
    {
        public byte[] ActualBytes
        {
            get
            {
                var actualBytes = new byte[Length];

                Array.Copy(GetBuffer(), actualBytes, Length);

                return actualBytes;
            }
        }
    }
}