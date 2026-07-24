namespace COServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public static class ProcessColdTime
    {
        public class Record
        {
            public Client.GameClient Attacker;
            public Client.GameClient Attacked;
        }
        public static ProcessAttackQueue ProcessAttack = new ProcessAttackQueue();



        public class ProcessAttackQueue : ConcurrentSmartThreadQueue<Record>
        {
            public ProcessAttackQueue()
                : base(2)
            {
                Start(5);
            }
            public void TryEnqueue(Record action)
            {
                Enqueue(action);
            }

            protected unsafe override void OnDequeue(Record record, int time)
            {
                var Attacker = record.Attacker;
                var Attacked = record.Attacked;
                if (Attacker != null && Attacked != null)
                {
                    //if (!Attacked.Player.ContainFlag(MsgUpdate.Flags.IncreseColdTime))
                    //{
                    //    if (Role.Core.Rate(100 - 5 * (Attacked.Player.BattlePower - Attacker.Player.BattlePower)))
                    //    {
                    //        int IncreaseColdTime = 0;
                    //        MsgSpell ClientSpell = null;
                    //        Attacked.Player.AddFlag(MsgUpdate.Flags.IncreseColdTime, IncreaseColdTime, true);
                    //        List<MsgSpell> array = new List<MsgSpell>();

                    //        foreach (var spell in Attacked.MySpells.ClientSpells.Values)
                    //        {
                    //            if (spell.IsSpellWithColdTime)
                    //            {
                    //                spell.ColdTime = spell.ColdTime.AddMilliseconds(IncreaseColdTime * 1000);
                    //                if (spell.GetColdTime > 0)
                    //                    array.Add(spell);
                    //            }
                    //        }

                    //    }
                    //}
                }
            }
        }

    }
}
