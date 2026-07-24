using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler.Calculate
{
    public class BackDmg
    {
        public unsafe static bool Calculate(Role.Player player, Role.Player target, Database.MagicType.Magic DBSpell, uint Damage, out MsgSpellAnimation.SpellObj SpellObj)
        {

            if (player.Alive == false)
            {
                SpellObj = default(MsgSpellAnimation.SpellObj);
                return false;
            }
          if (Base.Success(5))
            {
                if (target.ActivateCounterKill)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        MsgSpell ClientSpell;
                        if (target.Owner.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out ClientSpell))
                        {
                            Dictionary<ushort, Database.MagicType.Magic> DBSpells;
                            if (Database.Server.Magic.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out DBSpells))
                            {
                                if (DBSpells.TryGetValue(ClientSpell.Level, out DBSpell))
                                {
                                    SpellObj = new MsgSpellAnimation.SpellObj();
                                    SpellObj.Damage = 0;

                                    MsgSpellAnimation.SpellObj DmgObj = new MsgSpellAnimation.SpellObj();
                                    Physical.OnPlayer(target, player, DBSpell, out DmgObj, true);
                                    DmgObj.Damage = (uint)(DmgObj.Damage * 0.75);

                                    //update spell
                                    if (ClientSpell.Level < DBSpells.Count - 1)
                                    {
                                        ClientSpell.Experience += (int)(DmgObj.Damage * Program.ServerConfig.ExpRateSpell);
                                        if (ClientSpell.Experience > DBSpells[ClientSpell.Level].Experience)
                                        {
                                            ClientSpell.Level++;
                                            ClientSpell.Experience = 0;
                                        }
                                        target.Send(stream.SpellCreate(ClientSpell));
                                        target.Owner.MySpells.ClientSpells[ClientSpell.ID] = ClientSpell;
                                    }


                                    InteractQuery action = new InteractQuery()
                                    {
                                        ResponseDamage = DmgObj.Damage,
                                        X = player.X,
                                        Y = player.Y,
                                        OpponentUID = player.UID,
                                        UID = target.UID,
                                        AtkType = MsgAttackPacket.AttackID.Scapegoat
                                    };

                                    target.View.SendView(stream.InteractionCreate(&action), true);



                                    ReceiveAttack.Player.Execute(DmgObj, target.Owner, player);

                                    return true;
                                }
                            }
                        }
                    }
                }
                if (target.ContainReflect)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SpellObj = new MsgSpellAnimation.SpellObj();
                        SpellObj.Damage = 0;
                        SpellObj.UID = target.UID;

                        MsgSpellAnimation.SpellObj DmgObj = new MsgSpellAnimation.SpellObj();
                        //Physical.OnPlayer(player, target, DBSpell, out DmgObj, true);
                        DmgObj.Damage = Damage /10;
                        DmgObj.UID = player.UID;

                        InteractQuery action = new InteractQuery()
                        {
                            ResponseDamage = DmgObj.Damage,
                            Damage = (int)DmgObj.Damage,
                            AtkType = MsgAttackPacket.AttackID.Reflect,
                            X = player.X,
                            Y = player.Y,
                            OpponentUID = player.UID,
                            UID = target.UID
                        };

                        target.View.SendView(stream.InteractionCreate(&action), true);
                        ReceiveAttack.Player.Execute(DmgObj, target.Owner, player);
                    }
                    return true;
                }
            }
            SpellObj = default(MsgSpellAnimation.SpellObj);
            return false;
        }

    }
}
