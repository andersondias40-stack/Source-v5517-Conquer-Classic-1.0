using System;

namespace COServer.Game.MsgServer
{
    public unsafe static class SpellPacket
    {
        public static unsafe void GetSpell(this ServerSockets.Packet stream, out MsgSpell spell)
        {
            spell = new MsgSpell();
            spell.Experience = stream.ReadInt32();
            spell.ID = stream.ReadUInt16();
            //   spell.UnKnow = stream.ReadUInt16();
            spell.Level = stream.ReadUInt16();
            // spell.Type = stream.ReadUInt16();
            //  spell.UseSoul = (MsgSpell.UseSpellSoulTyp)stream.ReadUInt32();
            // spell.Soul = stream.ReadUInt8();
            //spell.HashLevelSoul = stream.ReadInt32();
        }
        public static unsafe ServerSockets.Packet SpellCreate(this ServerSockets.Packet stream, MsgSpell spell)
        {
            stream.InitWriter();
            stream.Write(spell.Experience);//4
            stream.Write(spell.ID);//8
            stream.Write(spell.Level);//10
                                      //  stream.Write(spell.Type);//18
            stream.Finalize(GamePackets.Spell);
            return stream;
        }
    }
    public unsafe class MsgSpell
    {
        public enum UseSpellSoulTyp : uint
        {
            None = 0,
            One = 9,
            Two = 12,
            Three = 16,
            Four = 24
        }
        public Time32 ColdTime = new Time32();
        public int GetColdTime
        {
            get
            {
                return (ColdTime.AllMilliseconds - Time32.Now.AllMilliseconds);
            }
        }
        public bool IsSpellWithColdTime = false;
        public DateTime LastUse = DateTime.Now;
        public ushort Size;
        public ushort PacketType;
        public int Experience;
        public ushort ID;
        public ushort UnKnow;
        public ushort Level
        {
            get
            {
                return _level;
            }
            set
            {
                if (value < _level)
                {

                }
                _level = value;
            }
        }
        public ushort Type;
        public UseSpellSoulTyp UseSoul;
        public byte Soul;
        public int HashLevelSoul;
        public DateTime StampSpell = new DateTime();
        public byte SoulLevel
        {
            get
            {
                return _levelHu;
            }
            set
            {
                _levelHu = value;
                if (value > 0) HashLevelSoul |= (int)(1UL << 1);
                if (value > 1) HashLevelSoul |= (int)(1UL << 4);
                if (value > 2) HashLevelSoul |= (int)(1UL << 8);
                if (value > 3) HashLevelSoul |= (int)(1UL << 16);
            }
        }

        public byte PreviousLevel;

        private byte _levelHu;
        private ushort _level;



        public byte UseSpellSoul
        {
            get
            {
                if (UseSoul == UseSpellSoulTyp.One)
                    return 1;
                if (UseSoul == UseSpellSoulTyp.Two)
                    return 2;
                if (UseSoul == UseSpellSoulTyp.Three)
                    return 3;
                if (UseSoul == UseSpellSoulTyp.Four)
                    return 4;
                return 0;
            }
            set
            {
                if (value == 1)
                    UseSoul = UseSpellSoulTyp.One;
                else if (value == 2)
                    UseSoul = UseSpellSoulTyp.Two;
                else if (value == 3)
                    UseSoul = UseSpellSoulTyp.Three;
                else if (value == 4)
                    UseSoul = UseSpellSoulTyp.Four;
                else
                    UseSoul = UseSpellSoulTyp.None;
            }
        }


        [PacketAttribute(Game.GamePackets.Spell)]
        public unsafe static void HandlerSpell(Client.GameClient client, ServerSockets.Packet stream)
        {

            MsgSpell spell;

            stream.GetSpell(out spell);

            MsgSpell ClientSpell;
            if (client.MySpells.ClientSpells.TryGetValue(spell.ID, out ClientSpell))
            {
                if (ClientSpell.SoulLevel >= spell.UseSpellSoul)
                {
                    ClientSpell.UseSoul = spell.UseSoul;
                    client.MySpells.ClientSpells[spell.ID] = ClientSpell;

                    spell.Type = 3;

                    client.Send(stream.SpellCreate(spell));
                }
            }
        }
    }
}
