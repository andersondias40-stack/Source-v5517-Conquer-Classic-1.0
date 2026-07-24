namespace COServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CanAttackNpc
    {
        public static bool Verified(Client.GameClient client, Role.SobNpc attacked
     , Database.MagicType.Magic DBSpell)
        {

            if (attacked.Name == "BoxerHuang" || attacked.UID == 180)
            {
                return false;
            }
            #region Scarecrow/Stake //الاسكلات بتشتغل على الاسكارو
            if (client.Player.Map == 1039 || client.Player.Map == 1002)
            {
                if (attacked.Type == Role.Flags.NpcType.Stake && DBSpell != null)
                    return true;

                if (attacked.HitPoints == 0)
                    return true;

                ushort levelbase = (ushort)((ushort)attacked.Mesh / 10);
                if (attacked.Type == Role.Flags.NpcType.Stake)
                    levelbase -= 42;
                else
                    levelbase -= 43;

                byte level = (byte)(20 + (levelbase / 3) * 5);
                if (levelbase == 108 || levelbase == 109)
                    level = 125;
                if (client.Player.Level >= level)
                    return true;
                else
                {                    
                    client.SendSysMesage("You can't attack this dummy because your level is not high enough.");  
                    return false;
                }
            }
            #endregion
            if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.RightGate].UID
                || attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.LeftGate].UID
                && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Dead && attacked.HitPoints == 0)
                return false;
            else
            if (attacked.HitPoints == 0)
                return false;
            //if (client.Player.OnTransform)
            //    return false;
            if (attacked.IsStatue)
            {
                if (attacked.HitPoints == 0)
                    return false;
                if (client.Player.PkMode == Role.Flags.PKMode.PK)
                    return true;
                else
                    return false;
            }
            /* if (client.Player.Map == 1039)
             {
                 if (attacked.Type == Role.Flags.NpcType.Stake || attacked.Type == Role.Flags.NpcType.Scarecrow)
                 {
                     if (attacked.UID >= 5000 && attacked.UID <= 5020 || attacked.UID >= 6000 && attacked.UID <= 6020)//lvl 20
                     {
                         if (client.Player.Level == 20)
                             return true;
                         else return false;
                     }
                     if (attacked.UID >= 5021 && attacked.UID <= 5041 || attacked.UID >= 6021 && attacked.UID <= 6041)//lvl 25
                     {
                         if (client.Player.Level == 25)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5042 && attacked.UID <= 5062 || attacked.UID >= 6042 && attacked.UID <= 6062)//lvl 30
                     {
                         if (client.Player.Level == 30)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5063 && attacked.UID <= 5083 || attacked.UID >= 6063 && attacked.UID <= 6083)//lvl 35
                     {
                         if (client.Player.Level == 35)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5048 && attacked.UID <= 5104 || attacked.UID >= 6048 && attacked.UID <= 6104)//lvl 40
                     {
                         if (client.Player.Level == 40)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5105 && attacked.UID <= 5125 || attacked.UID >= 6105 && attacked.UID <= 6125)//lvl 45
                     {
                         if (client.Player.Level == 45)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5126 && attacked.UID <= 5146 || attacked.UID >= 6126 && attacked.UID <= 6146)//lvl 50
                     {
                         if (client.Player.Level == 50)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5147 && attacked.UID <= 5167 || attacked.UID >= 6147 && attacked.UID <= 6167)//lvl 55
                     {
                         if (client.Player.Level == 55)
                             return true;
                         else return false;
                     }

                     if (attacked.UID >= 5147 && attacked.UID <= 5167 || attacked.UID >= 6147 && attacked.UID <= 6167)//lvl 55
                     {
                         if (client.Player.Level == 55)
                             return true;
                         else return false;
                     }


               //      Console.WriteLine(attacked.UID);
                 }
             }*/
            #region _ExtremeFlagWar
            if (attacked.UID == Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.IsFinished())
                    return false;
            }
            #endregion
            #region _EliteGuildWar
            if (attacked.UID == Game.MsgTournaments.MsgSchedules._EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules._EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules._EliteGuildWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules._EliteGuildWar.IsFinished())
                    return false;
            }
            #endregion
            #region _FirePoleWar
            //if (attacked.UID == Game.MsgTournaments.MsgSchedules._FirePoleWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            //{
            //    if (client.Player.MyGuild == null)
            //        return false;
            //    if (Game.MsgTournaments.MsgSchedules._FirePoleWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
            //        return false;
            //    if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules._FirePoleWar.Winner.GuildID)
            //        return false;
            //    if (Game.MsgTournaments.MsgSchedules._FirePoleWar.IsFinished())
            //        return false;
            //}
            #endregion

            if (attacked.UID == 890)
            {
                //if (client.Player.MyGuild == null)
                //    return false;
                ////var tournament = Game.MsgTournaments.MsgSchedules.CityWar.CurentWar;
                //if (tournament == null)
                //    return false;

                //if (!tournament.InWar(client))
                //    return false;
                //if (tournament.Winner == null)
                //    return false;
                //if (tournament.Winner.GuildId == client.Player.GuildID)
                //    return false;

            }
            if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (client.Player.MyGuild.Info.SilverFund < 50000)
                {
                    client.SendSysMesage("Your Guild fund is low, please donate to attack the pole.");
                    return false;

                }
             
                if (Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Idle)
                    return false;
            }
            return true;
        }
    }
}
