namespace COServer.Database
{
    using static COServer.Role.Flags;//ty pro4never redux
    public class MineRule
    {
        public uint ID { get; set; }
        public MineType RuleType { get; set; }
        public double RuleChance { get; set; }
        public int RuleAmount { get; set; }
        public uint RuleValue { get; set; }
    }
    public class MiningTable
    {
        private static MineRule[] MineRules = new MineRule[0];
        public static void Load()
        {
            var reader = new WindowsAPI.IniFile("MiningTable.ini");
            int count = reader.ReadInt32("MinesAmount", "Amount", 0);
            MineRules = new MineRule[count];
            for (int i = 0; i < count; i++)
            {
                var Rule = new MineRule()
                {
                    ID = reader.ReadUInt32("MineRules", "id" + i, 0),
                    RuleType = (MineType)reader.ReadUInt32("MineRules", "RuleType" + i, 0),
                    RuleChance = reader.ReadDouble("MineRules", "RuleChance" + i, 0),
                    RuleAmount = reader.ReadInt32("MineRules", "RuleAmount" + i, 0),
                    RuleValue = reader.ReadUInt32("MineRules", "RuleValue" + i, 0)
                };
                MineRules[i] = Rule;
            }
        }
        public static bool GetRandomRole(out MineRule Role)
        {
            Role = null;
            foreach (MineRule role in MineRules)
            {
                Role = role;
                if (PercentSuccess(role.RuleChance))
                {
                    break;
                }
            }
          
            return Role != null;
        }
        public static bool PercentSuccess(double _chance)
        {
            return new System.FastRandom().NextDouble() < _chance / 100.0;
        }

    }
}
