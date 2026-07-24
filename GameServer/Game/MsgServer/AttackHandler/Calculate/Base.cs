using System;

namespace COServer.Game.MsgServer.AttackHandler.Calculate
{
    public static class Base
    {
        public class StatusConstants
        {
            public const int AdjustSet = -30000,
                             AdjustFull = -32768,
                             AdjustPercent = 30000,

                             NAME_GREEN = 1,
                             NAME_WHITE = 2,
                             NAME_RED = 3,
                             NAME_BLACK = 4;
        }
        public static uint CutTrail(uint x, uint y) { return (x >= y) ? x : y; }
        public static SafeRandom MyRandom = new SafeRandom();
        public static Int32 Generate(Int32 Min, Int32 Max)
        {
            if (Max != Int32.MaxValue)
                Max++;

            Int32 Value = 0;
            /*lock (Rand) { */
            Value = MyRandom.Next(Min, Max); /*}*/
            return Value;
        }
        public static uint CalculatePoisonDamage(uint Hitpoints, byte Level)
        {
            Hitpoints = (uint)((Hitpoints * (10 * Math.Min((int)(Level + 1), 5))) / 100);//12
            if (Hitpoints <= 0)
                Hitpoints = 1;
            return Hitpoints;
        }
        public static uint CalculatePoisonDamageFog(uint Hitpoints, double Percent)
        {
            Hitpoints = (uint)(Hitpoints * Percent);
            if (Hitpoints <= 0)
                Hitpoints = 1;
            return Hitpoints;
        }
        public static int MulDiv(int number, int numerator, int denominator)
        {
            return (number * numerator /*+ denominator / 2*/) / denominator;
        }
        public static int MulDivgem(int number, int numerator, int denominator)
        {
            return (number * numerator + denominator / 2) / denominator;
        }

        public static bool Dodged(Client.GameClient attacker, Client.GameClient target)
        {
            var agility = attacker.Player.Agility;
            var hitrate = 100 + (agility / 2) - target.Status.Dodge;
            if (target.Status.Dodge > 0)
            {
                hitrate = MulDiv((int)hitrate, ((int)attacker.Status.AgilityAtack * (int)agility - 1) / Math.Max(1, (int)agility), 100);
                hitrate = AdjustHitrate((int)hitrate, (int)attacker.Status.AgilityAtack);

                if (Database.ItemType.IsShield(target.Equipment.LeftWeapon) 
                    || Database.ItemType.IsBoot(attacker.Equipment.Boots))
                    hitrate = MulDiv((int)hitrate, 35, 100);

                hitrate = Math.Min(90, Math.Max(1, hitrate)); // 10~90% dodge
            }
            int rat = Program.GetRandom.Next(50);

            return rat >= hitrate;
        }

        public static bool Dodged(Client.GameClient attacker, Game.MsgMonster.MonsterRole target)
        {
            var agility = attacker.Player.Agility;
            var hitrate = 100 + (agility / 2) - target.Family.Dodge;

            hitrate = MulDiv((int)hitrate, ((int)100 * (int)agility - 1) / Math.Max(1, (int)agility), 100);
            hitrate = AdjustHitrate((int)hitrate, (int)0);

            hitrate = Math.Min(90, Math.Max(10, hitrate)); // 10~90% dodge

            return Program.GetRandom.Next(100) < 15;
        }
        public static int AdjustHitrate(int hitrate, int power)
        {
            var addHitrate = 0;
            addHitrate += Math.Max(0, AdjustDataEx(hitrate, power)) - hitrate;
            return hitrate + addHitrate;
        }

