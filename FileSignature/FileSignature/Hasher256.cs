using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FileSignature
{
    internal class Hasher256
    {
        private SHA256 sha256 = SHA256Managed.Create();
        public byte[] ComputeHash(byte[] data)
        {
            return sha256.ComputeHash(data);
        }
    }
}
