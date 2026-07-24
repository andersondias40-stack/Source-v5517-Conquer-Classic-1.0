namespace COServer.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Npc
    {
        public static uint Execute(ServerSockets.Packet stream, MsgSpellAnimation.SpellObj obj, Client.GameClient client, Role.SobNpc attacked)
        {
            if (client.Pet != null) client.Pet.Target = attacked;
            if (obj.Damage >= attacked.HitPoints)
            {
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.GuildWar.PoleFundHolder(client.Player, obj.Damage, (uint)attacked.HitPoints, false);

                uint exp = (uint)attacked.HitPoints;

                attacked.Die(stream, client);

                if (attacked.UID == 7811)
                {
                    Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, attacked.UID, 2);
                    stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, (long)attacked.HitPoints);
                    stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, (long)attacked.MaxHitPoints);
                    stream = upd.GetArray(stream);
                    client.Player.View.SendView(stream, true);
                }
                if (obj.UID >= 7811 && obj.UID <= 7814)
                {
                    if (obj.Damage >= attacked.HitPoints)
                    {
                        attacked.Die(stream, client);
                        client.Player.ConquerPoints += 1000;
                        if (attacked.Map == 1002)
                            return exp / 10;
                    }
                }
                if (attacked.Map == 1039)
                    return exp;
            }
            else
            {

                attacked.HitPoints -= (int)obj.Damage;
                if (attacked.UID == Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.UpdateScore(client.Player, obj.Damage);
                if (attacked.UID == Game.MsgTournaments.MsgSchedules._EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules._EliteGuildWar.UpdateScore(client.Player, obj.Damage);
                //if (attacked.UID == Game.MsgTournaments.MsgSchedules._FirePoleWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                //    Game.MsgTournaments.MsgSchedules._FirePoleWar.UpdateScore(client.Player, obj.Damage);
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.GuildWar.UpdateScore(client.Player, obj.Damage);

                //if (Game.MsgTournaments.MsgSchedules.CityWar.Process == MsgTournaments.ProcesType.Alive)
                //{
                //    if (Game.MsgTournaments.MsgSchedules.CityWar.CurentWar != null && Game.MsgTournaments.MsgSchedules.CityWar.CurentWar.InWar(client))
                //    {
                //        if (Game.MsgTournaments.MsgSchedules.CityWar.CurentWar.Proces == MsgTournaments.ProcesType.Alive)
                //        {
                //            Game.MsgTournaments.MsgSchedules.CityWar.CurentWar.UpdateScore(client.Player, obj.Damage);
                //        }
                //    }
                //}
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.GuildWar.PoleFundHolder(client.Player, obj.Damage, (uint)attacked.HitPoints, true);

                    if (attacked.Map == 1039 || attacked.Map == 1038|| attacked.Map == 1002)
                    return obj.Damage;

            }
            return 0;//obj.Damage;
        }
    }
}