        public static int GetNameType(int nAtkerLev, int nMonsterLev)
        {
            int nDeltaLev = nAtkerLev - nMonsterLev;

            if (nDeltaLev >= 3)
                return StatusConstants.NAME_GREEN;
            else if (nDeltaLev >= 0)
                return StatusConstants.NAME_WHITE;
            else if (nDeltaLev >= -5)
                return StatusConstants.NAME_RED;
            else
                return StatusConstants.NAME_BLACK;
        }
        public static int CalcDamageUser2Monster(int nAtk, int nDef, int nAtkLev, int nDefLev, bool Range)
        {
            if (Range)
            {
                if (nAtkLev >= 120)
                    nAtkLev = 120;
            }
            //int nDamage = nAtk - nDef;
            int nDamage = nAtk > nDef ? nAtk - (int)nDef : nAtk - nDef / 5;

            if (GetNameType(nAtkLev, nDefLev) > StatusConstants.NAME_WHITE)
                return Math.Max(0, nDamage);

            int nDeltaLev = nAtkLev - nDefLev;
            if (nDeltaLev >= 3 && nDeltaLev <= 5)
                nDamage = (int)(nDamage * 1.5);
            else if (nDeltaLev > 5 && nDeltaLev <= 10)
                nDamage *= 2;
            else if (nDeltaLev > 10 && nDeltaLev <= 20)
                nDamage = (int)(nDamage * 2.5);
            else if (nDeltaLev > 20)
                nDamage *= 3;
            else nDamage *= 1;

            return nDamage;
        }
        public static int AdjustMinDamageUser2Monster(int nDamage, Client.GameClient pAtker)
        {
            int nMinDamage = 1;
            nMinDamage += pAtker.Player.Level / 10;

            nMinDamage += (int)(pAtker.Equipment.LeftWeapon % 10);
            nMinDamage += (int)(pAtker.Equipment.RightWeapon % 10);

            return Math.Max(nMinDamage, nDamage);
        }
        public static long BigMulDiv(long number, long numerator, long denominator)
        {
            return (number * numerator + denominator / 2) / denominator;
        }
        public static int AdjustDataEx(int data, int adjust, int maxData = 0)
        {
            if (adjust >= StatusConstants.AdjustPercent)
                return MulDiv(data, adjust - StatusConstants.AdjustPercent, 100);

            if (adjust <= StatusConstants.AdjustSet)
                return -1 * adjust + StatusConstants.AdjustSet;

            if (adjust == StatusConstants.AdjustFull)
                return maxData;

            return data + adjust;
        }
        public static int AdjustAttack(int attack, int power, int defense)
        {
            var addAttack = 0;
            if (defense > 0)
                addAttack += Math.Max(0, AdjustDataEx(defense, power, 100)) - attack;
            addAttack += Math.Max(0, AdjustDataEx(attack, power)) - attack;
            return attack + addAttack;
        }
        public static int AdjustDefense(int defense, int power, int decrease = 0)//, int bless =0)
        {
            var addDefense = 0;
            addDefense += Math.Max(0, AdjustDataEx(defense, power, decrease)) - defense;

            return defense + addDefense;
        }

        public static bool Rate(int value)
        {

            return value > MyRandom.Next() % 100;
        }
        public static Boolean Success(Double Chance)
        {
            //{
            //    if (Chance > 1)
            //        return Role.Core.Rate((int)Chance);
            //    return Role.Core.Rate(Chance);

            return ((Double)Generate(1, 1000000)) / 10000 >= 100 - Chance;
        }
        public static bool GetRefinery(uint attacker, uint Attacked)
        {
            if (attacker <= Attacked)
                return false;

            return Success((int)(attacker - Attacked));
            //    return Role.Core.Rate((double)(attacker - Attacked));
        }

        public static uint MathMin(uint val1, uint val2)
        {
            if (val1 < val2)
                return val1;
            if (val2 <= val1)
                return val2;
            return 0;
        }

        public static uint MathMax(uint val1, uint val2)
        {
            if (val1 < val2)
                return val2;
            if (val2 <= val1)
                return val1;
            return 0;
        }
    
