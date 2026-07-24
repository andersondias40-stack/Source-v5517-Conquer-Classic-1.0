using System;
using System.Linq;
namespace COServer.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Monster
    {
        public static uint ExecutePet(ServerSockets.Packet stream, uint obj, Client.GameClient client, MsgMonster.MonsterRole monster, bool CounterKill = false)
        {
            if (monster.HitPoints <= obj)
            {
                client.Map.SetMonsterOnTile(monster.X, monster.Y, false);
                monster.Dead(stream, client, client.Player.UID, client.Map, CounterKill);
                if (monster.UID >= 700000)
                {
                    var array = Database.Server.GamePoll.Values.Where(c => c.Pet != null && c.Pet?.monster.UID == monster.UID).SingleOrDefault();
                    if (array != null)
                        array.Pet.DeAtach(stream);
                }
            }
            else
            {
                monster.HitPoints -= obj;

                if (monster.Boss == 1)
                {
                    Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(stream, monster.UID, 2);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, monster.Family.MaxHealth);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, monster.HitPoints);
                    stream = Upd.GetArray(stream);
                    client.Player.View.SendView(stream, true);
                    monster.SendScores(stream);
                    monster.UpdateScores(client.Player, obj);
                }
            }

            if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) != MsgMonster.MonsterSettings.Guard)
            {
                if (obj > monster.Family.MaxHealth)
                    return (uint)AdjustExp(monster.Family.MaxHealth, client.Player.Level, monster.Level);
                else
                    return (uint)AdjustExp((int)obj, client.Player.Level, monster.Level);
            }
            else
                return 0;
        }

        public static uint Execute(ServerSockets.Packet stream, MsgSpellAnimation.SpellObj obj, Client.GameClient client, MsgMonster.MonsterRole monster, bool CounterKill = false)
        {
            if (client.Pet != null) client.Pet.Target = monster;
            if (monster.HitPoints <= obj.Damage)
            {
                client.Map.SetMonsterOnTile(monster.X, monster.Y, false);
                monster.Dead(stream, client, client.Player.UID, client.Map, CounterKill);
                var target = Database.Server.GamePoll.Values.Where(p => p.Pet != null && p.Pet.monster.UID == monster.UID).SingleOrDefault();
                if (target?.Pet != null)
                    target.Pet.DeAtach(stream);

                if (client.Team != null)
                    client.Team.ShareExperience(stream, client, monster);
            }
            else
            {
                monster.HitPoints -= obj.Damage;

                if (monster.Boss == 1)
                {
                    Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(stream, monster.UID, 2);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, monster.Family.MaxHealth);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, monster.HitPoints);
                    stream = Upd.GetArray(stream);
                    client.Player.View.SendView(stream, true);
                    monster.SendScores(stream);
                    monster.UpdateScores(client.Player, obj.Damage);
                }
            }

            if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) != MsgMonster.MonsterSettings.Guard)
            {
                if (obj.Damage > monster.Family.MaxHealth)
                    return (uint)AdjustExp(monster.Family.MaxHealth, client.Player.Level, monster.Level);
                else
                    return (uint)AdjustExp((int)obj.Damage, client.Player.Level, monster.Level);
            }
            else
                return 0;
        }
        public static int AdjustExp(int nDamage, int nAtkLev, int nDefLev)
        {
            //   return nDamage;
            if (nAtkLev > 120)
                nAtkLev = 120;

            int nExp = nDamage;

            int nNameType = Calculate.Base.GetNameType(nAtkLev, nDefLev);
            int nDeltaLev = nAtkLev - nDefLev;
            if (nNameType == Calculate.Base.StatusConstants.NAME_GREEN)
            {
                if (nDeltaLev >= 3 && nDeltaLev <= 5)
                    nExp = nExp * 70 / 100;
                else if (nDeltaLev > 5 && nDeltaLev <= 10)
                    nExp = nExp * 20 / 100;
                else if (nDeltaLev > 10 && nDeltaLev <= 20)
                    nExp = nExp * 10 / 100;
                else if (nDeltaLev > 20)
                    nExp = nExp * 5 / 100;
            }
            else if (nNameType == Calculate.Base.StatusConstants.NAME_RED)
            {
                nExp = (int)(nExp * 1.3);
            }
            else if (nNameType == Calculate.Base.StatusConstants.NAME_BLACK)
            {
                if (nDeltaLev >= -10 && nDeltaLev < -5)
                    nExp = (int)(nExp * 1.5);
                else if (nDeltaLev >= -20 && nDeltaLev < -10)
                    nExp = (int)(nExp * 1.8);
                else if (nDeltaLev < -20)
                    nExp = (int)(nExp * 2.3);
            }

            return Math.Max(1, nExp);//10
        }
    }
}
