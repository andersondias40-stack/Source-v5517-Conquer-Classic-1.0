using System;
using System.Collections.Generic;

namespace COServer.Role
{
    public class MessageBoard
    {
        private const Int32 TITLE_SIZE = 44;
        private const Int32 LIST_SIZE = 10;

        private static List<MessageInfo> TradeBoard = new List<MessageInfo>();
        private static List<MessageInfo> FriendBoard = new List<MessageInfo>();
        private static List<MessageInfo> TeamBoard = new List<MessageInfo>();
        private static List<MessageInfo> SynBoard = new List<MessageInfo>();
        private static List<MessageInfo> OtherBoard = new List<MessageInfo>();
        private static List<MessageInfo> SystemBoard = new List<MessageInfo>();

        public struct MessageInfo
        {
            public String Author;
            public String Words;
            public String Date;
        };

        public static void Add(String Author, String Words, UInt16 Channel)
        {
            MessageInfo Info = new MessageInfo();
            Info.Author = Author;
            Info.Words = Words;
            Info.Date = DateTime.Now.ToString("yyyyMMddHHmmss");

            switch (Channel)
            {
                case 2201:
                    TradeBoard.Add(Info);
                    break;
                case 2202:
                    FriendBoard.Add(Info);
                    break;
                case 2203:
                    TeamBoard.Add(Info);
                    break;
                case 2204:
                    SynBoard.Add(Info);
                    break;
                case 2205:
                    OtherBoard.Add(Info);
                    break;
                case 2206:
                    SystemBoard.Add(Info);
                    break;
            }
        }

        public static void Delete(MessageInfo Message, UInt16 Channel)
        {
            switch (Channel)
            {
                case 2201:
                    if (TradeBoard.Contains(Message))
                        TradeBoard.Remove(Message);
                    break;
                case 2202:
                    if (FriendBoard.Contains(Message))
                        FriendBoard.Remove(Message);
                    break;
                case 2203:
                    if (TeamBoard.Contains(Message))
                        TeamBoard.Remove(Message);
                    break;
                case 2204:
                    if (SynBoard.Contains(Message))
                        SynBoard.Remove(Message);
                    break;
                case 2205:
                    if (OtherBoard.Contains(Message))
                        OtherBoard.Remove(Message);
                    break;
                case 2206:
                    if (SystemBoard.Contains(Message))
                        SystemBoard.Remove(Message);
                    break;
            }
        }

        public static String[] GetList(UInt16 Index, UInt16 Channel)
        {
            MessageInfo[] Board = null;
            switch (Channel)
            {
                case 2201:
                    Board = TradeBoard.ToArray();
                    break;
                case 2202:
                    Board = FriendBoard.ToArray();
                    break;
                case 2203:
                    Board = TeamBoard.ToArray();
                    break;
                case 2204:
                    Board = SynBoard.ToArray();
                    break;
                case 2205:
                    Board = OtherBoard.ToArray();
                    break;
                case 2206:
                    Board = SystemBoard.ToArray();
                    break;
                default:
                    return null;
            }

            if (Board.Length == 0)
                return null;

            if ((Index / 8 * LIST_SIZE) > Board.Length)
                return null;

            String[] List = null;

            Int32 Start = (Board.Length - ((Index / 8 * LIST_SIZE) + 1));

            if (Start < LIST_SIZE)
                List = new String[(Start + 1) * 3];
            else
                List = new String[LIST_SIZE * 3];

            Int32 End = (Start - (List.Length / 3));

            Int32 x = 0;
            for (Int32 i = Start; i > End; i--)
            {
                List[x + 0] = Board[i].Author;
                if (Board[i].Words.Length > TITLE_SIZE)
                    List[x + 1] = Board[i].Words.Remove(TITLE_SIZE, Board[i].Words.Length - TITLE_SIZE);
                else
                    List[x + 1] = Board[i].Words;
                List[x + 2] = Board[i].Date;
                x += 3;
            }
            return List;
        }

        public static String GetWords(String Author, UInt16 Channel)
        {
            MessageInfo[] Board = null;
            switch (Channel)
            {
                case 2201:
                    Board = TradeBoard.ToArray();
                    break;
                case 2202:
                    Board = FriendBoard.ToArray();
                    break;
                case 2203:
                    Board = TeamBoard.ToArray();
                    break;
                case 2204:
                    Board = SynBoard.ToArray();
                    break;
                case 2205:
                    Board = OtherBoard.ToArray();
                    break;
                case 2206:
                    Board = SystemBoard.ToArray();
                    break;
                default:
                    return "";
            }

            foreach (MessageInfo Info in Board)
            {
                if (Info.Author == Author)
                    return Info.Words;
            }
            return "";
        }

        public static MessageInfo GetMsgInfoByAuthor(String Author, UInt16 Channel)
        {
            MessageInfo[] Board = null;
            switch (Channel)
            {
                case 2201:
                    Board = TradeBoard.ToArray();
                    break;
                case 2202:
                    Board = FriendBoard.ToArray();
                    break;
                case 2203:
                    Board = TeamBoard.ToArray();
                    break;
                case 2204:
                    Board = SynBoard.ToArray();
                    break;
                case 2205:
                    Board = OtherBoard.ToArray();
                    break;
                case 2206:
                    Board = SystemBoard.ToArray();
                    break;
                default:
                    return new MessageInfo();
            }

            foreach (MessageInfo Info in Board)
            {
                if (Info.Author == Author)
                    return Info;
            }
            return new MessageInfo();
        }
    }

}
