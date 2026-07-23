namespace AccServer.Interfaces
{
    public unsafe interface IPacket
    {
        byte[] ToArray();
        void Deserialize(byte[] buffer);
    }
}