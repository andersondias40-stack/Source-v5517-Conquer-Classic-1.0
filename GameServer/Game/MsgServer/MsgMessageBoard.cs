using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetMessageBoard(this ServerSockets.Packet stream,
            out UInt16 Index, out MsgMessageBoard.Channel Channel, out String Params, out MsgMessageBoard.Action Action)
        {
            //stream.ReadUInt32(); // 4
            Index = stream.ReadUInt16();//4
            Channel = (MsgMessageBoard.Channel)stream.ReadUInt16();//6
            Action = (MsgMessageBoard.Action)stream.ReadUInt16();//8
            Params = stream.ReadCString(10).Replace("\t", "");//10

        }
        public static unsafe ServerSockets.Packet MessageBoardCreate(this ServerSockets.Packet stream, UInt16 Index, MsgMessageBoard.Channel Channel, String[] Params,
            MsgMessageBoard.Action Action)
        {
            stream.InitWriter();
            Int32 StrLength = 0;
            if (Params != null)
            {
                for (Int32 i = 0; i < Params.Length; i++)
                {
                    if (Params[i] == null || Params[i].Length > 255)
                        return null;

                    StrLength += Params[i].Length + 1;
                }
            }
            stream.Write(Index);//4
            stream.Write((UInt16)Channel);//6
            stream.Write((Byte)Action);//8

            if (Params != null)
            {
                stream.Write((byte)Params.Length);//9
                for (int x = 0; x < Params.Length; x++)
                {
                    stream.Write((byte)Params[x].Length);//10
                    for (byte i = 0; i < (byte)Params[x].Length; i++)
                        stream.Write((byte)Params[x][i]);//11
                }
            }

            stream.Finalize(1111);

            return stream;
        }
    }
    public class MsgMessageBoard
    {
        public enum Action
        {
            None = 0,
            Del = 257,			        // to server					// no return
            GetList = 2,		    	// to server: index(first index)
            List = 3,		        	// to client: index(first index), name, words, time...
            GetWords = 4,	    		// to server: index(for get)	// return by MsgTalk
        };

        public enum Channel
        {
            None = 0,
            MsgTrade = 2201,
            MsgFriend = 2202,
            MsgTeam = 2203,
            MsgSyn = 2204,
            MsgOther = 2205,
            MsgSystem = 2206,
        };
        [PacketAttribute(1111)]
        public unsafe static void Handler(Client.GameClient user, ServerSockets.Packet packet)
        {
            UInt16 Index;
            MsgMessageBoard.Channel Channel;
            String Param;
            MsgMessageBoard.Action Action;
            packet.GetMessageBoard(out Index, out Channel, out Param, out Action);

            switch (Action)
            {
                case Action.Del:
                    {
                        if (Param != user.Player.Name /*|| user.ProjectManager*/) // || GM/PM
                            return;

                        Role.MessageBoard.MessageInfo Info =
                            Role.MessageBoard.GetMsgInfoByAuthor(Param, (UInt16)Channel);

                        Role.MessageBoard.Delete(Info, (UInt16)Channel);
                        break;
                    }
                case Action.GetList:
                    {
                        String[] List = Role.MessageBoard.GetList(Index, (UInt16)Channel);
                        user.Send(packet.MessageBoardCreate(Index, Channel, List, Action.List));
                        break;
                    }
                case Action.GetWords:
                    {
                        String Words = Role.MessageBoard.GetWords(Param, (UInt16)Channel);
                        user.Send(new MsgMessage(Param, user.Player.Name, Words, MsgMessage.MsgColor.white, (MsgMessage.ChatMode)Channel).GetArray(packet));
                        break;
                    }
            }
        }
    }
}