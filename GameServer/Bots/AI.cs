using COServer.Client;
using COServer.Game.MsgServer;
using COServer.Role;
using COServer.Role.Instance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using static COServer.Bots.Enumerator;


namespace COServer.Bots
{
	public class AI
	{
		private int JumpSpeed = 0;
		private int Accuracy = 0;
		private string Name = "iA.OrigensCO";

		private DateTime LastAtttack = DateTime.Now;

		private SkillType SkillType;


		public void SetLevel(BotLevel Level)
		{
			this.Level = Level;
			switch (Level)
			{
				case BotLevel.Noob:
					JumpSpeed = 3000;
					Accuracy = 30;
					break;

				case BotLevel.Easy:
					JumpSpeed = 1500;
					Accuracy = 40;
					break;

				case BotLevel.Normal:
					JumpSpeed = 1250;
					Accuracy = 60;
					break;

				case BotLevel.Medium:
					JumpSpeed = 1000;
					Accuracy = 70;
					break;

				case BotLevel.Hard:
					JumpSpeed = 800;
					Accuracy = 80;
					break;

				case BotLevel.Insane:
					JumpSpeed = 550;
					Accuracy = 100;
					break;
			}
		}


		public GameClient Bot;

		public DateTime ToStart;
		public BotLevel Level;
		public AI()
		{
			Bot = new GameClient(null) { Fake = true };
		}


		public void LoadBot(BotType BotType, GameClient Oppenent, SkillType skillType)
		{
			SkillType = skillType;
			if (Oppenent is null) return;

			using (var rec = new ServerSockets.RecycledPacket())
			{
				var stream = rec.GetStream();
				Bot.Player = new Player(Bot);
				Bot.Inventory = new Inventory(Bot);
				Bot.Equipment = new Equip(Bot);
				Bot.Warehouse = new Warehouse(Bot);
				Bot.MyProfs = new Proficiency(Bot);
				Bot.MySpells = new Spell(Bot);
				Bot.Status = new MsgStatus();

				switch (BotType)
				{

					case BotType.DuelBot:
						{
							Bot.Player.Name = Name;

							Bot.Player.UID = Database.Server.BotCounter.Next;

							Bot.Player.Avatar = Oppenent.Player.Avatar;//face
							Bot.Player.Body = 1003;
							Bot.Player.Hair = 935;
							Bot.Player.TransformationID = 0;
							Bot.Player.Strength = 176;
							Bot.Player.Agility = 36;
							Bot.Player.Vitality = 500;
							Bot.Player.Spirit = 0;
							Bot.Player.PKPoints = 0;
							Bot.Player.Level = 137;
							Bot.Player.Class = 15;
							Bot.Player.MyHits = Oppenent.Player.MyHits;
							Bot.Player.Map = Oppenent.Player.Map;
							Bot.Player.DynamicID = Oppenent.Player.DynamicID;
							Bot.Map = Database.Server.ServerMaps[700];
							Bot.Player.X = Oppenent.Player.X;
							Bot.Player.Y = Oppenent.Player.Y;
							Bot.Map.Enquer(Bot);

							Bot.Player.HitPoints = 1000;
							Bot.Status.MaxHitpoints = 1000;

							Bot.Player.Stamina = 100;



							Bot.Player.LeftWeaponId = 420003;
							Bot.Player.RightWeaponId = 410301;

							if (!Bot.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FastBlader))
								Bot.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FastBlader, 4);
							if (!Bot.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScrenSword))
								Bot.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScrenSword, 4);

							Bot.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
							Oppenent.Send(Bot.Player.GetArray(stream, false));
							Database.Server.GamePoll.TryAdd(Bot.Player.UID, Bot);
							Bot.Player.View.Role();
							BeginJumpBot(Oppenent);

