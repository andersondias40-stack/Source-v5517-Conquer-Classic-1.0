using COServer.WindowsAPI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace COServer
{
    public struct ErrorCodes
    {
        public const int INVALID_HANDLE_VALUE = -1;
    }
    public class Console
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static void DissableButton()
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

        }

        private static IntPtr _InputHandle;
        private static IntPtr _OutputHandle;


        private static Counter ExceptionsCounter = new Counter(1);

        /// <summary> The handle Input handle is not recommended </summary>
        public static IntPtr InputHandle
        {
            get { return _InputHandle; }
            set
            {
                if (!Kernel32.SetStdHandle(STD_INPUT_HANDLE, value))
                    throw new Exception("Unable to set the new Input handle");
                _InputHandle = value;
            }
        }

        /// <summary> handle Output handle is not recommended </summary>
        public static IntPtr OutputHandle
        {
            get { return _OutputHandle; }
            set
            {
                if (!Kernel32.SetStdHandle(STD_OUTPUT_HANDLE, value))
                    throw new Exception("Unable to set the new Output handle");
                _OutputHandle = value;
            }
        }

        public static string Title
        {
            get
            {
                StringBuilder sb = new StringBuilder(32767);
                uint size = (uint)sb.Capacity;
                Kernel32.GetConsoleTitle(sb, size);
                sb.Capacity = sb.Length;
                return sb.ToString();
            }
            set { Kernel32.SetConsoleTitle(value); }
        }

        public const int STD_INPUT_HANDLE = -10;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12; //Not being used yet

        static Console()
        {
            InputHandle = Kernel32.GetStdHandle(STD_INPUT_HANDLE);
            OutputHandle = Kernel32.GetStdHandle(STD_OUTPUT_HANDLE);

            if (InputHandle.ToInt32() == ErrorCodes.INVALID_HANDLE_VALUE ||
                OutputHandle.ToInt32() == ErrorCodes.INVALID_HANDLE_VALUE)
            {
                throw new Exception("Unable to get the Console Handle");
            }
        }

        public static void Write(string value)
        {
            uint written = 0;
            Kernel32.WriteConsole(OutputHandle, value, (uint)value.Length, out written, IntPtr.Zero);
        }
        private static DateTime NOW = DateTime.Now;
        private static Time32 NOW32 = Time32.Now;
        public static string TimeStamp()
        {
            return "[" + NOW.AddMilliseconds((Time32.Now - NOW32).GetHashCode()).ToString("hh:mm:ss") + "]";
        }
        public static void WriteLine(object value, ConsoleColor color = ConsoleColor.White)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Write(TimeStamp() + " ");
            System.Console.ForegroundColor = color;
            System.Console.Write(value.ToString() + "\n");
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return System.Console.ReadKey();
        }
        ///<summary> Attach a console to a specified process, CAUTION: If the console will be closed the process will close! </summary>
        public static void AttachConsole(uint PID)
        {
            if (!Kernel32.AttachConsole(PID))
                throw new Exception("Unable to attach the console.");
        }

        /// <summary>
        /// Create a console for the current process, Caution: Closing the console will also close the process!
        /// </summary>
        public static void AllocConsole()
        {
            if (!Kernel32.AllocConsole())
                throw new Exception("Unable to allocate a console.");
            //System.Console.read
        }
        public static void WriteException(Exception e)
        {
            try
            {
                Console.WriteLine(e.ToString());
                SaveException(e);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
        public static void SaveException(Exception e)
        {

            var runDir = Application.StartupPath;

            const string UnhandledExceptionsPath = "Exceptions\\";

            var dt = DateTime.Now;
            string date = dt.Month + "-" + dt.Day + "//";

            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date + e.TargetSite.Name))
                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date + e.TargetSite.Name);

            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + e.TargetSite.Name + "\\";

            string date2 = dt.DayOfYear + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second + "E" + ExceptionsCounter.Next;
            List<string> Lines = new List<string>();

            Lines.Add("----Exception message----");
            Lines.Add(e.Message);
            Lines.Add("----End of exception message----\r\n");

            Lines.Add("----Stack trace----");
            Lines.Add(e.StackTrace);
            Lines.Add("----End of stack trace----\r\n");

            File.WriteAllLines(fullPath + date2 + ".txt", Lines.ToArray());
            WriteLine(e.ToString());
        }
        public static string ReadLine()
        {
            StringBuilder sb = new StringBuilder();
            uint read = 0;


            Kernel32.ReadConsole(InputHandle, sb, 100, out read, IntPtr.Zero);
            if (read == 0)
                return "";
            return sb.ToString(0, (int)read - 2);
        }

        public unsafe static string log1(string name, byte* Data, int leng)
        {
            byte[] packet = new byte[leng];
            fixed (byte* recv_ptr = packet)
            {
                Kernel32.memcpy(recv_ptr, Data, leng);
            }
            return log2(name, packet);
        }
        public unsafe static string log2(string name, byte[] Data)
        {
            try
            {

                if (Data == null) return "";
                ushort PacketLength = (ushort)Data.Length;
                if (ASCIIEncoding.ASCII.GetString(Data).Contains("TQServer") || ASCIIEncoding.ASCII.GetString(Data).Contains("TQClient"))
                    PacketLength += 8;
                if (PacketLength > Data.Length) PacketLength = (ushort)Data.Length;

                string DataStr = "";
                DataStr += name + " " + "Packet Length : " + PacketLength + ", PacketType: " + BitConverter.ToInt16(Data, 2) + Environment.NewLine;


                for (int i = 0; i < Math.Ceiling((double)PacketLength / 16); i++)
                {
                    int t = 16;
                    if (((i + 1) * 16) > PacketLength)
                        t = PacketLength - (i * 16);
                    for (int a = 0; a < t; a++)
                    {
                        DataStr += Data[i * 16 + a].ToString("X2") + " ";

                    }
                    if (t < 16)
                        for (int a = t; a < 16; a++)
                            DataStr += "   ";
                    DataStr += "     ;";

                    for (int a = 0; a < t; a++)//unde e clasa de sockete
                    {
                        DataStr += Convert.ToChar(Data[i * 16 + a]);
                    }
                    DataStr += Environment.NewLine;
                }
                DataStr.Replace(Convert.ToChar(0), '.');
                DataStr += Environment.NewLine;
                return DataStr;
            }
            catch (Exception Exc)
            {
                Console.WriteException(Exc);
            }
            return "";
        }
        public unsafe static void PrintPacketAdvanced(byte* Data, int leng)
        {
            byte[] packet = new byte[leng];
            fixed (byte* recv_ptr = packet)
            {
                Kernel32.memcpy(recv_ptr, Data, leng);
            }
            PrintPacketAdvanced(packet);
        }
        public unsafe static void PrintPacketAdvanced(byte* Data)
        {
#if TEST
            ushort Size =*((ushort*)Data);
            byte[] packet = new byte[Size];
            for (int x = 0; x < Size; x++)
                packet[x] = Data[x];
            PrintPacketAdvanced(packet);
#endif
        }
        public static void SavePacket(byte[] Data)
        {
#if TEST
            try
            {

                if (Data == null) return;
                ushort PacketLength = (ushort)Data.Length;
                if (ASCIIEncoding.ASCII.GetString(Data).Contains("TQServer") || ASCIIEncoding.ASCII.GetString(Data).Contains("TQClient"))
                    PacketLength += 8;
                if (PacketLength > Data.Length) PacketLength = (ushort)Data.Length;

                string DataStr = "";
                DataStr += "Packet Length : " + PacketLength + ", PacketType: " + BitConverter.ToInt16(Data, 2) + Environment.NewLine;


                for (int i = 0; i < Math.Ceiling((double)PacketLength / 16); i++)
                {
                    int t = 16;
                    if (((i + 1) * 16) > PacketLength)
                        t = PacketLength - (i * 16);
                    for (int a = 0; a < t; a++)
                    {
                        DataStr += Data[i * 16 + a].ToString("X2") + " ";

                    }
                    if (t < 16)
                        for (int a = t; a < 16; a++)
                            DataStr += "   ";
                    DataStr += "     ;";

                    for (int a = 0; a < t; a++)//unde e clasa de sockete
                    {
                        DataStr += Convert.ToChar(Data[i * 16 + a]);
                    }
                    DataStr += Environment.NewLine;
                }
                DataStr.Replace(Convert.ToChar(0), '.');
                DataStr += Environment.NewLine;
                System.IO.StreamWriter SW = new System.IO.StreamWriter(@"C:\PacketSniffing\P" + 1 + ".txt", true);
                SW.WriteLine(DataStr);
                SW.Flush();
                SW.Close();
                //MyConsole.WriteLine(DataStr);
            }
            catch (Exception Exc)
            {
                MyConsole.WriteException(Exc);
            }
#endif
        }
        public static void PrintPacketAdvanced(byte[] Data)
        {
            try
            {

                if (Data == null) return;
                ushort PacketLength = (ushort)Data.Length;
                if (ASCIIEncoding.ASCII.GetString(Data).Contains("TQServer") || ASCIIEncoding.ASCII.GetString(Data).Contains("TQClient"))
                    PacketLength += 8;
                if (PacketLength > Data.Length) PacketLength = (ushort)Data.Length;

                string DataStr = "";
                DataStr += "Packet Length : " + PacketLength + ", PacketType: " + BitConverter.ToInt16(Data, 2) + Environment.NewLine;


                for (int i = 0; i < Math.Ceiling((double)PacketLength / 16); i++)
                {
                    int t = 16;
                    if (((i + 1) * 16) > PacketLength)
                        t = PacketLength - (i * 16);
                    for (int a = 0; a < t; a++)
                    {
                        DataStr += Data[i * 16 + a].ToString("X2") + " ";

                    }
                    if (t < 16)
                        for (int a = t; a < 16; a++)
                            DataStr += "   ";
                    DataStr += "     ;";

                    for (int a = 0; a < t; a++)//unde e clasa de sockete
                    {
                        DataStr += Convert.ToChar(Data[i * 16 + a]);
                    }
                    DataStr += Environment.NewLine;
                }
                DataStr.Replace(Convert.ToChar(0), '.');
                //DataStr += Environment.NewLine;
                //System.IO.StreamWriter SW = new System.IO.StreamWriter(@"C:\PacketSniffing\P" + 1 + ".txt", true);
                //SW.WriteLine(DataStr);
                //SW.Flush();
                //SW.Close();
                Console.WriteLine(DataStr);
            }
            catch (Exception Exc)
            {
                Console.WriteException(Exc);
            }
        }
    }
}
