using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.Bots
{
	public class Enumerator
    {
		public enum BotLevel
		{
			Noob = 0,
			Easy = 1,
			Normal = 2,
			Medium = 3,
			Hard = 4,
			Insane = 5
		}
		public enum BotType
		{
			EventBot = 0,
			DuelBot = 1,
			TournamentBot = 2,
			AFKBot = 3
		}
		public enum SkillType
		{
			ScentSword,
			FastBlade,
		}
	}
}