							break;
						}
				}
			}
		}

		#region Jump Bot
		public DateTime LastBotJump = DateTime.Now;
		public void HandleJump()
		{
			if (DateTime.Now >= LastBotJump.AddMilliseconds(JumpSpeed))
			{
				LastBotJump = DateTime.Now;
				Jump_Action();
				if (Level == BotLevel.Insane)
				{
					if (Core.GetDistance(Bot.Player.X, Bot.Player.Y, Bot.Player.Target.X, Bot.Player.Target.Y) <= 10)
					{
						if (Bot.Player.Map == Bot.Player.Target.Map && Bot.Player.DynamicID == Bot.Player.Target.DynamicID)
						{
							#region fb / ss
							Shoot(Accuracy);

							LastAtttack = DateTime.Now;
							#endregion
						}
						else Dispose();
					}
				}
			}

		}
		public void Attack()
		{
			if (DateTime.Now >= LastAtttack.AddMilliseconds(500))
			{
				if (Core.GetDistance(Bot.Player.X, Bot.Player.Y, Bot.Player.Target.X, Bot.Player.Target.Y) <= 10)
				{
					if (Bot.Player.Map == Bot.Player.Target.Map && Bot.Player.DynamicID == Bot.Player.Target.DynamicID)
					{
						#region fb / ss
						Shoot(Accuracy);

						LastAtttack = DateTime.Now;
						#endregion
					}
					else Dispose();
				}
			}
		}

		public void BeginJumpBot(GameClient target)
		{
			BotProcessring.Bots.TryAdd(Bot.Player.UID, this);

			Bot.Player.Target = target.Player;
		}

		public void StopJumpBot()
		{
			BotProcessring.Bots.TryRemove(Bot.Player.UID, out _);
		}

		private void Jump_Action()
		{
			if (Bot.Player.Target is null) return;

			Jump();
		}

		private unsafe void Jump()
		{
		jmp:
			var x = (ushort)(Bot.Player.Target.X + Program.GetRandom.Next(-4, 4));
			var y = (ushort)(Bot.Player.Target.Y + Program.GetRandom.Next(-4, 4));
			if (!Bot.Map.ValidLocation(x, y))
				goto jmp;

			Bot.Player.X = x;
			Bot.Player.Y = y;

			ActionQuery jmp = new ActionQuery();
			jmp.ObjId = Bot.Player.UID;
			jmp.Type = ActionType.Jump;
			jmp.dwParam = (uint)((Bot.Player.Y << 16) | Bot.Player.X);
			Bot.Map.View.MoveTo<Role.IMapObj>(Bot.Player, Bot.Player.X, Bot.Player.Y);
			using (var rec = new ServerSockets.RecycledPacket())
			{
				var stream = rec.GetStream();
				Bot.Player.Target.View.SendView(stream.ActionCreate(&jmp), true);
			}
		}

		public void Shoot(int accu)
		{
			var interact = new InteractQuery
			{
				AtkType = MsgAttackPacket.AttackID.Magic,
				SpellID = (ushort)(SkillType == SkillType.FastBlade ? 1045 : 1046),
				UID = Bot.Player.UID,
				SpellLevel = 4
			};
			if (Core.Rate(accu))
			{
				interact.X = Bot.Player.Target.X;
				interact.Y = Bot.Player.Target.Y;
				interact.OpponentUID = Bot.Player.Target.UID;
			}
			else
			{
				interact.X = (ushort)(Bot.Player.Target.X + 1);
				interact.Y = (ushort)(Bot.Player.Target.Y + 1);
			}
			Bot.Player.TotalHits++;
			bool pass = false;
			bool hitSomeone = false;
			using (var rec = new ServerSockets.RecycledPacket())
			{
				var stream = rec.GetStream();

				var magiceffect = new MsgSpellAnimation(Bot.Player.UID, 0, interact.X, interact.Y, interact.SpellID, interact.SpellLevel, 0);
				if (interact.OpponentUID != 0)
				{
					magiceffect.Targets.Enqueue(new MsgSpellAnimation.SpellObj(interact.OpponentUID, (uint)Bot.Player.Chains));
					hitSomeone = true;

					if (Bot.Player.MyHits != 0)
					{
						Bot.Player.MyHits--;
						if (Bot.Player.MyHits <= 0)
						{
							Bot.Player.Target.Owner.SendSysMesage("You've lost, better luck next time!");
							Bot.Player.Target.Owner.Teleport(454, 294, 1002);
							Bot.Player.Target.MyHits = 0;
							Bot.Player.MyHits = 0;
							this.Dispose();
							return;
						}
					}

					if (!pass)
					{
                        if (Bot.Player == null || Bot == null)
                        {
							return;
                        }
						Bot.Player.Hits++;
						Bot.Player.Chains++;
						if (Bot.Player.Chains > Bot.Player.MaxChains)
							Bot.Player.MaxChains = Bot.Player.Chains;
						pass = true;
					}

					magiceffect.SetStream(stream);
					magiceffect.Send(Bot.Player.Target.Owner, true);

				}
				else
				{
					if (!hitSomeone)
						Bot.Player.Chains = 0;

					MsgAttackPacket.ProcescMagic(Bot, stream, interact);
				}
			}
		}
		#endregion

		public void Dispose()
		{
			StopJumpBot();

			if (Bot == null)
				return;
			if (Bot.Map == null)
				return;
			Bot.Map.Denquer(Bot);
			Bot.Player.View.Role();

			Bot = null;
		}
	}
}