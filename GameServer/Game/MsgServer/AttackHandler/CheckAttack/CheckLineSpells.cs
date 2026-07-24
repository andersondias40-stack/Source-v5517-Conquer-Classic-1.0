namespace COServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CheckLineSpells
    {
        public static bool CheckUp(Client.GameClient user, ushort spellid)
        {
            if (Program.ArenaMaps.ContainsValue(user.Player.DynamicID))
            {
                if (spellid != 1045 && spellid != 1046 && spellid != 1047 && spellid != 11000)
                {
                    user.SendSysMesage("You can only use FastBlade/ScentSword here.");
                    return false;
                }
            }
            if ((MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FiveNOut) && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
            {
                if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                {
                    if (spellid != 1045 && spellid != 1046 && spellid != 1047 && spellid != 11000)
                    {
                        user.SendSysMesage("You can only use FastBlade/ScentSword here.");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
