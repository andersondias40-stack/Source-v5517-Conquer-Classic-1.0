namespace AccServer
{
    using System;
    using System.IO;
    using System.Text;
    public class PasswordCryptography
    {
        static UInt32 LeftRotate(UInt32 var, UInt32 offset)
        {
            UInt32 tmp1, tmp2;
            offset &= 0x1f;
            tmp1 = var >> (int)(32 - offset);
            tmp2 = var << (int)offset;
            tmp2 |= tmp1;
            return tmp2;
        }
        static UInt32 RightRotate(UInt32 var, UInt32 offset)
        {
            UInt32 tmp1, tmp2;
            offset &= 0x1f;
            tmp1 = var << (int)(32 - offset);
            tmp2 = var >> (int)offset;
            tmp2 |= tmp1;
            return tmp2;
        }

        static uint[] key = new uint[] {
                0xEBE854BC, 0xB04998F7, 0xFFFAA88C,
                0x96E854BB, 0xA9915556, 0x48E44110,
                0x9F32308F, 0x27F41D3E, 0xCF4F3523,
                0xEAC3C6B4, 0xE9EA5E03, 0xE5974BBA,
                0x334D7692, 0x2C6BCF2E, 0xDC53B74,
                0x995C92A6, 0x7E4F6D77, 0x1EB2B79F,
                0x1D348D89, 0xED641354, 0x15E04A9D,
                0x488DA159, 0x647817D3, 0x8CA0BC20,
                0x9264F7FE, 0x91E78C6C, 0x5C9A07FB,
                0xABD4DCCE, 0x6416F98D, 0x6642AB5B
        };
        public static string EncryptPassword(string password)
        {
            UInt32 tmp1, tmp2, tmp3, tmp4, A, B, chiperOffset, chiperContent;

            byte[] plain = new byte[16];
            Encoding.ASCII.GetBytes(password, 0, password.Length, plain, 0);

            MemoryStream mStream = new MemoryStream(plain);
            BinaryReader bReader = new BinaryReader(mStream);
            UInt32[] pSeeds = new UInt32[4];
            for (int i = 0; i < 4; i++) pSeeds[i] = bReader.ReadUInt32();
            bReader.Close();

            chiperOffset = 7;

            byte[] encrypted = new byte[plain.Length];
            MemoryStream eStream = new MemoryStream(encrypted);
            BinaryWriter bWriter = new BinaryWriter(eStream);

            for (int j = 0; j < 2; j++)
            {
                tmp1 = tmp2 = tmp3 = tmp4 = 0;
                tmp1 = key[5];
                tmp2 = pSeeds[j * 2];
                tmp3 = key[4];
                tmp4 = pSeeds[j * 2 + 1];

                tmp2 += tmp3;
                tmp1 += tmp4;

                A = B = 0;

                for (int i = 0; i < 12; i++)
                {
                    chiperContent = 0;
                    A = LeftRotate(tmp1 ^ tmp2, tmp1);
                    chiperContent = key[chiperOffset + i * 2 - 1];
                    tmp2 = A + chiperContent;

                    B = LeftRotate(tmp1 ^ tmp2, tmp2);
                    chiperContent = key[chiperOffset + i * 2];
                    tmp1 = B + chiperContent;
                }

                bWriter.Write(tmp2);
                bWriter.Write(tmp1);
            }
            bWriter.Close();

            return ASCIIEncoding.ASCII.GetString(encrypted);

        }
    }

    public sealed class RC5
    {
        private readonly uint[] bufKey = new uint[4];
        private readonly uint[] bufSub = new uint[0x1a];

        public RC5(byte[] data)
        {
            if (data.Length != 0x10)
            {
                throw new RC5Exception("Invalid data length. Must be 16 bytes");
            }
            uint index = 0;
            uint num2 = 0;
            uint num3 = 0;
            uint num4 = 0;
            for (int i = 0; i < 4; i++)
            {
                this.bufKey[i] = (uint)(((data[i * 4] + (data[(i * 4) + 1] << 8)) + (data[(i * 4) + 2] << 0x10)) + (data[(i * 4) + 3] << 0x18));
            }
            this.bufSub[0] = 0xb7e15163;
            for (int j = 1; j < 0x1a; j++)
            {
                this.bufSub[j] = this.bufSub[j - 1] - 0x61c88647;
            }
            for (int k = 1; k <= 0x4e; k++)
            {
                this.bufSub[index] = LeftRotate((this.bufSub[index] + num3) + num4, 3);
                num3 = this.bufSub[index];
                index = (index + 1) % 0x1a;
                this.bufKey[num2] = LeftRotate((this.bufKey[num2] + num3) + num4, (int)(num3 + num4));
                num4 = this.bufKey[num2];
                num2 = (num2 + 1) % 4;
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            if ((data.Length % 8) != 0)
            {
                throw new RC5Exception("Invalid password length. Must be multiple of 8");
            }
            int num = (data.Length / 8) * 8;
            if (num <= 0)
            {
                throw new RC5Exception("Invalid password length. Must be greater than 0 bytes.");
            }
            uint[] numArray = new uint[data.Length / 4];
            for (int i = 0; i < (data.Length / 4); i++)
            {
                numArray[i] = (uint)(((data[i * 4] + (data[(i * 4) + 1] << 8)) + (data[(i * 4) + 2] << 0x10)) + (data[(i * 4) + 3] << 0x18));
            }
            for (int j = 0; j < (num / 8); j++)
            {
                uint num4 = numArray[2 * j];
                uint num5 = numArray[(2 * j) + 1];
                for (int m = 12; m >= 1; m--)
                {
                    num5 = RightRotate(num5 - this.bufSub[(2 * m) + 1], (int)num4) ^ num4;
                    num4 = RightRotate(num4 - this.bufSub[2 * m], (int)num5) ^ num5;
                }
                uint num7 = num5 - this.bufSub[1];
                uint num8 = num4 - this.bufSub[0];
                numArray[2 * j] = num8;
                numArray[(2 * j) + 1] = num7;
            }
            byte[] buffer = new byte[numArray.Length * 4];
            for (int k = 0; k < numArray.Length; k++)
            {
                buffer[k * 4] = (byte)numArray[k];
                buffer[(k * 4) + 1] = (byte)(numArray[k] >> 8);
                buffer[(k * 4) + 2] = (byte)(numArray[k] >> 0x10);
                buffer[(k * 4) + 3] = (byte)(numArray[k] >> 0x18);
            }
            return buffer;
        }

        public byte[] Encrypt(byte[] data)
        {
            if ((data.Length % 8) != 0)
            {
                throw new RC5Exception("Invalid password length. Must be multiple of 8");
            }
            int num = (data.Length / 8) * 8;
            if (num <= 0)
            {
                throw new RC5Exception("Invalid password length. Must be greater than 0 bytes.");
            }
            uint[] numArray = new uint[data.Length / 4];
            for (int i = 0; i < (data.Length / 4); i++)
            {
                numArray[i] = (uint)(((data[i * 4] + (data[(i * 4) + 1] << 8)) + (data[(i * 4) + 2] << 0x10)) + (data[(i * 4) + 3] << 0x18));
            }
            for (int j = 0; j < (num / 8); j++)
            {
                uint num4 = numArray[j * 2];
                uint num5 = numArray[(j * 2) + 1];
                uint num6 = num4 + this.bufSub[0];
                uint num7 = num5 + this.bufSub[1];
                for (int m = 1; m <= 12; m++)
                {
                    num6 = LeftRotate(num6 ^ num7, (int)num7) + this.bufSub[m * 2];
                    num7 = LeftRotate(num7 ^ num6, (int)num6) + this.bufSub[(m * 2) + 1];
                }
                numArray[j * 2] = num6;
                numArray[(j * 2) + 1] = num7;
            }
            byte[] buffer = new byte[numArray.Length * 4];
            for (int k = 0; k < numArray.Length; k++)
            {
                buffer[k * 4] = (byte)numArray[k];
                buffer[(k * 4) + 1] = (byte)(numArray[k] >> 8);
                buffer[(k * 4) + 2] = (byte)(numArray[k] >> 0x10);
                buffer[(k * 4) + 3] = (byte)(numArray[k] >> 0x18);
            }
            return buffer;
        }

        private static uint LeftRotate(uint value, int shiftAmount)
        {
            return ((value << shiftAmount) | (value >> (0x20 - (shiftAmount & 0x1f))));
        }

        private static uint RightRotate(uint value, int shiftAmount)
        {
            return ((value >> shiftAmount) | (value << (0x20 - (shiftAmount & 0x1f))));
        }
    }
    public sealed class RC5Exception : Exception
    {
        public RC5Exception(string message) : base(message)
        {
        }
    }
}

namespace msvcrt
{
    using System;

    public class msvcrt
    {
        private static int _seed = 0;

        public static short rand()
        {
            _seed *= 0x343fd;
            _seed += 0x269ec3;
            return (short)((_seed >> 0x10) & 0x7fff);
        }

        public static void srand(int seed)
        {
            _seed = seed;
        }
    }
}