namespace COServer.Game.MsgServer.AttackHandler.Updates
{
    public class IncreaseExperience
    {
        public unsafe static void Up(ServerSockets.Packet stream, Client.GameClient user, uint Damage)
        {
            if (Damage == 0)
                return;
           // Damage = 1;
            user.IncreaseExperience(stream, Damage);

            if (user.Player.HeavenBlessing > 0)
            {
                user.Player.HuntingBlessing += Damage / 10;
            }

        }

    }
}
