using COServer.EventsLib;
using System;

namespace COServer.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Player
    {
        public unsafe static void Execute(MsgSpellAnimation.SpellObj obj, Client.GameClient client, Role.Player attacked)
        {
            if (attacked.Name.Contains("[PM]") || attacked.Name.Contains("[GM]"))
            {
                client.SendSysMesage("You can`t attack PM/GM.");
                return;
            }
            if (client.Pet != null) client.Pet.Target = attacked;
            EventManager.ExecuteAttack(attacked.Owner, client, ref obj.Damage);
            if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.KillTheCaptain)
            {
                if (MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                {
                    if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                    {
                        if (client.TeamKillTheCaptain == attacked.Owner.TeamKillTheCaptain)
                            return;
                    }
                }
            }
            if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FiveNOut)
            {
                if (MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                {
                    if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(client))
                    {
                        if (obj.Damage > 1)
                        {
                            obj.Damage = obj.Damage = 1;
                        }
                        if (attacked.FiveNOut > 0)
                        {
                            attacked.FiveNOut--;
                            if (attacked.FiveNOut == 0)
                                attacked.Owner.SendSysMesage("You`ve just lost your final point, next hit you`re out.");
                            else
                                attacked.Owner.SendSysMesage($"You`ve just lost 1 point. Current points left {attacked.FiveNOut}");
                        }
                        else
                            attacked.Owner.Teleport(428, 376, 1002);
                        return;
                    }
                }
            }
            if (Calculate.Base.Success(5))
            {
                CheckAttack.CheckItems.RespouseDurability(client);
            }
            ushort X = attacked.X;
            ushort Y = attacked.Y;
            //using (var rec = new ServerSockets.RecycledPacket())
            //{

            //    var stream = rec.GetStream();
            //    ActionQuery Gui = new ActionQuery()
            //    {
            //        Type = (ActionType)158,
            //        ObjId = client.Player.UID,
            //        wParam1 = client.Player.X,
            //        wParam2 = client.Player.Y
            //    };
            //    client.Send(stream.ActionCreate(&Gui));
            //    ActionQuery action = new ActionQuery()
            //    {
            //        Type = (ActionType)158,
            //        ObjId = attacked.UID,
            //        wParam1 = attacked.X,
            //        wParam2 = attacked.Y
            //    };
            //    attacked.Owner.Send(stream.ActionCreate(&action));
            //}
            if (attacked.HitPoints <= obj.Damage)
            {
                attacked.DeadState = true;
                if (client.Player.OnTransform)
                {
                    if (client.Player.TransformInfo != null)
                    {
                        client.Player.TransformInfo.FinishTransform();
                    }
                }
                attacked.Dead(client.Player, X, Y, 0);

            }
            else
            {
                CheckAttack.CheckGemEffects.CheckRespouseDamage(attacked.Owner);

                attacked.HitPoints -= (int)obj.Damage;
            }


        }
        public unsafe static void ExecutePet(int obj, Client.GameClient client, Role.Player attacked)
        {
            if (Calculate.Base.Success(10))
            {
                CheckAttack.CheckItems.RespouseDurability(client);
            }
            ushort X = attacked.X;
            ushort Y = attacked.Y;
            if (attacked.HitPoints <= obj)
            {
                attacked.DeadState = true;
                attacked.Dead(client.Player, X, Y, 0);

            }
            else
            {
                CheckAttack.CheckGemEffects.CheckRespouseDamage(attacked.Owner);

                attacked.HitPoints -= (int)obj;
            }


        }

    }
}
