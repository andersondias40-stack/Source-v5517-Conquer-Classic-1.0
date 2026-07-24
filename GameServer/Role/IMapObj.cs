namespace COServer.Role
{
    public interface IMapObj
    {
        string Name { get; set; }
        bool AllowDynamic { get; set; }
        uint UID { get; }
        ushort X { get; }
        ushort Y { get; }
        uint Map { get; }
        uint DynamicID { get; }
        bool Alive { get; }
        bool IsTrap();
        unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool View);
        uint IndexInScreen { get; set; }
        MapObjectType ObjType { get; }
        unsafe void Send(ServerSockets.Packet msg);
    }
}
