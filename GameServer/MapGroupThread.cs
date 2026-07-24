using COServer.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer
{
    public class MapGroupThread
    {
        public const int AI_Buffer = 500,
      AI_Guard = 700,
      AI_Monster = 400,
      User_Buffers = 500,
      User_Stamina = 500,
      User_StampXPCount = 3000,
      User_AutoAttack = 700,
      User_CheckSecounds = 1000,
      User_Mining = 3000,
      User_ItemTIme = 1000,
      User_CheckItems = 1000,
        user_minig = 1000;


        private ThreadItem thread;
        public MapGroupThread(int interval)
        {
            thread = new ThreadItem(interval, OnProcess);
            thread.Open();
        }
        private void OnProcess()
        {
            var clock = Time32.Now;

            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Fake) continue;

                user.Player.View.MonsterCallBack(clock);

                //if (clock > user.AutoHunt)
                //{
                //    if (user.Player.Robot)
                //    {
                //        Game.AutoHunting2.JumpRobot(user);
                //    }
                //    user.AutoHunt.Value = clock.Value + 1350;
                //}
                //if (clock > user.AutoHuntAttack)
                //{
                //    if (user.Player.Robot)
                //    {
                //        Game.AutoHunting2.SkillRobot(user);
                //    }
                //    if (user.Player.Robot)
                //    {
                //        Game.AutoHunting2.ReviveRobot(user);
                //    }
                //    user.AutoHuntAttack.Value = clock.Value + 700;
                //}
                if (clock > user.BuffersStamp)
                {
                    Client.PoolProcesses.BuffersCallback(user);
                    user.BuffersStamp.Value = clock.Value + User_Buffers;
                }
                if (clock > user.CheckItemTimeStamp)
                {
                    Client.PoolProcesses.CheckItemTime(user);
                    user.CheckItemTimeStamp.Value = clock.Value + User_ItemTIme;
                }

                if (clock > user.AttackStamp)
                {
                    Client.PoolProcesses.AutoAttackCallback(user);
                    user.AttackStamp.Value = clock.Value + User_AutoAttack;
                }
                if (clock > user.StaminStamp)
                {
                    Client.PoolProcesses.StaminaCallback(user);
                    user.StaminStamp.Value = clock.Value + User_Stamina;
                }

                if (clock > user.CheckItemsView)
                {
                    Client.PoolProcesses.CheckItems(user);
                    Client.PoolProcesses.AiThread(user);
                    user.CheckItemsView.Value = clock.Value + User_CheckItems;
                }
                if (clock > user.CheckSecoundsStamp)
                {
                    Client.PoolProcesses.CheckSecond(user);
                    user.CheckSecoundsStamp.Value = clock.Value + User_CheckSecounds;
                }
                if (clock > user.XPCountStamp)
                {
                    Client.PoolProcesses.XPCounter(user);
                    user.XPCountStamp.Value = clock.Value + User_StampXPCount;
                }
                if (clock > user.user_minig)
                {
                    Client.PoolProcesses.CharacterCallback(user);
                    user.user_minig.Value = clock.Value + User_Mining;
                }
            }


            foreach (var bot in BotProcessring.Bots.Values.Where(x => x != null))
            {
                if (!bot.Bot.Player.Alive) continue;
                //if (bot.ToStart > System.DateTime.Now) continue;
                bot.HandleJump();
                bot.Attack();
            }
        }
    }
}