        public static short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            return (short)Math.Sqrt((X - X2) * (X - X2) + (Y - Y2) * (Y - Y2));
            //  return (short)Math.Max(Math.Abs(X - X2), Math.Abs(Y - Y2));
            //short x = 0;
            //short y = 0;
            //if (X >= X2) x = (short)(X - X2);
            //else if (X2 >= X) x = (short)(X2 - X);
            //if (Y >= Y2) y = (short)(Y - Y2);
            //else if (Y2 >= Y) y = (short)(Y2 - Y);
            //if (x > y) return x;
            //else return y;
        }
        public static uint GetDamage12(uint MaxAttack, uint MinAttack)
        {
            return MinAttack;
            if (MaxAttack == MinAttack)
                MaxAttack += 1;

            if (MaxAttack < MinAttack)
                MaxAttack = MinAttack + 1;
            // MinAttack = MaxAttack + 1;

            int nMaxRand = 50;
            //   if (MyRandom.Next(100) < nMaxRand)
            //    return (uint)(MaxAttack - MyRandom.Next((int)(MaxAttack - MinAttack)) / 2);
            //  else
            return (uint)(MinAttack + (int)MyRandom.Next((int)(MaxAttack - MinAttack)) / 2);

        }
        public static uint GetDamage(uint MaxAttack, uint MinAttack)
        {
            //int nMaxRand = 50;
            uint nAttack = (uint)MyRandom.Next((int)MinAttack, (int)MaxAttack);
            //if (MyRandom.Next(100) < nMaxRand)
            //    nAttack = MaxAttack - (uint)MyRandom.Next((int)((MaxAttack - MinAttack) / 2));
            //else
            //    nAttack = MinAttack + (uint)MyRandom.Next((int)(MaxAttack - MinAttack) / 2);

            // if (MaxAttack == MinAttack)
            //     MaxAttack += 1;
            //
            // if (MaxAttack < MinAttack)
            //     MaxAttack = MinAttack + 1;
            // MinAttack = MaxAttack + 1;

            //int nMaxRand = 50;
            //   if (MyRandom.Next(100) < nMaxRand)
            //    return (uint)(MaxAttack - MyRandom.Next((int)(MaxAttack - MinAttack)) / 2);
            //  else
            //return (uint)(MinAttack + (int)MyRandom.Next((int)(MaxAttack - MinAttack)) / 2);
            return nAttack;
        }

        public static uint CalculateBless(uint Damage, uint bless)
        {
            uint BDamage = Damage * bless / 50;
            if (Damage > BDamage)
                Damage -= BDamage;
            else
                Damage = 1;
            return Damage;
        }
        public static uint CalcaulateDeffence(uint Damage, uint Defense)
        {
            if (Damage > Defense)
                Damage -= Defense;
            else
                Damage = 1;
            return Damage;
        }
        public static uint CalculateExtraAttack(uint Damage, uint pattack, uint pdeffence)
        {
            Damage += pattack;
            if (Damage > pdeffence)
                Damage -= pdeffence;
            else
                Damage = 1;
            return Damage;

        }

        internal static uint CalculateSoul(uint damage, float damagePersent)
        {
            damage = (uint)(damage * damagePersent);
            return damage;
        }

        public static uint GetFinalDmg(uint Damage, uint mindmg, uint maxdmag)
        {
            if (Damage > maxdmag)
            {
                Damage = GetDamage(mindmg, maxdmag);
            }
            return Damage;
        }
        public static uint CalculateSoul(uint Damage, byte LevelSoul)
        {
            //Damage += (uint)(Damage / 100);
            return Damage;
        }
        public static uint CalculateArtefactsDmg(uint Damage, uint AtackerPercent, uint TargetPercent)
        {
            if (AtackerPercent == TargetPercent)
                return Damage;

            if (AtackerPercent > TargetPercent)
            {
                uint Power = AtackerPercent - TargetPercent;
                Damage += (uint)MulDiv((int)Damage, (int)(Power / 100), 100);
            }
            return Damage;
        }
        public static int CalculatePotencyDamage(int Damage, int AttackerBattle, int TargetBattle, bool range = false)
        {
            if (AttackerBattle == TargetBattle)
                return Damage;
            int power = AttackerBattle - TargetBattle;
            if (power != 0)
            {

                power = power * (range ? 10 : 3);
                //3
                if (power > 0)
                {
                    power = Math.Min(60, power);
                }
                else if (power < 0)
                {
                    power = Math.Max(-30, power);
                }

                Damage = Base.MulDiv(Damage, 100 + power, 100);
            }
            return Damage;
        }
        public static int CalculateSoulsDamage(int Damage, int AttackerBattle, int TargetBattle)
        {
            if (AttackerBattle == TargetBattle)
                return Damage;
            int power = AttackerBattle - TargetBattle;
            if (power != 0)
            {
                if (power > 0)
                {
                    power = Math.Min(60, power);
                }
                else if (power < 0)
                {
                    power = Math.Max(-60, power);
                }
                Damage = Base.MulDiv(Damage, 100 + power, 100);
            }
            return Damage;
        }
        public static uint CalculateHealtDmg(uint Damage, uint MaxHitPoints, uint MinHitPoints)
        {
            if (MaxHitPoints == MinHitPoints)
                return 0;
            if (MaxHitPoints > MinHitPoints)
            {
                uint deference = MaxHitPoints - MinHitPoints;
                if (deference >= Damage)
                    return Damage;
                else
                    return deference;
            }
            else
                return 0;
        }
    }
}
