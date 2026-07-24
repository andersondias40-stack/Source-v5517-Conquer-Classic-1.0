using COServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.Role.Instance
{
    public unsafe class EffectStatus
    {
        public class Info
        {
            public string Name;
            public DateTime ExpireDate;
        }
        public Client.GameClient Owner;
        public ConcurrentDictionary<string, Info> EffectsPoll = new ConcurrentDictionary<string, Info>();
        public EffectStatus(Client.GameClient owner)
        {
            Owner = owner;
        }

        public ServerSockets.Packet CreateEffectPacket(string effect)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
                packet.ID = MsgStringPacket.StringID.Effect;
                packet.UID = Owner.Player.UID;
                packet.Strings = new string[] { effect };
                return stream.StringPacketCreate(packet);
            }
        }

        public void Add(string name, DateTime time)
        {
            if (EffectsPoll.TryGetValue(name, out var effect))
                effect.ExpireDate = time;
            else EffectsPoll.Add(name, new Info() { Name = name, ExpireDate = time });
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Owner.Player.View.SendView(CreateEffectPacket(name), true);
            }
        }

        public void Remove(string name)
        {
            if (EffectsPoll.TryRemove(name, out var effect))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    ActionQuery action = new ActionQuery()
                    {
                        ObjId = Owner.Player.UID,
                        Type = ActionType.RemoveEntity
                    };
                    Owner.Player.View.SendView(stream.ActionCreate(&action), false);
                    Owner.Player.View.SendView(Owner.Player.GetArray(stream, false), false);
                }
            }
        }

        public void Reload(Client.GameClient newPlayerOnScreen)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var effect in EffectsPoll.Values)
                {
                    if (effect.ExpireDate > DateTime.Now)
                    {
                        newPlayerOnScreen.Send(CreateEffectPacket(effect.Name));
                    }
                    else Remove(effect.Name);
                }
            }
        }

        public void CheckUp()
        {
            foreach (var effect in EffectsPoll.Values)
            {
                if (DateTime.Now > effect.ExpireDate)
                {
                    Remove(effect.Name);
                }
            }
        }

        public void Load()
        {
            if (File.Exists(Program.ServerConfig.DbLocation + "\\EffectStatus\\" + Owner.Player.UID + ".ini"))
            {
                var list = File.ReadAllLines(Program.ServerConfig.DbLocation + "\\EffectStatus\\" + Owner.Player.UID + ".ini");
                if (list != null && list.Length > 0)
                {
                    foreach (var str in list)
                    {
                        var eff = str.Split('/');
                        if (eff.Length > 0)
                        {
                            EffectsPoll.Add(eff[0], new Info() { Name = eff[0], ExpireDate = DateTime.FromBinary(long.Parse(eff[1])) });
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Owner.Player.View.SendView(CreateEffectPacket(eff[0]), true);
                            }
                        }
                    }
                }
            }
        }

        public void Save()
        {
            WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
            if (binary.Open(Program.ServerConfig.DbLocation + "\\EffectStatus\\" + Owner.Player.UID + ".ini", FileMode.Create))
                binary.Close();
            List<string> effects = new List<string>();
            foreach (var eff in EffectsPoll.Values)
                effects.Add($"{eff.Name}/{eff.ExpireDate.Ticks}");
            File.WriteAllLines(Program.ServerConfig.DbLocation + "\\EffectStatus\\" + Owner.Player.UID + ".ini", effects.ToList());
        }
    }
}
