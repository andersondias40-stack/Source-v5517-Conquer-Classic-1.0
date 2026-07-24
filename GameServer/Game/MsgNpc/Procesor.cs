using COServer.Game.MsgServer;
using COServer.Role.Instance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Game.MsgNpc
{
    using ActionInvoker = CachedAttributeInvocation<ProcessAction, NpcAttribute, NpcID>;
    public unsafe delegate void ProcessAction(Client.GameClient user, ServerSockets.Packet stream, byte Option, string Input, uint id);



    public class Procesor
    {
        public static ExecuteNpcInvoker ExecuteNpc = new ExecuteNpcInvoker();
        public unsafe class InvokerClient
        {
            public Client.GameClient client;
            public byte InteractType;
            public byte option;
            public string input;
            public uint npcid;
            public InvokerClient(Client.GameClient Client, ServerSockets.Packet Server_Replay, uint _npcid, byte _InteractType, byte _option, string _input)
            {
                client = Client;

                option = _option;
                InteractType = _InteractType;
                input = _input;
                npcid = _npcid;
            }
        }

        public static ActionInvoker invoker = new ActionInvoker(NpcAttribute.Translator);

        [PacketAttribute(GamePackets.NpcServerReplay)]
        private unsafe static void NpcServerReplay(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.InTrade == true || user.IsVendor || !user.Socket.Alive)
                return;
            if (!user.Player.Alive)
                return;
            uint npcid;
            ushort Mesh;
            byte option;
            byte type;
            NpcServerReplay.Mode Action;
            string input;
            //action ==6 place!
            stream.NpcDialog(out npcid, out Mesh, out option, out type, out Action, out input);
            if (Action == MsgNpc.NpcServerReplay.Mode.PlaceFurniture)
            {
                if (!user.Inventory.HaveSpace(1))
                {
                    user.CreateBoxDialog("Please make 1 more space your inventory.");
                    return;
                }
                Npc furniture;
                if (user.MyHouse.Furnitures.TryGetValue(npcid, out furniture))
                {
                    var npc = Database.NpcServer.GetNpcFromMesh(furniture.Mesh);
                    if (npc != null)
                    {
                        user.Inventory.Add(stream, npc.ItemID);
                        user.MyHouse.Furnitures.TryRemove(npcid, out furniture);
                        Database.ItemType.DBItem item;
                        if (Database.Server.ItemsBase.TryGetValue(npc.ItemID, out item))
                            user.SendSysMesage("You got a " + item.Name + "!", MsgMessage.ChatMode.System);
                        var action = new ActionQuery()
                        {
                            ObjId = npcid,
                            Type = ActionType.RemoveEntity
                        };
                        user.Send(stream.ActionCreate(&action));
                    }
                }
                return;
            }
            if (Action == MsgNpc.NpcServerReplay.Mode.Statue)
            {
                Npc furniture;
                if (user.MyHouse.Furnitures.TryGetValue(npcid, out furniture))
                {
                    user.MyHouse.Furnitures.TryRemove(npcid, out furniture);

                    var action = new ActionQuery()
                    {
                        ObjId = npcid,
                        Type = ActionType.RemoveEntity
                    };
                    user.Send(stream.ActionCreate(&action));
                }
                return;
            }
            if (option == 255)
                return;
            if (npcid == 42 && user.Player.VipLevel >= 6)
            {
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(user, stream);
                dialog.FinalizeDialog();
                return;
            }
            user.ActiveNpc = (uint)npcid;
            ExecuteNpc.Enqueue(new InvokerClient(user, stream, (uint)npcid, type, option, input));
        }
        [PacketAttribute(GamePackets.NpcServerRequest)]
        private unsafe static void NpcServerRequest(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.InTrade == true || user.IsVendor || !user.Socket.Alive)
                return;
            if (!user.Player.Alive)
                return;
            uint npcid;
            ushort Mesh;
            byte option;
            byte type;
            NpcServerReplay.Mode Action;
            string input;

            stream.NpcDialog(out npcid, out Mesh, out option, out type, out Action, out input);
            
           if (type == 101 && user.Player.VipLevel >= 6)
            {
                if (option == 230)
                {
                    user.ActiveNpc = 12;

                    ExecuteNpc.Enqueue(new InvokerClient(user, stream, 12, type, option, input));
                    return;
                }
                if (option == 231)//deposit
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText("What would you like to deposit?");
                    data.AddOption("DragonBalls.", 232);
                    data.AddOption("DBScrolls.", 234);
                    data.AddOption("Meteors.", 236);
                    data.AddOption("MeteorScrolls.", 238);
                    data.FinalizeDialog();
                    return;
                }
                #region Deposit
                //dbs
                if (option == 232)// dbs
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositDbs} DragonBalls.\n");
                    data.AddText($"How many DragonBalls would you like to deposit?");
                    data.AddInput($"DragonBalls:", 233);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 233)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var space = byte.TryParse(input, out var res);
                    if (space == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Inventory.Contain(Database.ItemType.DragonBall, res) && res > 0)
                    {
                        user.Inventory.Remove(Database.ItemType.DragonBall, res, stream);
                        user.Player.DepositDbs += res;
                        data.AddText($"Successfully deposited: {res} DragonBalls.\n");
                        data.AddText($"You now have: {user.Player.DepositDbs} DragonBalls.");
                        data.AddOption("Thanks!", 255);
                        data.FinalizeDialog();
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                    }
                    return;

                }


                //db scroll
                if (option == 234)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositSDbs} \n");
                    data.AddText($"How many DBScrolls would you like to deposit?");
                    data.AddInput($"DBScrolls:", 235);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 235)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var space = byte.TryParse(input, out var num);
                    if (space == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Inventory.Contain(Database.ItemType.DragonBallScroll, num) && num > 0)
                    {
                        user.Inventory.Remove(Database.ItemType.DragonBallScroll, num, stream);
                        user.Player.DepositSDbs += num;
                        data.AddText($"Successfully deposited: {num} DBScrolls.\n");
                        data.AddText($"You now have: {user.Player.DepositSDbs} DBScrolls.");
                        data.AddOption("Thanks!", 255);
                        data.FinalizeDialog();
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }

                //mets
                if (option == 236)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositMets} Meteors.\n");
                    data.AddText($"How many Meteors would you like to deposit?");
                    data.AddInput($"Meteors:", 237);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 237)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var space = byte.TryParse(input, out var numb);
                    if (space == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Inventory.Contain(Database.ItemType.Meteor, numb) && numb > 0)
                    {
                        user.Inventory.Remove(Database.ItemType.Meteor, numb, stream);
                        user.Player.DepositMets += numb;
                        data.AddText($"Successfully deposited: {numb} Meteors.\n");
                        data.AddText($"You now have: {user.Player.DepositMets} Meteors.");
                        data.AddOption("Thanks!", 255);
                        data.FinalizeDialog();
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }

                //mets scroll

                if (option == 238)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositSMets} MeteorScrolls.\n");
                    data.AddText($"How many MeteorScrolls would you like to deposit?");
                    data.AddInput($"MeteorScrolls:", 239);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 239)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var space = byte.TryParse(input, out var numb);
                    if (space == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Inventory.Contain(Database.ItemType.MeteorScroll, numb) && numb > 0)
                    {
                        user.Inventory.Remove(Database.ItemType.MeteorScroll, numb, stream);
                        user.Player.DepositSMets += numb;
                        data.AddText($"Successfully deposited: {numb} MeteorScrolls.\n");
                        data.AddText($"You now have: {user.Player.DepositSMets} MeteorScrolls.");
                        data.AddOption("Thanks!", 255);
                        data.FinalizeDialog();
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }

                #endregion
                if (option == 248)// Wirthdraw
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText("What would you like to withdraw?");
                    data.AddOption("DragonBalls.", 240);
                    data.AddOption("DBScrolls.", 242);
                    data.AddOption("Meteors.", 244);
                    data.AddOption("MeteorScrolls.", 246);
                    data.FinalizeDialog();
                    return;

                }
                #region Wirthdraw
                //dbs
                if (option == 240)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositDbs} \n");
                    data.AddText($"How many DragonBalls would you like to withdraw?");
                    data.AddInput($"DragonBalls:", 241);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 241)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var index = byte.TryParse(input, out var space);
                    if (index == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Player.DepositDbs >= space && space > 0)
                    {
                        if (user.Inventory.HaveSpace(space))
                        {
                            user.Inventory.Add(stream, Database.ItemType.DragonBall, space);
                            user.Player.DepositDbs -= space;
                            data.AddText($"Successfully withdrew: {space} DragonBalls.\n");
                            data.AddText($"You now have: {user.Player.DepositDbs} DragonBalls.");
                            data.AddOption("Thanks!", 255);
                            data.FinalizeDialog();

                        }
                        else
                        {
                            data.AddText($"Sorry, you don't have {space} free spaces in your inventory.");
                            data.AddOption("Okay.", 255);
                            data.FinalizeDialog();
                        }
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Okay.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }

                //dragon balls scroll

                if (option == 242)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositSDbs} \n");
                    data.AddText($"How many DBScrolls would you like to withdraw?");
                    data.AddInput($"DBScrolls:", 243);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 243)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var index = byte.TryParse(input, out var space);
                    if (index == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Player.DepositSDbs >= space && space > 0)
                    {
                        if (user.Inventory.HaveSpace(space))
                        {
                            user.Inventory.Add(stream, Database.ItemType.DragonBallScroll, space);
                            user.Player.DepositSDbs -= space;
                            data.AddText($"Successfully withdrew: {space} DBScrolls.\n");
                            data.AddText($"You now have: {user.Player.DepositSDbs} DBScrolls.");
                            data.AddOption("Thanks!", 255);
                            data.FinalizeDialog();
                        }
                        else
                        {
                            data.AddText($"Sorry, you don't have {space} free spaces in your inventory.");
                            data.AddOption("Okay.", 255);
                            data.FinalizeDialog();
                        }
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Okay.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }

                // meteors

                if (option == 244)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositMets} Meteors.\n");
                    data.AddText($"How many Meteors would you like to withdraw?");
                    data.AddInput($"Meteors:", 245);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 245)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var index = byte.TryParse(input, out var space);
                    if (index == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Player.DepositMets >= space && space > 0)
                    {
                        if (user.Inventory.HaveSpace(space))
                        {
                            user.Inventory.Add(stream, Database.ItemType.Meteor, space);
                            user.Player.DepositMets -= space;
                            data.AddText($"Successfully withdrew: {space} Meteors.\n");
                            data.AddText($"You now have: {user.Player.DepositMets} Meteors.");
                            data.AddOption("Thanks!", 255);
                            data.FinalizeDialog();
                        }
                        else
                        {
                            data.AddText($"Sorry, you don't have {space} free spaces in your inventory.");
                            data.AddOption("Okay.", 255);
                            data.FinalizeDialog();
                        }
                    }
                    else
                    {
                        data.AddText($"Sorry, you don't have this amount.");
                        data.AddOption("Okay.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }

                //mets scroll


                if (option == 246)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositSMets} MeteorScrolls.\n");
                    data.AddText($"How many MeteorScrolls would you like to withdraw?");
                    data.AddInput($"MeteorScrolls:", 247);
                    data.FinalizeDialog();
                    return;
                }
                if (option == 247)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    var index = byte.TryParse(input, out var space);
                    if (index == false)
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                        return;
                    }
                    if (user.Player.DepositSMets >= space && space > 0)
                    {
                        if (user.Inventory.HaveSpace(space))
                        {
                            user.Inventory.Add(stream, Database.ItemType.MeteorScroll, space);
                            user.Player.DepositSMets -= space;
                            data.AddText($"Successfully withdrew: {space} MeteorScrolls.\n");
                            data.AddText($"You now have: {user.Player.DepositSMets} MeteorScrolls.");
                            data.AddOption("Thanks!", 255);
                            data.FinalizeDialog();
                        }
                        else
                        {
                            data.AddText($"Sorry, you don't have {space} free spaces in your inventory.");
                            data.AddOption("Okay.", 255);
                            data.FinalizeDialog();
                        }
                    }
                    else
                    {
                        data.AddText($"Wrong amount, please try again.");
                        data.AddOption("Sorry.", 255);
                        data.FinalizeDialog();
                    }
                    return;
                }
                #endregion

                if (option == 250)
                {
                    Game.MsgNpc.Dialog data = new Game.MsgNpc.Dialog(user, stream);
                    data.AddText($"You have: {user.Player.DepositMets} Meteors.\n");
                    data.AddText($"You have: {user.Player.DepositSMets} MeteorScrolls.\n");
                    data.AddText($"You have: {user.Player.DepositDbs} DragonBalls.\n");
                    data.AddText($"You have: {user.Player.DepositSDbs} DBScrolls.");
                    data.AddOption("Thanks!", 255);
                    data.FinalizeDialog();
                    return;
                }
            }
            if (type == (byte)NpcReply.InteractTypes.MessageBox)
            {
                if (Program.BlockTeleportMap.Contains(user.Player.Map))
                    return;
                if (user.Player.StartMessageBox > Time32.Now)
                {
                    if (option == 0 && user.Player.MessageOK != null)
                        user.Player.MessageOK.Invoke(user);
                    else if (user.Player.MessageCancel != null)
                        user.Player.MessageCancel.Invoke(user);
                }
                user.Player.MessageOK = null;
                user.Player.MessageCancel = null;
                return;
            }
            if (type == 102)
            {
                if (user.Player.GuildRank == Role.Flags.GuildMemberRank.GuildLeader || user.Player.GuildRank == Role.Flags.GuildMemberRank.DeputyLeader)
                {
                    if (user.Player.MyGuild != null)
                    {
                        user.Player.MyGuild.Quit(input, true, stream);
                        return;
                    }
                }
            }
            if (option == 255 || option == 0 || user.InTrade)
                return;
            if (user.ActiveNpc == 987977854)
            {
                switch (option)
                {
                    case 1:
                        {
                            user.Player.Robot = true;
                            break;
                        }
                    case 2:
                        {
                            user.Player.Robot = false;
                            break;
                        }
                    case 3:
                        {
                            user.ActiveNpc = 987977854;
                            Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(user, stream);
                            dialog.Text("You're currently collecting items in order selected below, do you want to change them?");
                            dialog.Option(user.Player.DBallsStatus + " DB Items.", 4);
                            dialog.Option(user.Player.MeteorsStatus + " Meteors.", 5);
                            dialog.Option(user.Player.PlusItemsStatus + " Plus Items.", 13);
                            dialog.Option(user.Player.QualityItemsStatus + " Quality Items.", 6);
                            dialog.Option(user.Player.BlessedItemsStatus + " Blessed Items.", 10);
                            dialog.Option(user.Player.SocketedItemsStatus + " Socketed Items.", 8);
                            dialog.Option(user.Player.LootMoneyStatus + " Gold.", 16);
                            dialog.Option("Thanks!", 255);
                            dialog.AddAvatar(0);
                            dialog.FinalizeDialog();
                            break;
                        }
                    case 4: user.Player.LootDragonBalls = !user.Player.LootDragonBalls; break;
                    case 5: user.Player.LootMeteorItems = !user.Player.LootMeteorItems; break;
                    case 6: user.Player.LootQualityItems = !user.Player.LootQualityItems; break;
                    case 8: user.Player.LootSocketedItems = !user.Player.LootSocketedItems; break;
                    case 10: user.Player.LootBlessedItems = !user.Player.LootBlessedItems; break;
                    case 13: user.Player.LootPlusItems = !user.Player.LootPlusItems; break;
                    case 16: user.Player.LootMoney = !user.Player.LootMoney; break;
                }
                return;
            }
            if (user.ActiveNpc == 9999997 && user.Player.WaitingKillCaptcha)
            {
                if (option == 255) return;
                if (input == user.Player.KillCountCaptcha)
                {
                    user.Player.SolveCaptcha();
                }
                else
                {
                    Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(user, stream);
                    dialog.Text("Input the current text: " + user.Player.KillCountCaptcha + " to verify you're a human.");
                    dialog.AddInput("Captcha message:", (byte)user.Player.KillCountCaptcha.Length);
                    dialog.Option("No thank you.", 255);
                    dialog.AddAvatar(39);
                    dialog.FinalizeDialog();
                }
                return;
            }
            npcid = (uint)user.ActiveNpc;

            ExecuteNpc.Enqueue(new InvokerClient(user, stream, (uint)npcid, type, option, input));
        }
        public class ExecuteNpcInvoker : ConcurrentSmartThreadQueue<InvokerClient>
        {
            public ExecuteNpcInvoker()
                : base(3)
            {
                Start(5);
            }
            public void TryEnqueue(InvokerClient action)
            {
                Enqueue(action);
            }

            protected unsafe override void OnDequeue(InvokerClient action, int time)
            {
                try
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        if (!action.client.Player.OnMyOwnServer)
                        {
                            if (action.client.Player.Map == 3935)
                            {
                                Game.MsgNpc.Npc _obj;
                                if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out _obj))
                                {
                                    if (action.client.ProjectManager)
                                        action.client.SendSysMesage("Active Npc [" + action.npcid + "] X[" + _obj.X + "] Y[" + _obj.Y + "]", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                                    Tuple<NpcAttribute, ProcessAction> processFolded;
                                    if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                        processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else
                                {
                                    Role.IMapObj inpc;
                                    if (action.client.Player.View.TryGetValue((uint)action.npcid, out inpc, Role.MapObjectType.SobNpc))
                                    {
                                        var npc = inpc as Role.SobNpc;
                                        Tuple<NpcAttribute, ProcessAction> processFolded;
                                        if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                            processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);

                                    }
                                }
                            }
                            return;
                        }
                        if (action.InteractType == (byte)NpcReply.InteractTypes.MessageBox)
                        {
                            if (action.client.Player.StartMessageBox > Time32.Now)
                            {
                                if (action.option == 255 && action.client.Player.MessageOK != null)
                                    action.client.Player.MessageOK.Invoke(action.client);
                                else if (action.client.Player.MessageCancel != null)
                                    action.client.Player.MessageCancel.Invoke(action.client);
                            }
                            action.client.Player.MessageOK = null;
                            action.client.Player.MessageCancel = null;
                            return;
                        }
                        if ((uint)action.npcid == 3124)//house WH
                        {
                            if (action.client.MyHouse != null && action.client.Player.DynamicID == action.client.Player.UID)
                            {
                                ActionQuery query = new ActionQuery()
                                {
                                    Type = ActionType.OpenDialog,
                                    ObjId = action.client.Player.UID,
                                    dwParam = MsgServer.DialogCommands.Warehouse,
                                    wParam1 = action.client.Player.X,
                                    wParam2 = action.client.Player.Y
                                };
                                action.client.Send(stream.ActionCreate(&query));

                                return;
                            }
                            else
                            {
                                action.client.SendSysMesage("I'm sorry but you dont own this house!");
                            }
                        }
                        if (action.client.Player.Map == 1038)//Guild War
                        {
                            Role.IMapObj inpc;
                            if (action.client.Player.View.TryGetValue((uint)action.npcid, out inpc, Role.MapObjectType.SobNpc))
                            {
                                var npc = inpc as Role.SobNpc;
                                Tuple<NpcAttribute, ProcessAction> processFolded;
                                if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                    processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                                return;
                            }

                        }
                        if (action.npcid == (uint)NpcID.ExitArena || action.npcid == (uint)NpcID.GoldPrizeToken
                              || action.npcid == (uint)NpcID.HeavenDemonBox
                              || action.npcid == (uint)NpcID.ChaosDemonBox
                              || action.npcid == (uint)NpcID.SacredDemonBox
                              || action.npcid == (uint)NpcID.AuroraDemonBox
                              || action.npcid == (uint)NpcID.DemonBox
                              || action.npcid == (uint)NpcID.AncientDemonBox
                            || action.npcid == (uint)NpcID.FloodDemonBox
                            //|| action.npcid == (uint)NpcID.VIPBook
                            || action.npcid == (uint)NpcID.L60UniqueGearPack)
                        {
                            Tuple<NpcAttribute, ProcessAction> processFolded;
                            if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                            return;
                        }
                        Game.MsgNpc.Npc obj;
                        if (action.client.Map.SearchNpcInScreen((uint)action.npcid, action.client.Player.X, action.client.Player.Y, out obj))
                        {
                            if (action.client.ProjectManager)
                                action.client.SendSysMesage("Active Npc [" + action.npcid + "] X[" + obj.X + "] Y[" + obj.Y + "]", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white); if (action.client.Player.Map == 1511)
                                if (action.client.Player.Map == 1511)
                                {
                                    
                                    return;
                                }
                            if (action.client.Player.Map == 1780)
                            {
                                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.TreasureThief)
                                {
                                    var tournament = Game.MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgTreasureChests;
                                    tournament.Reward(action.client, obj, stream);
                                    return;
                                }
                            }
                            Tuple<NpcAttribute, ProcessAction> processFolded;
                            if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);
                            else
                            {
                                //if (action.npcid == (uint)NpcID.CityWars1 || action.npcid == (uint)NpcID.CityWars2
                                //    || action.npcid == (uint)NpcID.CityWars3
                                //    || action.npcid == (uint)NpcID.CityWars4
                                //    || action.npcid == (uint)NpcID.CityWars5)
                                //{
                                //    NpcHandler.CityWars(action.client, stream, action.option, action.input, action.npcid);

                                //}
                                if (action.npcid == (uint)NpcID.BlueMouse || action.npcid == (uint)NpcID.BlueMouse2 || action.npcid == (uint)NpcID.BlueMouse3
                                    || action.npcid == (uint)NpcID.BlueMouse4)
                                {
                                    NpcHandler.BlueMouse(action.client, stream, action.option, action.input, action.npcid);
                                }
                                if (action.npcid == (uint)NpcID.MikeSkypass1 || action.npcid == (uint)NpcID.MikeSkypass2
                                    || action.npcid == (uint)NpcID.MikeSkypass3 || action.npcid == (uint)NpcID.MikeSkypass4
                                    || action.npcid == (uint)NpcID.MikeSkypass5)
                                {
                                    NpcHandler.MikeSkypass(action.client, stream, action.option, action.input, action.npcid);
                                }
                                if (action.npcid == (uint)NpcID.MikeMain || action.npcid == (uint)NpcID.Mikesnake || action.npcid == (uint)NpcID.Mikesnake1
                                    || action.npcid == (uint)NpcID.Mikesnake2 || action.npcid == (uint)NpcID.Mikesnake3
                                    || action.npcid == (uint)NpcID.Mikesnake4 || action.npcid == (uint)NpcID.Mikesnake5 || action.npcid == (uint)NpcID.Mikesnake6
                                    || action.npcid == (uint)NpcID.Mikesnake7 || action.npcid == (uint)NpcID.Mikesnake8 || action.npcid == (uint)NpcID.Mikesnake9
                                    || action.npcid == (uint)NpcID.Mikesnake10 || action.npcid == (uint)NpcID.Mikesnake11 || action.npcid == (uint)NpcID.Mikesnake12
                                    || action.npcid == (uint)NpcID.Mikesnake13 || action.npcid == (uint)NpcID.Mikesnake14 || action.npcid == (uint)NpcID.Mikesnake15
                                    || action.npcid == (uint)NpcID.Mikesnake16 || action.npcid == (uint)NpcID.Mikesnake17 || action.npcid == (uint)NpcID.Mikesnake18
                                    || action.npcid == (uint)NpcID.Mikesnake19 || action.npcid == (uint)NpcID.Mikesnake20 || action.npcid == (uint)NpcID.Mikesnake21
                                    || action.npcid == (uint)NpcID.Mikesnake22 || action.npcid == (uint)NpcID.Mikesnake23)
                                {
                                    NpcHandler.MikeMain(action.client, stream, action.option, action.input, action.npcid);
                                }
                                if (action.client.Player.Map == 1038)
                                {
                                    if (action.npcid == (uint)NpcID.GuildConductor1 || action.npcid == (uint)NpcID.GuildConductor2
                                        || action.npcid == (uint)NpcID.GuildConductor3 || action.npcid == (uint)NpcID.GuildConductor4)
                                        NpcHandler.GuildConductorsProces(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else if (((int)action.npcid >= 10031 && (int)action.npcid <= 10041 || (int)action.npcid == 10043) && action.client.Player.DynamicID == 0)
                                {
                                    NpcHandler.SpaceMarks(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else if (action.npcid == (uint)NpcID.TeleGuild1 || action.npcid == (uint)NpcID.TeleGuild2
                                   || action.npcid == (uint)NpcID.TeleGuild3 || action.npcid == (uint)NpcID.TeleGuild4)
                                {
                                    NpcHandler.GuildCondTeleBack(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else if (action.npcid == (uint)NpcID.WHTwin || action.npcid == (uint)NpcID.wHPheonix
                                   || action.npcid == (uint)NpcID.WHMarket || action.npcid == (uint)NpcID.WHBird
                                   || action.npcid == (uint)NpcID.WHDesert || action.npcid == (uint)NpcID.WHApe
                                   || action.npcid == (uint)NpcID.WHPoker || action.npcid == (uint)NpcID.WHMarket2
                                   || action.npcid == (uint)NpcID.WHMarket3 || action.npcid == (uint)NpcID.WHMarket4
                                   || action.npcid == (uint)NpcID.WHMarket5 || action.npcid == (uint)NpcID.WHMarket6)
                                {
                                    NpcHandler.Warehause(action.client, stream, action.option, action.input, action.npcid);
                                }

                                else if ((int)action.npcid >= 925 && (int)action.npcid <= 930 && action.client.Player.Map == 700 && action.client.Player.DynamicID == 0)
                                {
                                    NpcHandler.LotteryBoxes(action.client, stream, action.option, action.input, action.npcid);
                                }
                                else
                                {
                                    if (action.client.ProjectManager)
                                        Console.WriteLine("Haven't found NPC -> " + action.npcid + " ");
                                }
                            }
                        }
                        else if (action.npcid == 12)
                        {
                            if (action.client.Player.VipLevel > 0)
                            {

                                ActionQuery query = new ActionQuery()
                                {
                                    Type = ActionType.OpenDialog,
                                    ObjId = action.client.Player.UID,
                                    dwParam = MsgServer.DialogCommands.VIPWarehouse,
                                    wParam1 = action.client.Player.X,
                                    wParam2 = action.client.Player.Y
                                };
                                action.client.Send(stream.ActionCreate(&query));



                            }
                        }
                        else
                        {
                            Role.IMapObj inpc;
                            if (action.client.Player.View.TryGetValue((uint)action.npcid, out inpc, Role.MapObjectType.SobNpc))
                            {
                                var npc = inpc as Role.SobNpc;
                                Tuple<NpcAttribute, ProcessAction> processFolded;
                                if (invoker.TryGetInvoker((NpcID)action.npcid, out processFolded))
                                    processFolded.Item2(action.client, stream, action.option, action.input, action.npcid);

                            }
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
    }
}
