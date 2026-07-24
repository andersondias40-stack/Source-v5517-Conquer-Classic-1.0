using System;
using System.IO;

namespace COServer.WindowsAPI
{
    public unsafe class BinaryFile
    {
        private FileStream pFile;

        public BinaryFile(string FileName, FileMode Mode)
        {
            Open(FileName, Mode);
        }
        public BinaryFile()
        {
            pFile = null;
        }

        public int Position
        {
            get
            {
                if (Success)
                {
                    return Kernel32.SetFilePointer(pFile.SafeFileHandle, 0, null, SeekOrigin.Current);
                }
                else
                {
                    throw new IOException("File isn't open/failed to open previously");
                }
            }
            set
            {
                if (Success)
                {
                    int distance = value - Position;
                    Kernel32.SetFilePointer(pFile.SafeFileHandle, distance, null, SeekOrigin.Current);
                }
                else
                {
                    throw new IOException("File isn't open/failed to open previously");
                }
            }
        }
        public bool Success { get { return pFile != null; } }
        public bool Open(string FileName, FileMode Mode)
        {
            try
            {
                pFile = new FileStream(FileName, Mode);
            }
            catch
            {
                pFile = null;
            }
            return Success;
        }
        public void Close()
        {
            if (Success)
            {
                pFile.Close();
            }
            else
            {
                throw new IOException("File isn't open/failed to open previously");
            }
        }
        public bool Reopen(string FileName, FileMode Mode)
        {
            Close();
            return Open(FileName, Mode);
        }
        public bool Read(void* Buffer, int Count, int Size)
        {
            return Read(Buffer, Count * Size);
        }
        public bool Read(void* Buffer, int AmountOfBytes)
        {
            if (AmountOfBytes < 0)
                throw new ArgumentException("AmountOfBytes");
            int read = -1;
            if (Success)
            {
                Kernel32.ReadFile(pFile.SafeFileHandle, (byte*)Buffer, AmountOfBytes, &read, IntPtr.Zero);
            }
            else
            {
                throw new IOException("File isn't open/failed to open previously");
            }
            return (read == AmountOfBytes);
        }
        public bool Write(void* Buffer, int Count, int Size)
        {
            return Write(Buffer, Count * Size);
        }
        public bool Write(void* Buffer, int AmountOfBytes)
        {
            if (AmountOfBytes < 0)
                throw new ArgumentException("AmountOfBytes");
            int written = -1;
            if (Success)
            {
                Kernel32.WriteFile(pFile.SafeFileHandle, (byte*)Buffer, AmountOfBytes, &written, IntPtr.Zero);
            }
            else
            {
                throw new IOException("File isn't open/failed to open previously");
            }
            if (written != AmountOfBytes)
                Console.WriteLine("Omggggggggggggggggggggggggggggggggggggggggg ");
            return (written == AmountOfBytes);
        }
    }
}
