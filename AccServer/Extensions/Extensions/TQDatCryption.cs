using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaTrixAICore
{  
    public enum TqFileCryptoSeed
    {
        Itemtype = 0x2537,
        LevExp = 0x4d2,
        MagicType = Itemtype,
        MapDestination = Itemtype,
        MonsterType = Itemtype,
        MountType = Itemtype,
        Silent = Itemtype,
        UserHelpInfo = Itemtype
    }
    public class DatCryption
    {
        private byte[] key;
        public TqFileCryptoSeed seed = TqFileCryptoSeed.Itemtype;
        public DatCryption(TqFileCryptoSeed _seed = TqFileCryptoSeed.Itemtype)
        {
            this.key = new byte[128];
            this.seed = _seed;
            DatCryption.MSRandom msRandom = new DatCryption.MSRandom(seed);
            for (int index = 0; index < this.key.Length; ++index)
                this.key[index] = (byte)(msRandom.Next() % 256);
        }
        public byte[] Decrypt(byte[] b)
        {
            for (int index = 0; index < b.Length; ++index)
            {
                int num1 = (int)b[index] ^ (int)this.key[index % 128];
                int num2 = index % 8;
                b[index] = (byte)((num1 << 8 - num2) + (num1 >> num2));
            }
            return b;
        }
        public byte[] Encrypt(byte[] b)
        {
            for (int index = 0; index < b.Length; ++index)
            {
                int num1 = index % 8;
                int num2 = (int)(byte)(((int)b[index] >> 8 - num1) + ((int)b[index] << num1));
                b[index] = (byte)((uint)num2 ^ (uint)this.key[index % 128]);
            }
            return b;
        }
        public byte[] Decrypt(string file)
        {
            return this.Decrypt(File.ReadAllBytes(file));
        }
        public class MSRandom
        {
            public long Seed;

            public MSRandom(TqFileCryptoSeed seed)
            {
                this.Seed = (long)seed;
            }

            public int Next()
            {
                return (int)((this.Seed = this.Seed * 214013L + 2531011L) >> 16 & (long)short.MaxValue);
            }
        }
    }
}
