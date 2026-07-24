namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet HeroInfo(this ServerSockets.Packet stream, Role.Player client, int inittransfer = 0)
        {
            stream.InitWriter();
            stream.Write(client.UID);//4
            stream.Write(client.Mesh);//10
            stream.Write(client.Hair);//12
            stream.Write((uint)client.Money);//14
            stream.Write(client.ConquerPoints);//18
            stream.Write(client.Experience);//22
            stream.Write(client.SetLocationType);//30
            stream.Write((uint)0);//32
            stream.Write((ushort)0);//36
            stream.Write((uint)0);//38
            stream.Write(client.VirtutePoints);//42
            stream.Write(client.HeavenBlessing);//forinterserver//46
            stream.Write(client.Strength);//50
            stream.Write(client.Agility);//52
            stream.Write(client.Vitality);//54
            stream.Write(client.Spirit);//56
            stream.Write(client.Atributes);//58
            stream.Write((ushort)client.HitPoints);//60
            stream.Write(client.Mana);
            stream.Write(client.PKPoints);
            stream.Write((byte)client.Level);
            stream.Write(client.Class);
            stream.Write(client.FirstClass);
            stream.Write(client.Reborn);
            stream.Write((byte)0);
            stream.Write(client.QuizPoints);
            stream.Write(client.Name, client.Spouse);
            stream.Finalize(GamePackets.HeroInfo);
            return stream;
        }

    }

}
