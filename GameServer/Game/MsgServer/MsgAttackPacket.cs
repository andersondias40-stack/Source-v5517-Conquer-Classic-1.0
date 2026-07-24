
using COServer.Role.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace COServer.Game.MsgServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct InteractQuery
    {
        public static InteractQuery ShallowCopy(InteractQuery item)
        {
            return (InteractQuery)item.MemberwiseClone();
        }

        public int TimeStamp;//4
        public uint UID;//8 attacker
        public uint OpponentUID;//12 attacked
        public ushort X;//16
        public ushort Y;//18
        public MsgAttackPacket.AttackID AtkType;//20
        public ushort SpellID;//24
        public bool KilledMonster
        {
            get { return (SpellID == 1); }
            set { SpellID = (ushort)(value ? 1 : 0); }
        }
        public ushort SpellLevel;//30
        public uint KillCounter
        {
            get { return SpellLevel; }
            set { SpellLevel = (ushort)value; }
        }
        public int Damage//24
        {
            get { fixed (void* ptr = &SpellID) { return *((int*)ptr); } }
            set { fixed (void* ptr = &SpellID) { *((int*)ptr) = value; } }
        }
        //public int Data;
        //{
        //    get { fixed (void* ptr = &X) { return *((int*)ptr); } }
        //    set { fixed (void* ptr = &X) { *((int*)ptr) = value; } }
        //}
        public int dwParam
        {
            get { fixed (void* ptr = &SpellLevel) { return *((int*)ptr); } }
            set { fixed (void* ptr = &SpellLevel) { *((int*)ptr) = value; } }
        }
        public bool OnCounterKill
        {

            get { return Damage != 0; }
            set { Damage = value ? 1 : 0; }
        }
        public uint ResponseDamage;//28
    }

    public unsafe static class MsgAttackPacket
    {

        public enum AttackID : uint
        {
            None = 0x00,
            Physical = 2,
            Magic = 24,
            Archer = 28,
            RequestMarriage = 8,
            AcceptMarriage = 9,
            Death = 0x0E,
            Reflect = 26,
            Dash = 27,//28?
            UpdateHunterJar = 36,
            CounterKillSwitch = 44,
            Scapegoat = 43,
            MerchantAccept = 40,
            MerchantRefuse = 34,
            MerchantProgress = 42,

            FatalStrike = 45,
            InteractionRequest = 46,
            InteractionAccept = 47,
            InteractionRefuse = 48,
            InteractionEffect = 49,
            InteractionStopEffect = 50,
            InMoveSpell = 53,
            BlueDamage = 55,
            BackFire = 57

        }
        public static unsafe void Interaction(this ServerSockets.Packet stream, InteractQuery* pQuery, COServer.Client.GameClient user)
        {

            //stream.ReadUnsafe(pQuery, sizeof(InteractQuery));

            //if (pQuery->AtkType == AttackID.Magic && user.OnAutoAttack == false)
            //{
            //    DecodeMagicAttack(pQuery);
            //}
            stream.ReadUnsafe(pQuery, sizeof(InteractQuery));

            if (pQuery->AtkType == AttackID.Magic && user.OnAutoAttack == false)
            {
                DecodeMagicAttack(pQuery);
            }
        }

        public static unsafe ServerSockets.Packet InteractionCreate(this ServerSockets.Packet stream, InteractQuery* pQuery)
        {
            // pQuery->Timestamp = TimeStamp.GetTime();
            if (pQuery->AtkType == AttackID.Magic)
            {
                EncodeMagicAttack(pQuery);
            }

            stream.InitWriter();

            stream.WriteUnsafe(pQuery, sizeof(InteractQuery));
            stream.Finalize(GamePackets.Attack);

            return stream;
        }

        /// <summary>
        /// original  from cosv3
        /// </summary>
        /// <param name="pQuery"></param>
        /// 
        //public static unsafe void EncodeMagicAttack(InteractQuery* pQuery)
        //{
        //    int magicType, magicLevel;
        //    BitUnfold32(pQuery->Damage, out magicType, out magicLevel);

        //    magicType = (ushort)(ExchangeShortBits((uint)magicType - 0x14be, 3) ^ pQuery->UID ^ 0x915d);
        //    magicLevel = (ushort)((magicLevel + 0x100 * (pQuery->TimeStamp % 0x100)) ^ 0x3721);

        //    pQuery->Damage = BitFold32(magicType, magicLevel);
        //    pQuery->OpponentUID = (uint)ExchangeLongBits((((uint)pQuery->OpponentUID - 0x8b90b51a) ^ (uint)pQuery->UID ^ 0x5f2d2463u), 32 - 13);
        //    pQuery->X = (ushort)(ExchangeShortBits((uint)pQuery->X - 0xdd12, 1) ^ pQuery->UID ^ 0x2ed6);
        //    pQuery->Y = (ushort)(ExchangeShortBits((uint)pQuery->Y - 0x76de, 5) ^ pQuery->UID ^ 0xb99b);
        //}
        //private static unsafe void DecodeMagicAttack(InteractQuery* pQuery)
        //{
        //    int magicType, magicLevel;
        //    BitUnfold32(pQuery->Damage, out magicType, out magicLevel);

        //    magicType = (ushort)(ExchangeShortBits(((ushort)magicType ^ (uint)pQuery->UID ^ 0x915d), 16 - 3) + 0x14be);
        //    magicLevel = (ushort)(((byte)magicLevel) ^ 0x21);

        //    pQuery->Damage = BitFold32(magicType, magicLevel);
        //    pQuery->OpponentUID = (uint)((ExchangeLongBits((uint)pQuery->OpponentUID, 13) ^ (uint)pQuery->UID ^ 0x5f2d2463) + 0x8b90b51a);
        //    pQuery->X = (ushort)(ExchangeShortBits(((ushort)pQuery->X ^ (uint)pQuery->UID ^ 0x2ed6), 16 - 1) + 0xdd12);
        //    pQuery->Y = (ushort)(ExchangeShortBits(((ushort)pQuery->Y ^ (uint)pQuery->UID ^ 0xb99b), 16 - 5) + 0x76de);
        //}
        public static unsafe void EncodeMagicAttack(InteractQuery* pQuery)
        {
            int magicType, magicLevel;
            BitUnfold32(pQuery->Damage, out magicType, out magicLevel);

            magicType = (ushort)(ExchangeShortBits((uint)magicType - 0x14be, 3) ^ pQuery->UID ^ 0x915d);
            magicLevel = (ushort)((magicLevel + 0x100 * (pQuery->TimeStamp % 0x100)) ^ 0x3721);

            pQuery->Damage = BitFold32(magicType, magicLevel);
            pQuery->OpponentUID = (uint)ExchangeLongBits((((uint)pQuery->OpponentUID - 0x8b90b51a) ^ (uint)pQuery->UID ^ 0x5f2d2463u), 32 - 13);
            pQuery->X = (ushort)(ExchangeShortBits((uint)pQuery->X - 0xdd12, 1) ^ pQuery->UID ^ 0x2ed6);
            pQuery->Y = (ushort)(ExchangeShortBits((uint)pQuery->Y - 0x76de, 5) ^ pQuery->UID ^ 0xb99b);
        }
        private static unsafe void DecodeMagicAttack(InteractQuery* pQuery)
        {
            int magicType, magicLevel;
            BitUnfold32(pQuery->Damage, out magicType, out magicLevel);

            magicType = (ushort)(ExchangeShortBits(((ushort)magicType ^ (uint)pQuery->UID ^ 0x915d), 16 - 3) + 0x14be);
            magicLevel = (ushort)(((byte)magicLevel) ^ 0x21);

            pQuery->Damage = BitFold32(magicType, magicLevel);
            pQuery->OpponentUID = (uint)((ExchangeLongBits((uint)pQuery->OpponentUID, 13) ^ (uint)pQuery->UID ^ 0x5f2d2463) + 0x8b90b51a);
            pQuery->X = (ushort)(ExchangeShortBits(((ushort)pQuery->X ^ (uint)pQuery->UID ^ 0x2ed6), 16 - 1) + 0xdd12);
            pQuery->Y = (ushort)(ExchangeShortBits(((ushort)pQuery->Y ^ (uint)pQuery->UID ^ 0xb99b), 16 - 5) + 0x76de);
        }
        public static int BitFold32(int lower16, int higher16)
        {
            return (lower16) | (higher16 << 16);
        }
        public static void BitUnfold32(int bits32, out int lower16, out int upper16)
        {
            lower16 = (int)(bits32 & UInt16.MaxValue);
            upper16 = (int)(bits32 >> 16);
        }
        public static void BitUnfold64(ulong bits64, out int lower32, out int upper32)
        {
            lower32 = (int)(bits64 & UInt32.MaxValue);
            upper32 = (int)(bits64 >> 32);
        }
        private static uint ExchangeShortBits(uint data, int bits)
        {
            data &= 0xffff;
            return ((data >> bits) | (data << (16 - bits))) & 0xffff;
        }

        public static uint ExchangeLongBits(uint data, int bits)
        {
            return (data >> bits) | (data << (32 - bits));
        }
        [PacketAttribute(GamePackets.Attack)]
        public static void HandlerProcess(Client.GameClient user, ServerSockets.Packet stream)
        {
            user.Player.Protect = Time32.Now;
            user.OnAutoAttack = false;
            user.Player.RemoveBuffersMovements(stream);
            user.Player.Action = Role.Flags.ConquerAction.None;

            if (user.Player.BlockMovementCo)
            {
                if (DateTime.Now < user.Player.BlockMovement)
                {
                    user.SendSysMesage($"You can`t move for {(user.Player.BlockMovement - DateTime.Now).TotalSeconds} seconds.");
                    user.Pullback();
                    return;
                }
                else
                    user.Player.BlockMovementCo = false;
            }

            InteractQuery Attack;
            stream.Interaction(&Attack, user);
            // Proteção para novatos em TwinCity contra PK/ Team
            Role.IMapObj target;
            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
            {
                var attacked = target as Role.Player;
                if (attacked != null && attacked.Map == 1002 && attacked.Level <= 1)
                {
                    if (user.Player.PkMode == Role.Flags.PKMode.PK || user.Player.PkMode == Role.Flags.PKMode.Team)
                    {
                        user.SendSysMesage("You cannot attack level 1 players in TwinCity while in PK/Team mode.", MsgMessage.ChatMode.System);
                        return; // bloqueia o ataque
                    }
                }
            }



            //   Console.WriteLine(Attack.TimeStamp);
            //  Console.WriteLine(Environment.TickCount);
            //  Console.WriteLine(Attack.PacketStamp);
            if (user.Player.ActivePick)
                user.Player.RemovePick(stream);

            /*    var action = new ActionQuery()
                 {
                     ObjId = user.Player.UID,
                     Type = ActionType.AbortMagic
                 };
                 user.Send(stream.ActionCreate(&action));
              */
            user.Player.LastAttack = Time32.Now;
            if (user.Player.WaitingKillCaptcha)
            {
                user.Player.KillCountCaptchaStamp = Time32.Now;
                user.Player.WaitingKillCaptcha = true;
                user.ActiveNpc = 9999997;
                if (user.Player.KillCountCaptcha == "")
                    user.Player.KillCountCaptcha = Role.Core.Random.Next(10000, 50000).ToString();
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(user, stream);
                dialog.Text("Input the current text: " + user.Player.KillCountCaptcha + " to verify you're human.")
                    .AddInput("Captcha message:", (byte)user.Player.KillCountCaptcha.Length)
                    .Option("No thank you.", 255)
                    .AddAvatar(39)
                    .FinalizeDialog();
                //  user.Send(dialog.stream);
                return;
            }

            if (user != null && Database.ItemType.IsShield(user.Equipment.LeftWeapon) && !(user.Player.Class >= 21 && user.Player.Class <= 25))
            {
                if (user.Inventory.HaveSpace(1))
                {
                    user.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream);
                    user.Equipment.QueryEquipment();
                }
                else
                    user.SendSysMesage("Remove the shield or free 1 inventory space.");
                return;
            }
            // yasta packet el attack called 1 time when the character start to attack then the thread work , i know :D
            //var obj = Database.Server.GamePoll.Values.Where(x => x.Player.UID == Attack.OpponentUID && x.Pet != null).SingleOrDefault();
            //if (Database.Server.GamePoll.TryGetValue(Attack.OpponentUID, out Client.GameClient obj))
            //{
            //    if (user.Pet != null) user.Pet.monster.Target = obj.Player;
            //}


            Process(user, Attack);




            //    Console.WriteLine("attack stamp" + Attack.TimeStamp);
            //  Attack.TimeStamp = Attack.PacketStamp = Environment.TickCount;
            //  EncodeMagicAttack(&Attack);



        }

        public static void Process(Client.GameClient user, InteractQuery Attack)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();


                if (!user.Player.Alive)
                {
                    return;
                }

                if (user.Player.Map != 1039)//training
                    AttackHandler.CheckAttack.CheckItems.AttackDurability(user, stream);

                if (user.Player.ContainFlag(MsgUpdate.Flags.Freeze))
                {
                    return;
                }
                if (!EventsLib.EventManager.ExecuteSkill((uint)Attack.AtkType, 0, user))
                    return;
                switch (Attack.AtkType)
                {
                    case AttackID.MerchantAccept:
                        {
                            user.Player.Merchant = 255;
                            user.Send(stream.InteractionCreate(&Attack));
                            break;
                        }
                    case AttackID.MerchantRefuse:
                        {
                            user.Player.Merchant = 0;
                            user.Send(stream.InteractionCreate(&Attack));
                            break;
                        }
                    case AttackID.UpdateHunterJar:
                        {
                            MsgGameItem Jar;
                            if (user.Inventory.TryGetItem(user.DemonExterminator.ItemUID, out Jar))
                            {
                                Attack.UID = Attack.OpponentUID = user.Player.UID;
                                Attack.X = Jar.MaximDurability;
                                Attack.Y = 0;
                                Attack.dwParam = (ushort)((user.DemonExterminator.HuntKills << 16) | Jar.MaximDurability);

                                user.Send(stream.InteractionCreate(&Attack));
                            }
                            break;
                        }
                    case AttackID.InteractionRequest:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.ObjInteraction == null && Opponent.ObjInteraction == null)
                                {
                                    Opponent.ActiveDance = user.Player.ActiveDance = (ushort)Attack.ResponseDamage;
                                    Opponent.Owner.Send(stream.InteractionCreate(&Attack));
                                    Opponent.Owner.Send(stream.InteractionCreate(&Attack));
                                }
                            }
                            break;
                        }
                    case AttackID.InteractionRefuse:
                        {
                            user.Player.ActiveDance = 0;
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                Opponent.ActiveDance = 0;
                                Opponent.Owner.Send(stream.InteractionCreate(&Attack));
                            }
                            break;
                        }
                    case AttackID.InteractionAccept:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.ObjInteraction == null && Opponent.ObjInteraction == null)
                                {
                                    Attack.ResponseDamage = user.Player.ActiveDance;
                                    Opponent.Owner.Send(stream.InteractionCreate(&Attack));

                                    Attack.TimeStamp = 0;
                                    user.Send(stream.InteractionCreate(&Attack));

                                    user.Player.Action = (Role.Flags.ConquerAction)Attack.Damage;
                                    Opponent.Action = (Role.Flags.ConquerAction)Attack.Damage;

                                    user.Player.ObjInteraction = Opponent.Owner;
                                    Opponent.ObjInteraction = user;

                                }
                            }
                            break;
                        }
                    case AttackID.InteractionEffect:
                        {
                            if (user.Player.ObjInteraction != null)
                            {
                                if (user.Player.ObjInteraction.Player.ObjInteraction != null)
                                {
                                    Attack.ResponseDamage = user.Player.ActiveDance;
                                    Attack.TimeStamp = 0;
                                    CreateInteractionEffect(Attack, user);

                                    InteractQuery user_effect = user.Player.InteractionEffect;

                                    user.Player.View.SendView(stream.InteractionCreate(&user_effect), true);

                                    Attack.UID = user.Player.ObjInteraction.Player.UID;
                                    Attack.OpponentUID = user.Player.UID;

                                    CreateInteractionEffect(Attack, user.Player.ObjInteraction);

                                    user_effect = user.Player.ObjInteraction.Player.InteractionEffect;
                                    user.Player.ObjInteraction.Player.View.SendView(stream.InteractionCreate(&user_effect), true);
                                }
                            }
                            break;
                        }
                    case AttackID.InteractionStopEffect:
                        {
                            Attack.ResponseDamage = user.Player.ActiveDance;
                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                            Attack.UID = Attack.OpponentUID;
                            Attack.OpponentUID = user.Player.UID;
                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                            if (user.Player.ObjInteraction != null)
                            {
                                user.Player.OnInteractionEffect = false;
                                user.Player.Action = Role.Flags.ConquerAction.None;
                                user.Player.ObjInteraction.Player.OnInteractionEffect = false;
                                user.Player.ObjInteraction.Player.Action = Role.Flags.ConquerAction.None;
                                user.Player.ObjInteraction.Player.ObjInteraction = null;
                                user.Player.ObjInteraction = null;
                            }
                            break;
                        }
                    case AttackID.RequestMarriage:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.Spouse != "None" || Opponent.Spouse != "None")
                                {
                                    user.SendSysMesage("You can't marry someone that is already married to someone else!");
                                    break;
                                }
                                // if (user.Player.Body % 10 <= 2 && Opponent.Body % 10 >= 3 || user.Player.Body % 10 >= 2 && Opponent.Body % 10 <= 3)
                                {
                                    Attack.X = Opponent.X;
                                    Attack.Y = Opponent.Y;

                                    Opponent.Send(stream.InteractionCreate(&Attack));

                                }
                                //else
                                //{
                                //    user.SendSysMesage("You cannot marry someone of your gender!");
                                //}
                            }
                            break;
                        }
                    case AttackID.AcceptMarriage:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.Spouse != "None" || Opponent.Spouse != "None")
                                {
                                    user.SendSysMesage("You can't marry someone that is already married to someone else!");
                                    break;
                                }
                                // if (user.Player.Body % 10 <= 2 && Opponent.Body % 10 >= 3 || user.Player.Body % 10 >= 2 && Opponent.Body % 10 <= 3)
                                {
                                    user.Player.Spouse = Opponent.Name;
                                    user.Player.SpouseUID = Opponent.UID;

                                    Opponent.Spouse = user.Player.Name;
                                    Opponent.SpouseUID = user.Player.UID;

#if Arabic
                                     MsgMessage messaj = new MsgMessage("Congratulations! " + user.Player.Name + " and " + Opponent.Name + " have just got married!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
#else
                                    MsgMessage messaj = new MsgMessage("Congratulations! " + user.Player.Name + " and " + Opponent.Name + " have just got married!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
#endif

                                    Program.SendGlobalPackets.Enqueue(messaj.GetArray(stream));

                                    user.Player.SendString(stream, MsgStringPacket.StringID.Spouse, false, new string[1] { user.Player.Spouse });
                                    Opponent.SendString(stream, MsgStringPacket.StringID.Spouse, false, new string[1] { Opponent.Spouse });
                                    user.Player.SendString(stream, MsgStringPacket.StringID.Fireworks, true, new string[1] { "1122" });
                                    //firework-2love
                                    user.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { "firework-2love" });
                                }
                                //else
                                //{
                                //    user.SendSysMesage("You cannot marry someone of your gender!");
                                //}
                            }
                            break;
                        }
                    case AttackID.Archer:
                        {
                            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
                                break;
                            AttackHandler.Updates.GetWeaponSpell.CheckExtraEffects(user, stream);

                            Time32 Timer = Time32.Now;
                            Role.IMapObj target;
                            if (Timer < user.Player.AttackStamp.AddMilliseconds(650))
                            {
                                return;
                            }

                            user.Player.AttackStamp = Timer;

                            Game.MsgServer.MsgGameItem arrow = null;
                            if (!user.Equipment.FreeEquip(Role.Flags.ConquerItem.LeftWeapon))
                            {
                                user.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon, out arrow);
                                if (arrow.Durability <= 0)
                                    break;

                            }
                            else
                                break;

                            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.TryngEffect(user);

                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {

                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    Role.Player attacked = target as Role.Player;
                                    if (AttackHandler.CheckAttack.CanAttackPlayer.Verified(user, attacked, null, true))
                                    {
                                        MsgSpellAnimation.SpellObj AnimationObj;
                                        AttackHandler.Calculate.Range.OnPlayer(user.Player, attacked, null, out AnimationObj);

                                        Attack.Damage = (int)AnimationObj.Damage;
                                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true); // Not done yet i think it was Auto Attack xd 


                                        Attack.AtkType = AttackID.Archer;
                                        AttackHandler.ReceiveAttack.Player.Execute(AnimationObj, user, attacked);

                                        if (attacked.Alive)
                                            CreateAutoAtack(Attack, user);

                                        if (user.Player.Map != 1039)
                                        {
                                            arrow.Durability -= (ushort)Math.Min(arrow.Durability, (ushort)1);
                                            user.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateArrowCount, arrow.UID, arrow.Durability, 0, 0, 0, 0));
                                            if (arrow.Durability <= 0 /*<= Spell.UseArrows*/ || arrow.Durability > 10000)
                                                AttackHandler.CheckAttack.CanUseSpell.ReloadArrows(user.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon), user, stream);
                                        }

                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                                else
                                    user.OnAutoAttack = false;

                            }
                            else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                            {

                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    if (AttackHandler.CheckAttack.CanAttackMonster.Verified(user, attacked, null))
                                    {
                                        MsgSpellAnimation.SpellObj AnimationObj;
                                        AttackHandler.Calculate.Range.OnMonster(user.Player, attacked, null, out AnimationObj);



                                        Attack.Damage = (int)AnimationObj.Damage;
                                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                        Attack.AtkType = AttackID.Archer;
                                        AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked));
                                        AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                        if (attacked.Family.ID != 4145 && target.Alive)
                                            CreateAutoAtack(Attack, user);
                                        if (!user.Player.Robot)
                                        {
                                            if (user.Player.Map != 1039)
                                                arrow.Durability -= (ushort)Math.Min(arrow.Durability, (ushort)1);
                                            user.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateArrowCount, arrow.UID, arrow.Durability, 0, 0, 0, 0));
                                            if (arrow.Durability <= 0 /*<= Spell.UseArrows*/ || arrow.Durability > 10000)
                                                AttackHandler.CheckAttack.CanUseSpell.ReloadArrows(user.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon), user, stream);
                                        }
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                                else
                                    user.OnAutoAttack = false;

                            }
                            else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                            {
                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    var attacked = target as Role.SobNpc;
                                    if (AttackHandler.CheckAttack.CanAttackNpc.Verified(user, attacked, null))
                                    {
                                        MsgSpellAnimation.SpellObj AnimationObj;
                                        AttackHandler.Calculate.Range.OnNpcs(user.Player, attacked, null, out AnimationObj);

                                        Attack.Damage = (int)AnimationObj.Damage;
                                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                        Attack.AtkType = AttackID.Archer;
                                        if (target.Alive)
                                            CreateAutoAtack(Attack, user);

                                        AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked));
                                        AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);

                                        if (user.Player.Map != 1039)
                                        {
                                            arrow.Durability -= (ushort)Math.Min(arrow.Durability, (ushort)1);
                                            user.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateArrowCount, arrow.UID, arrow.Durability, 0, 0, 0, 0));
                                            if (arrow.Durability <= 0 /*<= Spell.UseArrows*/ || arrow.Durability > 10000)
                                                AttackHandler.CheckAttack.CanUseSpell.ReloadArrows(user.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon), user, stream);
                                        }
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                                else
                                    user.OnAutoAttack = false;

                            }
                            else
                                user.OnAutoAttack = false;

                            break;
                        }
                    case AttackID.CounterKillSwitch:
                        {

                            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
                                break;
                            Dictionary<ushort, Database.MagicType.Magic> Spells;
                            if (Database.Server.Magic.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out Spells))
                            {
                                MsgSpell ClientSpell;
                                if (user.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out ClientSpell))
                                {
                                    Database.MagicType.Magic spell;
                                    if (Spells.TryGetValue(ClientSpell.Level, out spell))
                                    {
                                        switch (spell.Type)
                                        {
                                            case Database.MagicType.MagicSort.CounterKill:
                                                {
                                                    Database.MagicType.Magic DBSpell;

                                                    Attack.SpellID = (ushort)Role.Flags.SpellID.CounterKill;
                                                    Attack.SpellLevel = ClientSpell.Level;

                                                    if (AttackHandler.CheckAttack.CanUseSpell.Verified(Attack, user, Spells, out ClientSpell, out DBSpell))
                                                    {
                                                        user.Player.ActivateCounterKill = !user.Player.ActivateCounterKill;
                                                        Attack.OnCounterKill = user.Player.ActivateCounterKill;
                                                        user.Send(stream.InteractionCreate(&Attack));
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case AttackID.Physical:
                        {
                            if (Attack.UID == user.Player.UID)
                            {
                                if (Program.ArenaMaps.ContainsKey(user.Player.DynamicID))
                                {
                                    user.SendSysMesage("You can't hit with melee.");
                                    return;
                                }

                                if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
                                {
                                    return;
                                }
                                AttackHandler.Updates.GetWeaponSpell.CheckExtraEffects(user, stream);


                                if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                                {
                                    InteractQuery AttackPaket = new InteractQuery();
                                    Role.IMapObj _target;
                                    if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                    {
                                        if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                        {
                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            if (user.OnAutoAttack)
                                            {
                                                user.Player.RandomSpell = AttackPaket.SpellID;
                                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                                                MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);
                                                if (_target.Alive)
                                                {

                                                    CreateAutoAtack(Attack, user);
                                                }
                                                else
                                                    user.OnAutoAttack = false;
                                                break;
                                            }

                                            if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, _target))
                                            {
                                                AttackPaket.UID = Attack.UID;
                                                AttackPaket.SpellID = Attack.SpellID;
                                                user.Player.RandomSpell = Attack.SpellID;

                                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                                MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);


                                                if (_target.Alive)
                                                {
                                                    //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                    CreateAutoAtack(Attack, user);
                                                }
                                                else
                                                    user.OnAutoAttack = false;

                                            }
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                        break;
                                    }
                                }

                                if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                                {

                                    InteractQuery AttackPaket = new InteractQuery();
                                    Role.IMapObj _target;
                                    if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                    {
                                        if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                        {
                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            if (user.OnAutoAttack)
                                            {
                                                List<ushort> CanUse = new List<ushort>();
                                                AttackPaket.SpellID = (ushort)CanUse[Program.GetRandom.Next(0, CanUse.Count)];
                                                user.Player.RandomSpell = AttackPaket.SpellID;
                                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                                MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);
                                                if (_target.Alive)
                                                {
                                                    //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                    CreateAutoAtack(Attack, user);
                                                }
                                                else
                                                    user.OnAutoAttack = false;
                                                break;
                                            }

                                            if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, _target))
                                            {

                                                AttackPaket.X = _target.X;
                                                AttackPaket.Y = _target.Y;
                                                AttackPaket.OpponentUID = _target.UID;
                                                AttackPaket.UID = Attack.UID;
                                                AttackPaket.SpellID = Attack.SpellID;
                                                user.Player.RandomSpell = Attack.SpellID;

                                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                                MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);

                                                if (_target.Alive)
                                                    CreateAutoAtack(Attack, user);
                                                else
                                                    user.OnAutoAttack = false;
                                            }
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                        break;
                                    }
                                }
                                if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                                {

                                    InteractQuery AttackPaket = new InteractQuery();
                                    Role.IMapObj _target;
                                    if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                    {
                                        if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                        {
                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            if (user.OnAutoAttack)
                                            {
                                                List<ushort> CanUse = new List<ushort>();
                                                AttackPaket.SpellID = (ushort)CanUse[Program.GetRandom.Next(0, CanUse.Count)];
                                                user.Player.RandomSpell = AttackPaket.SpellID;
                                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                                                MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);
                                                if (_target.Alive)
                                                {
                                                    //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                    CreateAutoAtack(Attack, user);
                                                }
                                                else
                                                    user.OnAutoAttack = false;
                                                break;
                                            }


                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            AttackPaket.OpponentUID = _target.UID;
                                            AttackPaket.UID = Attack.UID;
                                            AttackPaket.SpellID = Attack.SpellID;
                                            user.Player.RandomSpell = Attack.SpellID;

                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                            MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);

                                            if (_target.Alive)
                                                CreateAutoAtack(Attack, user);
                                            else
                                                user.OnAutoAttack = false;
                                            break;
                                        }
                                        else
                                            user.OnAutoAttack = false;

                                        break;
                                    }
                                }
                                if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                                {
                                    InteractQuery AttackPaket = new InteractQuery();
                                    Role.IMapObj _target;
                                    if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                        || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                    {
                                        if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                        {
                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            if (user.OnAutoAttack)
                                            {
                                                List<ushort> CanUse = new List<ushort>();
                                                if (user.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleAttack))
                                                    CanUse.Add((ushort)Role.Flags.SpellID.TripleAttack);
                                                AttackPaket.SpellID = (ushort)CanUse[Program.GetRandom.Next(0, CanUse.Count)];
                                                user.Player.RandomSpell = AttackPaket.SpellID;
                                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                                                MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);
                                                if (_target.Alive)
                                                {
                                                    //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                    CreateAutoAtack(Attack, user);
                                                }
                                                else
                                                    user.OnAutoAttack = false;
                                                break;
                                            }
                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            AttackPaket.OpponentUID = _target.UID;

                                            AttackPaket.UID = Attack.UID;

                                            if (AttackHandler.Calculate.Base.Success(30) && user.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleAttack))
                                            {
                                                AttackPaket.SpellID = (ushort)Role.Flags.SpellID.TripleAttack;
                                                user.Player.RandomSpell = AttackPaket.SpellID;
                                            }
                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                            MsgServer.MsgAttackPacket.ProcescMagic(user, stream, AttackPaket);


                                            if (_target.Alive)
                                                CreateAutoAtack(Attack, user);
                                            else
                                                user.OnAutoAttack = false;
                                            break;
                                        }
                                        else
                                            user.OnAutoAttack = false;

                                        break;
                                    }
                                }

                                Time32 Timer = Time32.Now;
                                Role.IMapObj target;

                                if (Timer < user.Player.AttackStamp.AddMilliseconds(user.Equipment.AttackSpeed(true)))
                                {
                                    return;
                                }
                                user.Player.AttackStamp = Timer;


                                MsgServer.AttackHandler.CheckAttack.CheckGemEffects.TryngEffect(user);
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {

                                    if (!user.Player.ContainFlag(MsgUpdate.Flags.Poisoned))
                                        if (user.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Poison
                                            || user.Equipment.LeftWeaponEffect == Role.Flags.ItemEffect.Poison)
                                        {
                                            if (Role.Core.Rate(10) || Attack.SpellID == (ushort)Role.Flags.SpellID.Poison)
                                            {
                                                AttackHandler.Poison.Execute(user, Attack, stream, null);
                                                return;
                                            }
                                        }
                                    if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, target))
                                    {
                                        if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 3)
                                        {
                                            Role.Player attacked = target as Role.Player;
                                            if (AttackHandler.CheckAttack.CanAttackPlayer.Verified(user, attacked, null))
                                            {
                                                Attack.TimeStamp = 0;

                                                MsgSpellAnimation.SpellObj AnimationObj;
                                                AttackHandler.Calculate.Physical.OnPlayer(user.Player, attacked, null, out AnimationObj);


                                                Attack.Damage = (int)AnimationObj.Damage;

                                                user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                                Attack.AtkType = AttackID.Physical;



                                                AttackHandler.ReceiveAttack.Player.Execute(AnimationObj, user, attacked);

                                                if (attacked.Alive)
                                                    CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                    }
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, target))
                                    {
                                        MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;

                                        if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= user.Equipment.GetAttackRange(attacked.SizeAdd) || user.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                        {

                                            if (AttackHandler.CheckAttack.CanAttackMonster.Verified(user, attacked, null))
                                            {
                                                MsgSpellAnimation.SpellObj AnimationObj;
                                                if (attacked.Family.ID == 4145)
                                                    Attack.SpellID = 0;
                                                Attack.TimeStamp = 0;

                                                if (user.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                                {
                                                    Attack.AtkType = AttackID.FatalStrike;
                                                    user.Shift(target.X, target.Y, stream);
                                                    AttackHandler.Calculate.Physical.OnMonster(user.Player, attacked, Database.Server.Magic[(ushort)Role.Flags.SpellID.FatalStrike][0], out AnimationObj);
                                                }
                                                else
                                                    AttackHandler.Calculate.Physical.OnMonster(user.Player, attacked, null, out AnimationObj);

                                                Attack.Damage = (int)AnimationObj.Damage;

                                                user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                                Attack.AtkType = AttackID.Physical;
                                                AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked));
                                                AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                                if (attacked.Alive || attacked.Family.ID == 41299)
                                                    CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                    }
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                                {

                                    if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, target))
                                    {
                                        if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 3)
                                        {
                                            var attacked = target as Role.SobNpc;
                                            if (AttackHandler.CheckAttack.CanAttackNpc.Verified(user, attacked, null))
                                            {
                                                Attack.TimeStamp = 0;

                                                MsgSpellAnimation.SpellObj AnimationObj;
                                                AttackHandler.Calculate.Physical.OnNpcs(user.Player, attacked, null, out AnimationObj);

                                                Attack.Damage = (int)AnimationObj.Damage;
                                                user.Player.View.SendView(stream.InteractionCreate(&Attack), true);


                                                Attack.AtkType = AttackID.Physical;

                                                AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked));
                                                AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                                if (attacked.Alive)
                                                    CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                    }

                                }
                                else
                                    user.OnAutoAttack = false;
                            }
                            else
                            {
                                if (user.Pet != null)
                                {
                                    Time32 Timer = Time32.Now;
                                    if (Timer < user.Pet.AttackStamp.AddMilliseconds(500))
                                    {
                                        return;
                                    }
                                    user.Pet.AttackStamp = Timer;
                                    Role.IMapObj target = null;
                                    if (user.Pet.Owner.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player)
                                        || user.Pet.Owner.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster)
                                            || user.Pet.Owner.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                                    {

                                        if (AttackHandler.Calculate.Base.GetDistance(user.Pet.monster.X, user.Pet.monster.Y, target.X, target.Y) <= user.Pet.Family.AttackRange)
                                        {
                                            Attack.TimeStamp = 0;

                                            // MsgSpellAnimation.SpellObj AnimationObj;
                                            // AttackHandler.Calculate.Physical.OnPlayer(user.Pet.Owner.Player, attacked, null, out AnimationObj);

                                            Attack.X = target.X;
                                            Attack.Y = target.Y;
                                            Attack.OpponentUID = target.UID;
                                            Attack.SpellID = (ushort)user.Pet.Family.SpellId;
                                            Attack.AtkType = MsgAttackPacket.AttackID.Magic;
                                            MsgServer.MsgAttackPacket.ProcescMagic(user, stream, Attack);
                                            //Attack.SpellID = (ushort)user.Pet.Family.SpellId;
                                            //Attack.SpellLevel = 1;
                                            //Attack.Damage = (int)185;
                                            //var spelluse = new MsgSpellAnimation(user.Pet.monster.UID, Attack.OpponentUID, Attack.X, Attack.Y, (ushort)user.Pet.Family.SpellId, 1, 0);
                                            //spelluse.Targets.Enqueue(new MsgSpellAnimation.SpellObj(Attack.OpponentUID, 185));
                                            //spelluse.SetStream(stream);
                                            //spelluse.Send(user.Pet.monster);
                                            //spelluse.Send(user.Pet.Owner);
                                            //Attack.AtkType = AttackID.Magic;
                                            //user.Pet.Owner.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                            //Attack.AtkType = AttackID.Magic;

                                            //AttackHandler.ReceiveAttack.Player.ExecutePet(185, user, attacked);
                                        }
                                        else
                                            user.Pet.Owner.OnAutoAttack = false;
                                    }
                                }
                            }
                            break;
                        }
                    case AttackID.Magic:
                        {

                            ProcescMagic(user, stream, Attack);
                            break;
                        }
                }
            }


        }
        public static void ProcescMagic(Client.GameClient user, ServerSockets.Packet stream, InteractQuery Attack, bool ignoreStamp = false)
        {

            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
            {
                return;
            }
            if (user.Player.Map == 1036)
            {
                if (Attack.SpellID != (ushort)Role.Flags.SpellID.Bless)
                {
                    return;
                }
            }
            if (user.Player.Map == 1601)
            {
                if (Attack.SpellID != (ushort)Role.Flags.SpellID.Bless)
                {
                    return;
                }
            }
            if (user.Player.OnTransform)
                return;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.TryngEffect(user);
            if (!EventsLib.EventManager.ExecuteSkill((uint)Attack.AtkType, Attack.SpellID, user))
                return;
            bool OnTGAutoAttack = true;
            Dictionary<ushort, Database.MagicType.Magic> Spells;
            if (Database.Server.Magic.TryGetValue(Attack.SpellID, out Spells))
            {
                Database.MagicType.Magic spell;
                if (Spells.TryGetValue(Attack.SpellLevel, out spell))
                {
                    var Timer = Time32.Now;
                    if (spell.CoolDown == 0)
                        spell.CoolDown = 600;

                    if (ignoreStamp == false)
                    {
                        if (spell.CoolDown > 1000 && spell.CoolDown < 3000)
                            spell.CoolDown = 800;

                        else if (Timer < user.Player.SpellAttackStamp.AddMilliseconds(spell.CoolDown)
                            && spell.ID != (ushort)Role.Flags.SpellID.FastBlader && spell.ID != (ushort)Role.Flags.SpellID.ScrenSword
                            && spell.ID != (ushort)Role.Flags.SpellID.ViperFang)
                        {

                            return;
                        }
                        user.Player.SpellAttackStamp = Timer;
                    }
                    if (user.OnAutoAttack == false)
                    {
                        if (user.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.MP || user.Equipment.RingEffect == Role.Flags.ItemEffect.MP)
                        {
                            if (spell != null && spell.UseMana > 0)
                            {
                                if (Attack.SpellID == (ushort)Role.Flags.SpellID.EffectMP || AttackHandler.Calculate.Base.Success(30))
                                {
                                    user.Player.RandomSpell = 1175;
                                    AttackHandler.EffectMP.Execute(Attack, user, stream, Spells);

                                }
                            }
                        }
                        AttackHandler.Updates.GetWeaponSpell.CheckExtraEffects(user, stream);


                    }
                    switch (spell.Type)
                    {
                        case Database.MagicType.MagicSort.Square:
                            Console.WriteLine("565555555");
                            break;
                        case Database.MagicType.MagicSort.AddMana:
                            AttackHandler.AddMana.Execute(user, Attack, stream, Spells); break;

                        case Database.MagicType.MagicSort.PhysicalSpells:
                            AttackHandler.PhysicalSpells.Execute(user, Attack, stream, Spells); break;

                        case Database.MagicType.MagicSort.DispatchXp:
                            AttackHandler.DispatchXp.Execute(user, Attack, stream, Spells); break;

                        case Database.MagicType.MagicSort.ShieldBlock:
                            AttackHandler.ShieldBlock.Execute(user, Attack, stream, Spells); break;

                        case Database.MagicType.MagicSort.Compasion:
                        case Database.MagicType.MagicSort.Tranquility:
                        case Database.MagicType.MagicSort.RemoveBuffers:
                            {
                                AttackHandler.RemoveBuffers.Execute(user, Attack, stream, Spells);
                                break;
                            }
                        case Database.MagicType.MagicSort.Perimeter:
                            AttackHandler.Perimeter.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.DirectAttack:
                            AttackHandler.DirectAttack.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.DecLife:
                            AttackHandler.DecLife.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Transform:
                            {
                                if (spell.ID == (ushort)Role.Flags.SpellID.Golem || spell.ID == (ushort)Role.Flags.SpellID.WaterElf
                                    || spell.ID == (ushort)Role.Flags.SpellID.NightDevil)
                                    OnTGAutoAttack = false;
                                if ((MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive))
                                {
                                    if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                                    {
                                        return;
                                    }
                                }
                                AttackHandler.Transform.Execute(user, Attack, stream, Spells); break;
                            }
                        case Database.MagicType.MagicSort.AttackStatus:
                            AttackHandler.AttackStatus.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Collide:
                            AttackHandler.Collide.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Sector:
                            AttackHandler.Sector.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Line:
                        case Database.MagicType.MagicSort.LINE_PENETRABLE:
                            AttackHandler.Line.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Attack:
                            AttackHandler.Attack.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.AttachStatus:
                            {
                                if (spell.ID == (ushort)Role.Flags.SpellID.MagicShield && user.Player.Map == 1039)//Disable Shield skill in TG. (Shield Spell ID: 1090)
                                    break;
                                if (spell.ID == (ushort)Role.Flags.SpellID.Stigma || spell.ID == (ushort)Role.Flags.SpellID.StarofAccuracy
                                     || spell.ID == (ushort)Role.Flags.SpellID.Invisibility || spell.ID == (ushort)Role.Flags.SpellID.Cure
                                      || spell.ID == (ushort)Role.Flags.SpellID.AdvancedCure)
                                    OnTGAutoAttack = true;
                                else
                                    OnTGAutoAttack = false;
                                AttackHandler.AttachStatus.Execute(user, Attack, stream, Spells); break;
                            }
                        case Database.MagicType.MagicSort.DetachStatus:
                            AttackHandler.DetachStatus.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Recruit:
                            AttackHandler.Recruit.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Bomb:
                            {
                                if (spell.ID == (ushort)Role.Flags.SpellID.Vulcano || spell.ID == (ushort)Role.Flags.SpellID.Lightning
                                    || spell.ID == (ushort)Role.Flags.SpellID.SpeedLightning)
                                    OnTGAutoAttack = false;

                                AttackHandler.Bomb.Execute(user, Attack, stream, Spells); break;
                            }
                        case Database.MagicType.MagicSort.Pounce:
                            AttackHandler.Pounce.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.CallPet:
                            {
                                //Players shouldn't be able to summon guard(Spell ID: 4000) or summon pets (Spell IDs: 4010, 4020, 4050, 4060, and 4070) inside Training Grounds. Video on desktop.
                                if (user.Player.Map != 1039)   // OnTGAutoAttack = false;
                                    AttackHandler.SummonGuard.Execute(user, Attack, stream, Spells); break;
                            }
                    }
                }
            }
            if (user.Player.Map == 1039 && OnTGAutoAttack)
            {

                // EncodeMagicAttack(&Attack);
                if (Database.MagicType.RandomSpells.Contains((Role.Flags.SpellID)Attack.SpellID))//يرجع الاسكل لphysical jh
                {
                    Attack.AtkType = AttackID.Physical; Attack.SpellID = 0; Attack.Damage = 0; Attack.SpellLevel = 0;
                }
                CreateAutoAtack(Attack, user);
            }
        }

        public static void CreateAutoAtack(InteractQuery pQuery, Client.GameClient client)
        {
            client.OnAutoAttack = true;
            client.AutoAttack = new InteractQuery();
            client.AutoAttack.AtkType = pQuery.AtkType;
            client.AutoAttack.Damage = pQuery.Damage;
            //client.AutoAttack.Data = pQuery.Data;
            client.AutoAttack.dwParam = pQuery.dwParam;
            client.AutoAttack.OpponentUID = pQuery.OpponentUID;
            client.AutoAttack.ResponseDamage = pQuery.ResponseDamage;
            client.AutoAttack.SpellID = pQuery.SpellID;
            client.AutoAttack.SpellLevel = pQuery.SpellLevel;
            client.AutoAttack.UID = pQuery.UID;
            client.AutoAttack.X = pQuery.X;
            client.AutoAttack.Y = pQuery.Y;

        }

        public static void CreateInteractionEffect(InteractQuery pQuery, Client.GameClient client)
        {
            client.Player.OnInteractionEffect = true;

            client.Player.InteractionEffect = new InteractQuery();
            client.Player.InteractionEffect.AtkType = pQuery.AtkType;
            client.Player.InteractionEffect.Damage = pQuery.Damage;
            //    client.Player.InteractionEffect.Data = pQuery.Data;
            client.Player.InteractionEffect.dwParam = pQuery.dwParam;
            client.Player.InteractionEffect.OpponentUID = pQuery.OpponentUID;
            client.Player.InteractionEffect.ResponseDamage = pQuery.ResponseDamage;
            client.Player.InteractionEffect.SpellID = pQuery.SpellID;
            client.Player.InteractionEffect.SpellLevel = pQuery.SpellLevel;
            client.Player.InteractionEffect.UID = pQuery.UID;
            client.Player.InteractionEffect.X = pQuery.X;
            client.Player.InteractionEffect.Y = pQuery.Y;
        }

    }
}
