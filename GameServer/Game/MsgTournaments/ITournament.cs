namespace COServer.Game.MsgTournaments
{
    public interface ITournament
    {
        ProcesType Process { get; set; }
        TournamentType Type { get; set; }
        void Open();
        bool Join(Client.GameClient user, ServerSockets.Packet stream);
        void CheckUp();
        bool InTournament(Client.GameClient user);
    }
}
