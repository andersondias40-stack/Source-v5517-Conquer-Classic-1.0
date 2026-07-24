using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class Collide
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                Role.Flags.ConquerAngle direction = (Role.Flags.ConquerAngle)Attack.OpponentUID;
                //  var direction = Role.Core.GetAngle(user.Player.X, user.Player.Y, Attack.X, Attack.Y);
                Attack.AtkType = MsgAttackPacket.AttackID.Dash;
                Attack.TimeStamp = 0;
                Attack.SpellID = ClientSpell.ID;
                Attack.SpellLevel = ClientSpell.Level;

                ushort awalkX = Attack.X, awalkY = Attack.Y;
                Role.Core.IncXY(direction, ref awalkX, ref awalkY);
                user.Map.View.MoveTo<Role.IMapObj>(user.Player, awalkX, awalkY);
                user.Player.X = awalkX;
                user.Player.Y = awalkY;
                user.Player.View.Role(false, null);



                Role.IMapObj target;
                //        if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))

                if (user.Player.View.SameLocation(Role.MapObjectType.Player, out target))
                {
                    Role.Player attacked = target as Role.Player;

                    if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                    {
                        var direction2 = Role.Core.GetAngle(attacked.X, attacked.Y, user.Player.X, user.Player.Y);
                        MsgSpellAnimation.SpellObj AnimationObj;
                        Calculate.Physical.OnPlayer(user.Player, attacked, DBSpell, out AnimationObj);
                        ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
                        AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
                        Attack.Damage = (int)AnimationObj.Damage;

                        Attack.X = attacked.X;
                        Attack.Y = attacked.Y;
                        Attack.OpponentUID = target.UID;
                        ushort walkX = attacked.X, walkY = attacked.Y;
                        Role.Core.IncXY(direction2, ref walkX, ref walkY);
                        user.Map.View.MoveTo<Role.IMapObj>(attacked, walkX, walkY);
                        attacked.X = walkX;
                        attacked.Y = walkY;
                        attacked.View.Role(false, null);
                        Attack.OpponentUID = attacked.UID;
                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);
                    }
                }
                else if (user.Player.View.SameLocation(Role.MapObjectType.Monster, out target))
                {
                    Game.MsgMonster.MonsterRole attacked = target as Game.MsgMonster.MonsterRole;
                    if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
                    {
                        ushort walkX = attacked.X, walkY = attacked.Y;
                        Role.Core.IncXY(direction, ref walkX, ref walkY);
                        user.Map.View.MoveTo<Role.IMapObj>(attacked, walkX, walkY);
                        attacked.X = walkX;
                        attacked.Y = walkY;

                        MsgSpellAnimation.SpellObj AnimationObj;
                        Calculate.Physical.OnMonster(user.Player, attacked, DBSpell, out AnimationObj);
                        ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
                        AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
                        Attack.Damage = (int)AnimationObj.Damage;
                        Attack.OpponentUID = attacked.UID;
                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                        return;
                    }
                }


            }
        }
    }
}
