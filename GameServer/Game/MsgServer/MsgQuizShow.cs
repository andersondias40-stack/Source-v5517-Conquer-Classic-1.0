using System;

namespace COServer.Game.MsgServer
{
    public static class MsgQuizShow
    {
        public enum AcotionID : ushort
        {
            Open = 1,
            SendQuestion = 2,
            ReceiveToping = 3,
            SendToping = 4,
            HistoryBestsRank = 5,
            GiveAwaser = 3,
            Remove = 8

        }
        public static unsafe void GetQuizShow(this ServerSockets.Packet stream, out AcotionID type, out byte Answer)
        {
            type = (AcotionID)stream.ReadUInt16();
            stream.SeekForward(2);
            Answer = stream.ReadUInt8();
        }
        public static unsafe ServerSockets.Packet QuizShowCreate(this ServerSockets.Packet stream, AcotionID type, ushort DwParam1, ushort DwParam2, byte DwParam3
            , byte DwParam4, ushort DwParam5, ushort DwParam6, ushort DwParam7)
        {
            stream.InitWriter();
            stream.Write((byte)type); stream.Write((byte)9);
            for (int x = 0; x < 10; x++)
            {
                stream.Write((byte)2);
                stream.Write((byte)8);
            }
            /*  stream.Write((byte)9);
              stream.Write(DwParam1);//6

              stream.Write(DwParam2);//8

              stream.Write(DwParam3);//10

              stream.Write(DwParam4);//11
              stream.Write(DwParam5);//14
              stream.Write(DwParam6);//16
              stream.Write(DwParam7);//18*/
            stream.ZeroFill(20);
            stream.Finalize(GamePackets.QuizShow);
            return stream;

        }
        public static unsafe ServerSockets.Packet QuizShowCreate(this ServerSockets.Packet stream, AcotionID type, ushort DwParam1, ushort DwParam2, byte DwParam3
          , byte DwParam4, ushort DwParam5, ushort DwParam6, ushort DwParam7, params string[] texts)
        {
            stream.InitWriter();
            stream.Write((ushort)1);
            stream.Write(DwParam1);//noumber
            stream.Write(DwParam2);//question level 0= none ,1 or 2.
            stream.Write(DwParam3);//time?
            stream.Write(DwParam4);//11
            stream.Write(DwParam5);//14
            stream.Write(DwParam6);//my quiz points
            stream.Write(DwParam7);//18
            stream.ZeroFill(6);
            stream.Write(texts);
            stream.Finalize(GamePackets.QuizShow);
            return stream;

        }
        public static unsafe ServerSockets.Packet QuizShowCreate(this ServerSockets.Packet stream, AcotionID type, ushort DwParam1, ushort DwParam2, byte DwParam3
        , byte DwParam4, ushort DwParam5, ushort DwParam6, ushort DwParam7, params Client.GameClient[] users)
        {
            stream.InitWriter();
            stream.Write((ushort)type);
            stream.Write(DwParam1);//question number
            stream.Write(DwParam2);//question level 1 or 2.
            stream.Write(DwParam3);//10
            stream.Write(DwParam4);//11
            stream.Write(DwParam5);//14
            stream.Write(DwParam6);//my quiz points
            stream.Write(DwParam7);//18
            stream.ZeroFill(2);
            stream.Write(users.Length);

            for (int x = 0; x < Math.Min(3, users.Length); x++)
            {
                var element = users[x];
                stream.Write(element.Player.Name, 16);
                stream.Write((ushort)element.QuizShowPoints);
                stream.Write((ushort)element.QuizRank);
            }

            stream.Finalize(GamePackets.QuizShow);
            return stream;

        }

        [PacketAttribute(GamePackets.QuizShow)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            Console.PrintPacketAdvanced(stream.Memory, stream.Size);
            AcotionID type;
            byte Answer;
            stream.GetQuizShow(out type, out Answer);
            switch (type)
            {
                case AcotionID.SendQuestion:
                    {
                        //if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.QuizShow
                        //    && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                        //{
                        //    var Tournament = MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgQuizShow;
                        //    if (Tournament.CurrentQuestion.CheckAnswer(Answer))
                        //    {
                        //        user.Player.QuizPoints += (ushort)(15 + Tournament.UseQuestion.Count);
                        //        user.QuizShowPoints += (ushort)(15 + Tournament.UseQuestion.Count);
                        //        user.RightAnswer = 1;
                        //        user.GainExpBall(400, true, Role.Flags.ExperienceEffect.angelwing);
                        //    }
                        //    else
                        //    {
                        //        user.QuizShowPoints += 1;
                        //        user.RightAnswer = 2;
                        //    }
                        //    if (Tournament.UseQuestion.Count == 1)
                        //    {
                        //        user.Send(stream.QuizShowCreate(AcotionID.SendToping, user.QuizShowPoints, (ushort)user.GetQuizTimer(), 0, user.RightAnswer, 0, 0, 0));
                        //    }
                        //}
                        break;
                    }
                case AcotionID.Remove:
                    {
                     //   if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.QuizShow
                     //&& MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                     //   {
                     //       var Tournament = MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgQuizShow;
                     //       Tournament.RemovePlayer(user);
                     //   }
                        break;
                    }
            }

        }

    }
}
