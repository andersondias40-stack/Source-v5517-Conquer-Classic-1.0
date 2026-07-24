using System;

namespace Project_Terror_v2.Cryptography
{
    public unsafe interface ICrypto
    {
        void Encrypt(Byte* pBup, byte[] pOut, Int32 Length);
        void Decrypt(byte[] In, int start, byte* buff_out, int Size);
        void GenerateKey(byte[] BufKey);
        void SetIVs(Byte[] EncryptIV, Byte[] DecryptIV);
    }
}
