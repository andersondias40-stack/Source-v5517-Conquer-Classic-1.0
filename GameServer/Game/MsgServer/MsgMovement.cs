using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace COServer.Game.MsgServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WalkQuery
    {
        public uint Direction;//4
        public uint UID;//8
        public uint Running;
        public uint TimeStamp;
    }
    public static unsafe class MsgMovement
    {
        public const uint Walk = 0, Run = 1, Steed = 9;


        public static sbyte[] DeltaMountX = new sbyte[24] { 0, -2, -2, -2, 0, 2, 2, 2, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1 };
        public static sbyte[] DeltaMountY = new sbyte[24] { 2, 2, 0, -2, -2, -2, 0, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2 };


        public static unsafe void GetWalk(this ServerSockets.Packet stream, WalkQuery* pQuery)
        {
            stream.ReadUnsafe(pQuery, sizeof(WalkQuery));
        }

        public static unsafe ServerSockets.Packet MovementCreate(this ServerSockets.Packet stream, WalkQuery* pQuery)
        {
            stream.InitWriter();
            pQuery->TimeStamp = (uint)Time32.Now.Value;
            stream.WriteUnsafe(pQuery, sizeof(WalkQuery));
            stream.Finalize(GamePackets.Movement);

            return stream;
        }

        public static uint Bodyyyy = 0;
        public static uint UIDDDD = 1000000;
        public static int eeffect = 1;
        public static int LastClientStamp = 0;
        [PacketAttribute(GamePackets.Movement)]
        public unsafe static void Movement(Client.GameClient client, ServerSockets.Packet packet)
        {
            if (client.Player.Robot)
            {
                client.Pullback();
                client.Player.MessageBox("You can't move while auto hunting.", new Action<Client.GameClient>(p =>
                {
                    p.Player.Robot = false;
                    p.AutoHunting = 0;
                }), null, 0);
            }
            if (client.Player.InUseIntensify)
            {
                if (client.Player.ContainFlag(MsgUpdate.Flags.Intensify))
                    client.Player.RemoveFlag(MsgUpdate.Flags.Intensify);
                client.Player.InUseIntensify = false;
            }
            bool MyPet = false;
            if (client.Player.Mining) client.Player.Mining = false;
            client.Player.LastMove = DateTime.Now;
            if (client.Player.BlockMovementCo)
            {
                if (DateTime.Now < client.Player.BlockMovement)
                {
                    client.SendSysMesage($"You can`t move for {(client.Player.BlockMovement - DateTime.Now).TotalSeconds} seconds.");
                    client.Pullback();
                    return;
                }
                else
                    client.Player.BlockMovementCo = false;
            }

            Role.Flags.ConquerAngle dir;
            WalkQuery walkPacket;
            packet.GetWalk(&walkPacket);
            if (client.Pet != null)
                MyPet = (walkPacket.UID == client.Pet.monster.UID);
            if (!MyPet && walkPacket.UID != client.Player.UID)
                return;
            ushort walkX = MyPet ? client.Pet.monster.X : client.Player.X, walkY = MyPet ? client.Pet.monster.Y : client.Player.Y;
            dir = (Role.Flags.ConquerAngle)(walkPacket.Direction % 8);

            if (!MyPet)
            {
                walkPacket.UID = client.Player.UID;

                client.Player.Action = Role.Flags.ConquerAction.None;
                client.OnAutoAttack = false;
                client.Player.Protect = Time32.Now;
                client.Player.RemoveBuffersMovements(packet);


                Role.Core.IncXY(dir, ref walkX, ref walkY);


                if (client.Map == null)
                {
                    client.Teleport(428, 378, 1002);
                    return;
                }

                if (client.Player.Map == 1038)
                {
                    if (!Game.MsgTournaments.MsgSchedules.GuildWar.ValidWalk(client.TerainMask, out client.TerainMask, walkX, walkY))
                    {
                        client.SendSysMesage("Illegal jumping over the gates detected.");
                        client.Pullback();
                        return;
                    }
                }


                #region BlueMouse Teleporter
                if (client.Player.Map == 1025 && client.Player.X >= 166 && client.Player.X <= 169
                    && client.Player.Y >= 110 && client.Player.Y <= 114)
                {
                    client.Teleport(16, 80, 1502);
                }
                if (client.Player.Map == 1025 && client.Player.X >= 112 && client.Player.X <= 117
                  && client.Player.Y >= 159 && client.Player.Y <= 163)
                {
                    client.Teleport(74, 17, 1503);
                }
                if (client.Player.Map == 1502 && client.Player.X >= 10 && client.Player.X <= 14
                  && client.Player.Y >= 77 && client.Player.Y <= 79)
                {
                    client.Teleport(162, 111, 1025);
                }
                if (client.Player.Map == 1503 && client.Player.X >= 72 && client.Player.X <= 74
                 && client.Player.Y >= 11 && client.Player.Y <= 14)
                {
                    client.Teleport(114, 157, 1025);
                }
                #endregion
                #region AncientDevil
                if (client.Player.Map == 1082 && !Game.MsgTournaments.MsgSchedules.SpawnDevil)
                {
                    TimeSpan horaAtual = DateTime.Now.TimeOfDay;

                    //// A cada 2 horas, verificar se estamos nos primeiros 10 minutos
                    if (horaAtual.Hours % 2 == 0 && horaAtual.Minutes >= 0 && horaAtual.Minutes < 10)
                    {
                        if (client.Player.X >= 217 && client.Player.X <= 218 && client.Player.Y >= 207 && client.Player.Y <= 208)
                        {
                            if (client.Inventory.Contain(710011, 1)
                            && client.Inventory.Contain(710012, 1)
                            && client.Inventory.Contain(710013, 1)
                                && client.Inventory.Contain(710014, 1)
                                && client.Inventory.Contain(710015, 1))
                            {
                                client.Inventory.Remove(710011, 1, packet);
                                client.Inventory.Remove(710012, 1, packet);
                                client.Inventory.Remove(710013, 1, packet);
                                client.Inventory.Remove(710014, 1, packet);
                                client.Inventory.Remove(710015, 1, packet);
                                Database.Server.AddMapMonster(packet, client.Map, 9111, 213, 205, 1, 1, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                Game.MsgTournaments.MsgSchedules.SpawnDevil = true;
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The AncientDevil is being awaken! Prepare yourself to fight, it will appear in a matter of seconds!.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(packet));
                                Program.DiscordAPIevents.Enqueue("``The AncientDevil is being awaken! Prepare yourself to fight, it will appear in a matter of seconds!.``");
                            }
                            else
                            {
                                client.SendSysMesage("You can only summon the AncientDevil inside its map near 218,208! Also you need the five Amulets (Trojan, Fire, Archer, Warrior and Water)");
                            }
                        }
                    }
                    else
                    {
                        client.SendSysMesage("The AncientDevil can only be summoned every 2 hours, and only during the first 10 minutes of the hour.");
                    }
                }
                #endregion

                if (client.Player.ObjInteraction != null)
                {
                    if (client.Player.ObjInteraction.Player.X == client.Player.X && client.Player.ObjInteraction.Player.Y == client.Player.Y)
                    {
                        client.Map.View.MoveTo<Role.IMapObj>(client.Player, walkX, walkY);
                        client.Player.X = walkX;
                        client.Player.Y = walkY;
                        client.Player.Angle = dir;
                        client.Map.View.MoveTo<Role.IMapObj>(client.Player.ObjInteraction.Player, walkX, walkY);
                        client.Player.ObjInteraction.Player.X = walkX;
                        client.Player.ObjInteraction.Player.Y = walkY;
                        client.Player.ObjInteraction.Player.Angle = dir;
                        client.Player.ObjInteraction.Player.View.Role();
                        return;
                    }
                }
                client.Player.View.SendView(packet.MovementCreate(&walkPacket), true);
                client.Map.View.MoveTo<Role.IMapObj>(client.Player, walkX, walkY);
                client.Player.X = walkX;
                client.Player.Y = walkY;
                client.Player.Angle = dir;

                client.Player.View.Role(false, packet.MovementCreate(&walkPacket));

                if (client.Player.ActivePick)
                    client.Player.RemovePick(packet);
                var Squama = MsgTournaments.MsgSchedules.Squama.Squama.Where(x => x.Value.X == client.Player.X && x.Value.Y == client.Player.Y).SingleOrDefault();
                if (Squama.Value != null)
                {
                    MsgTournaments.MsgSchedules.Squama.ClaimedReward(client, Squama.Key);
                    Squama.Value.SquamaTrap = false;
                }
            }
            else if (client.Pet != null)
            {
                walkPacket.UID = client.Pet.monster.UID;
                walkPacket.Running = 1;
                walkPacket.Direction = (byte)dir;
                client.Pet.monster.Action = Role.Flags.ConquerAction.None;
                client.OnAutoAttack = false;

                Role.Core.IncXY(dir, ref walkX, ref walkY);
                client.Player.View.SendView(packet.MovementCreate(&walkPacket), true);
                client.Map.View.MoveTo<Role.IMapObj>(client.Pet.monster, walkX, walkY);
                client.Pet.monster.X = walkX;
                client.Pet.monster.Y = walkY;
                client.Pet.monster.Facing = dir;
                client.Player.View.Role(false, packet.MovementCreate(&walkPacket));
                client.Pet.monster.UpdateMonsterView(client.Player.View, packet);
                //client.Player.View.SendView(client.Pet.monster.GetArray(packet, false), true);
            }
        }
    }
}