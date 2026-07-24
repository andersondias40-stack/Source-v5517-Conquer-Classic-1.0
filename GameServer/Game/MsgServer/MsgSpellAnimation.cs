using COServer.Client;
using COServer.Database;
using COServer.ServerSockets;
using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer
{
    public class MsgSpellAnimation : IDisposable
    {
        public class SpellObj
        {
            public uint UID;
            public uint Damage;
            public uint Hit;
            public SpellObj()
            {
                Hit = 1;
            }
            public SpellObj(uint _target, uint _damage)
            {
                UID = _target;
                Damage = _damage;
                Hit = 1;
            }
        }

        public uint UID;//4
        public uint OpponentUID;//8
        public ushort X;//8
        public ushort Y;//10
        public ushort SpellID;//12
        public ushort SpellLevel;//14
        public Queue<SpellObj> Targets;//16

        public MsgSpellAnimation()
        {
            Targets = new Queue<SpellObj>();
        }
        public MsgSpellAnimation(uint _uid, uint oponnent
            , ushort _x, ushort _y, ushort _spell, ushort _spelllevel, byte _levelsoul, uint boomb = 0)
        {
            Targets = new Queue<SpellObj>();

            UID = _uid;
            OpponentUID = oponnent;
            X = _x;
            Y = _y;
            SpellID = _spell;
            SpellLevel = _spelllevel;
        }
        private unsafe ServerSockets.Packet CreateAnimation(Queue<SpellObj> Spells, ServerSockets.Packet stream)
        {
            stream.InitWriter();
            stream.Write(UID);//4 attacker
            if (OpponentUID != 0)
                stream.Write(OpponentUID);//8
            {
                stream.Write(X);//8
                stream.Write(Y);//10
            }
            stream.Write(SpellID);//12
            stream.Write(SpellLevel);//14
                                     //if (SpellID == (ushort)Role.Flags.SpellID.FastBlader
                                     //    || SpellID == (ushort)Role.Flags.SpellID.ScrenSword
                                     //    || SpellID == (ushort)Role.Flags.SpellID.ViperFang)//UGLY CODE TO AVOID 2 SHOT IN SAME TIME UNTAL TO KNOW THE PROBLEM
                                     //{
                                     //if (Targets.Count == 2 && UID == UID)
                                     //    stream.Write((uint)1);//16
                                     //else if (Targets.Count == 3 && UID == UID)
                                     //    stream.Write((uint)2);//16
                                     //else
            stream.Write((uint)Targets.Count);//16
                                              //}
                                              //else
                                              //{
                                              //    stream.Write((uint)Targets.Count);//16
                                              //}
                                              // SpellObj Obj;

            foreach (var Obj in Spells.ToArray())
            {
                // if (Obj.Damage >= 1)
                {
                    stream.Write(Obj.UID);//4 -- 20
                    stream.Write(Obj.Damage);//8
                    stream.Write(Obj.Hit);//12
                                          //stream.Write((uint)Obj.Effect);//16
                                          //stream.Write((uint)0);//unknow//20
                                          //stream.Write((uint)Obj.MoveX);//24
                                          //stream.Write((uint)Obj.MoveY);//28
                                          //stream.Write((uint)0);//unknow//32
                }
            }
            stream.Finalize(Game.GamePackets.SpellUse);
            return stream;
        }
        public ServerSockets.Packet _Stream;
        public unsafe void SetStream(ServerSockets.Packet stream)
        {
            _Stream = stream;

        }
        public void JustMe(Client.GameClient user)
        {
            if (Targets.Count < 30)
            {
                Console.WriteLine("JustMe" + Targets.Count);
                user.Send(CreateAnimation(Targets, _Stream));
            }
            else
            {
                Dictionary<uint, Queue<SpellObj>> BigArray = new Dictionary<uint, Queue<SpellObj>>();
                //BigArray.Add(0, new Queue<SpellObj>());
                var TargetsArray = Targets.ToArray();
                uint count = 0;
                for (int x = 0; x < TargetsArray.Length; x++)
                {
                    if (x % 30 == 0)
                    {
                        count++;
                        BigArray.Add(count, new Queue<SpellObj>());
                    }
                    BigArray[count].Enqueue(TargetsArray[x]);
                }
                foreach (var small_array in BigArray.Values)
                    user.Send(CreateAnimation(small_array, _Stream));

            }
        }
        public void Send(Client.GameClient user, bool self = true)
        {
            {
                if (Targets.Count < 30)
                {
                    user.Player.View.SendView(CreateAnimation(Targets, _Stream), self);
                }
                else
                {
                    Dictionary<uint, Queue<SpellObj>> BigArray = new Dictionary<uint, Queue<SpellObj>>();
                    //BigArray.Add(0, new Queue<SpellObj>());
                    var TargetsArray = Targets.ToArray();
                    uint count = 0;
                    for (int x = 0; x < TargetsArray.Length; x++)
                    {
                        if (x % 30 == 0)
                        {
                            count++;
                            BigArray.Add(count, new Queue<SpellObj>());
                        }
                        BigArray[count].Enqueue(TargetsArray[x]);
                    }
                    foreach (var small_array in BigArray.Values)
                        user.Player.View.SendView(CreateAnimation(small_array, _Stream), self);

                }
            }
        }
        public void Send(MsgMonster.MonsterRole monster)
        {
            if (Targets.Count < 30)
            {
                monster.Send(CreateAnimation(Targets, _Stream));
            }
            else
            {
                Dictionary<uint, Queue<SpellObj>> BigArray = new Dictionary<uint, Queue<SpellObj>>();
                //   BigArray.Add(0, new Queue<SpellObj>());
                var TargetsArray = Targets.ToArray();
                uint count = 0;
                for (int x = 0; x < TargetsArray.Length; x++)
                {
                    if (x % 30 == 0)// x % 30 = 0
                    {
                        count++;
                        BigArray.Add(count, new Queue<SpellObj>());
                    }
                    BigArray[count].Enqueue(TargetsArray[x]);
                }
                foreach (var small_array in BigArray.Values)
                    monster.Send(CreateAnimation(small_array, _Stream));

            }

        }

        public void Dispose()
        {
            Targets = null;
            Console.WriteLine("called sisposed");
        }

        internal void TargetSend(GameClient user, InteractQuery attack, uint duration, Dictionary<ushort, MagicType.Magic> dBSpells, bool Update, Packet stream)
        {
            if (Update == true)
            {
                AttackHandler.Updates.IncreaseExperience.Up(stream, user, duration);
            }
            AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, attack, duration, dBSpells);//update spell experiance 
            SetStream(stream);// set stream >> packet size
            Send(user);//send spell 
        }
    }
}
