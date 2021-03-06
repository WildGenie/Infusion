﻿using System;
using System.Collections.Generic;

namespace Infusion.IO.Encryption.Login
{
    internal sealed class LoginEncryptionDetector
    {
        private static readonly Version[] supportedVersions =
        {
            new Version(6,0,14),
            new Version(6,0,13),
            new Version(6,0,12),
            new Version(6,0,11),
            new Version(6,0,10),
            new Version(6,0,9),
            new Version(6,0,8),
            new Version(6,0,7),
            new Version(6,0,6),
            new Version(6,0,5),
            new Version(6,0,4),
            new Version(6,0,3),
            new Version(6,0,2),
            new Version(6,0,1),
            new Version(6,0,0),
            new Version(5,0,9),
            new Version(5,0,8),
            new Version(5,0,7),
            new Version(5,0,6),
            new Version(5,0,5),
            new Version(5,0,4),
            new Version(5,0,3),
            new Version(5,0,2),
            new Version(5,0,1),
            new Version(5,0,0),
            new Version(4,0,11),
            new Version(4,0,10),
            new Version(4,0,9),
            new Version(4,0,8),
            new Version(4,0,7),
            new Version(4,0,6),
            new Version(4,0,5),
            new Version(4,0,4),
            new Version(4,0,3),
            new Version(4,0,2),
            new Version(4,0,1),
            new Version(4,0,0),
            new Version(3,0,8),
            new Version(3,0,7),
            new Version(3,0,6),
            new Version(3,0,5),
            new Version(3,0,4),
            new Version(3,0,3),
            new Version(3,0,2),
            new Version(3,0,1),
            new Version(3,0,0),
            new Version(2,0,9),
            new Version(2,0,8),
            new Version(2,0,7),
            new Version(2,0,6),
            new Version(2,0,5),
            new Version(2,0,4),
            new Version(2,0,3),
            new Version(2,0,2),
            new Version(2,0,1),
            new Version(2,0,0),
        };

        private readonly byte[] rawBuffer = new byte[62];
        private readonly byte[] decryptedBuffer = new byte[62];

        public LoginEncryptionDetectionResult Detect(uint seed, IPullStream inputStream, Version defaultVersion)
        {
            var reader = new StreamPacketReader(new PullStreamToStreamAdapter(inputStream), rawBuffer);
            var length = 0;

            while (inputStream.DataAvailable && length < 62)
                rawBuffer[length++] = reader.ReadByte();

            rawBuffer.CopyTo(decryptedBuffer, 0);

            var versionsToTest = GetVersionsToTest(defaultVersion).GetEnumerator();

            LoginEncryptionKey? currentKey = null;
            LoginCrypt loginCrypt = null;
            while (!IsGameServerPacket(decryptedBuffer))
            {
                if (!versionsToTest.MoveNext())
                    break;
                var version = versionsToTest.Current;

                currentKey = LoginEncryptionKey.FromVersion(version);
                loginCrypt = new LoginCrypt(seed, currentKey.Value);
                loginCrypt.Decrypt(rawBuffer, decryptedBuffer, 62);
            }

            if (currentKey.HasValue)
                return new LoginEncryptionDetectionResult(decryptedBuffer, loginCrypt, currentKey.Value);
            else
                return new LoginEncryptionDetectionResult(decryptedBuffer);
        }

        private IEnumerable<Version> GetVersionsToTest(Version defaultVersion)
        {
            if (defaultVersion != null)
                yield return defaultVersion;

            foreach (var version in supportedVersions)
            {
                yield return version;
            }
        }

        private bool IsGameServerPacket(byte[] encryptedBuffer)
        {
            if (encryptedBuffer[0] != 0x80 || encryptedBuffer[30] != 0x00 || encryptedBuffer[60] != 0x00)
                return false;

            for (int toCheck = 21; toCheck <= 30; toCheck++)
            {
                if (encryptedBuffer[toCheck] != 0x00 || encryptedBuffer[toCheck + 30] != 0x00)
                    return false;
            }

            return true;
        }

    }
}
