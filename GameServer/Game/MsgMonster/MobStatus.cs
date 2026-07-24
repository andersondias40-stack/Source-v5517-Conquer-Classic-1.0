using System;

namespace COServer.Game.MsgMonster
{
    [Flags]
    public enum MobStatus : uint
    {
        Idle = 0,
        SearchTarget = 1,
        Attacking = 2,
        Respawning = 3
    }
}
