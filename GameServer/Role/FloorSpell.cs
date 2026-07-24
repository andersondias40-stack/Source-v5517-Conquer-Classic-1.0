using System;

namespace COServer.Role
{
    public class FloorSpell
    {
        public Time32 Stamp = new Time32();
        public Game.MsgFloorItem.MsgItemPacket FloorPacket;
        public Database.MagicType.Magic DBSkill;

        public FloorSpell(uint ID, ushort X, ushort Y, byte color, Database.MagicType.Magic _DBSkill, int MillisecondsStamp)
        {

            if (Game.MsgFloorItem.MsgItem.UIDS.Count < 900092)
                Game.MsgFloorItem.MsgItem.UIDS.Set(900092);

            FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
            FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Next;
            FloorPacket.m_ID = ID;
            FloorPacket.m_X = X;
            FloorPacket.m_Y = Y;
            FloorPacket.m_Color = color;
            FloorPacket.DropType = Game.MsgFloorItem.MsgDropID.Effect;

            Stamp = Time32.Now.AddMilliseconds(MillisecondsStamp);
            DBSkill = _DBSkill;
        }



        public unsafe class ClientFloorSpells
        {
            public MyList<FloorSpell> Spells = new MyList<FloorSpell>();
            public Game.MsgServer.MsgSpellAnimation SpellPacket;
            public Database.MagicType.Magic DBSkill;
            public ushort X;
            public ushort Y;
            public byte LevelHu;
            public uint UID;
            private object SyncRoot;
            public GameMap GMap;

            public ClientFloorSpells(uint _UID, ushort _X, ushort _Y, byte _LevelHu, Database.MagicType.Magic _DBSkill, GameMap _GMap)
            {
                GMap = _GMap;
                DBSkill = _DBSkill;
                SyncRoot = new object();
                X = _X;
                Y = _Y;
                LevelHu = _LevelHu;
                UID = _UID;

            }
            public void CreateMsgSpell(uint Target)
            {
                SpellPacket = new Game.MsgServer.MsgSpellAnimation(UID, Target, X, Y, DBSkill.ID, DBSkill.Level, LevelHu);
            }

            public bool CheckInvocke(Time32 Now, FloorSpell spell)
            {
                if (Now > spell.Stamp)
                {
                    return true;
                }
                return false;
            }
            public void RemoveItem(FloorSpell item)
            {
                lock (SyncRoot)
                    Spells.Remove(item);
            }
            public void SendView(ServerSockets.Packet stream, Client.GameClient client)
            {

                SpellPacket.SetStream(stream);

                SpellPacket.Send(client);

            }

            public void AddItem(FloorSpell item)
            {
                lock (SyncRoot)
                    Spells.Add(item);
            }
        }
    }
}
