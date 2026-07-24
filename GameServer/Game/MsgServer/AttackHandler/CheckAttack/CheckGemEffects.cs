namespace COServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CheckGemEffects
    {
        public enum GemEffect : byte
        {
            moon,
            phoenix,
            rainbow,
            purpleray,
            goldendragon,
            fastflash,//fury gem
            goldenkylin,
            recovery,//tortoiseGem
            Aegis4,//bless Effect
        }
        public static void CheckRespouseDamage(Client.GameClient client)
        {
            if (client.Equipment.SuperKylinGem)
            {
                if (Calculate.Base.Success(10))
                {
                    if (Calculate.Base.Success(5))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.goldenkylin.ToString() });
                        }
                    }
                }
            }
            if (client.Equipment.SuprtTortoiseGem)
            {
                if (Calculate.Base.Success(10))
                {
                    if (Calculate.Base.Success(5))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.recovery.ToString() });
                        }
                    }
                }
            }
            if (client.Equipment.HaveBless)
            {
                if (Calculate.Base.Success(10))
                {
                    if (Calculate.Base.Success(5))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.Aegis4.ToString() });
                        }
                    }
                }
            }
        }
        public static void TryngEffect(Client.GameClient client)
        {
            if (client.Pet != null) return;
          
            if (client.Equipment.SuperDragonGem)
            {
                if (Calculate.Base.Success(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.goldendragon.ToString() });
                    }
                }
            }
            if (client.Equipment.SuperPheonixGem)
            {
                if (Calculate.Base.Success(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.phoenix.ToString() });
                    }
                }
            }
            if (client.Equipment.SuperRaibowGem)
            {
                if (Calculate.Base.Success(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.rainbow.ToString() });
                    }
                }
            }
            if (client.Equipment.SuperMoonGem)
            {
                if (Calculate.Base.Success(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.moon.ToString() });
                    }
                }
            }
            if (client.Equipment.SuperVioletGem)
            {
                if (Calculate.Base.Success(5))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendGemString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { GemEffect.purpleray.ToString() });
                    }
                }
            }
        }
    }
}
