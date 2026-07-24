using System;
using System.Collections.Generic;
using System.IO;

namespace COServer.Database
{
    public class MagicType : Dictionary<ushort, Dictionary<ushort, MagicType.Magic>>
    {
        public enum WeaponsType : ushort
        {
            Boxing = 000,
            Blade = 410,
            Sword = 420,
            Backsword = 421,
            Hook = 430,
            Whip = 440,
            Mace = 441,
            Axe = 450,
            Hammer = 460,
            Crutch = 470,
            Club = 480,
            Scepter = 481,
            Dagger = 490,
            Prod = 491,
            Fan = 492,
            Flute = 493,
            Glaive = 510,
            Scythe = 511,
            Epee = 520,
            Zither = 521,
            Lute = 522,
            Poleaxe = 530,
            LongHammer = 540,
            //Scythe = 550,
            Spear = 560,
            Pickaxe = 562,
            Spade = 570,
            Halbert = 580,
            Wand = 561,
            Bow = 500,
            NinjaSword = 601,
            ArcherBow = 1050,

            //Boxing = 700,
            //Blade = 710,
            //Sword = 720,
            MagicSword = 721,
            //Hook = 730,
            //Whip = 740,
            //Mace = 741,
            //Axe = 750,
            //Hammer = 760,
            //Crutch = 770,
            //Club = 780,
            //Scepter = 781,
            //Dagger = 790,
            //Prod = 791,
            //Fan = 792,
            //Flute = 793,
            Shield = 900,
            Other = 422,
            PrayerBeads = 610,
            Rapier = 611,
            Pistol = 612,
            CrossSaber = 614,
            TwistedClaw = 616,
            Nunchaku = 617,
            Fist = 624,
            WindFan = 626
        }
        public enum MagicSort
        {
            Attack = 1,
            Recruit = 2,
            Cross = 3,
            Sector = 4,
            Bomb = 5,
            AttachStatus = 6,
            DetachStatus = 7,
            Square = 8,
            JumpAttack = 9,
            RandomTransport = 10,
            DispatchXp = 11,
            Collide = 12,
            SerialCut = 13,
            Line = 14,
            LINE_PENETRABLE = 33,
            AtkRange = 15,
            AttackStatus = 16,
            CallTeamMember = 17,
            RecordTransportSpell = 18,
            Transform = 19,
            AddMana = 20,
            LayTrap = 21,
            Dance = 22,
            CallPet = 23,
            Vampire = 24,
            Instead = 25,
            DecLife = 26,
            Toxic = 27,
            ShurikenVortex = 28,
            CounterKill = 29,

            Spook = 30,
            WarCry = 31,
            Riding = 32,

            ChainBolt = 37,
            StarArrow = 38,
            DragonWhirl = 40,
            RemoveBuffers = 46,
            Tranquility = 47,
            DirectAttack = 48,
            Compasion = 50,
            Auras = 51,
            ShieldBlock = 52,
            Oblivion = 53,
            WhirlwindKick = 54,//54
            PhysicalSpells = 55,
            ScurvyBomb = 56,
            CannonBarrage = 57,
            BlackSpot = 58,
            Summon = 72,
            BombLine = 60,
            MoveLine = 61,
            AddBlackSpot = 62,
            PirateXpSkill = 63,
            ChargingVortex = 64,
            MortalDrag = 65,
            KineticSpark = 67,
            BladeFlurry = 68,
            SectorBack = 69,
            BreathFocus = 70,
            FatalCross = 71,
            Summons = 72,
            FatalSpin = 73,
            DragonCyclone = 75,
            StraightFist = 76,
            Perimeter = 78,
            AirKick = 79,
            Strike = 81,
            SectorPasive = 82,
            ManiacDance = 83,
            Omnipotence = 85,
            Pounce = 84,
            BurntFrost = 86,

            Rectangle = 87,
            RemoveStamin = 88,
            PetAttachStatus = 89,
            SwirlingStorm = 91
        }

        public static List<Role.Flags.SpellID> RandomSpells = new List<Role.Flags.SpellID>()
        {
           Role.Flags.SpellID.Phoenix, Role.Flags.SpellID.Rage, Role.Flags.SpellID.Boreas, Role.Flags.SpellID.Earthquake
           , Role.Flags.SpellID.Halt,Role.Flags.SpellID.Phoenix
           , Role.Flags.SpellID.Rage, Role.Flags.SpellID.Roamer, Role.Flags.SpellID.Seizer, Role.Flags.SpellID.Snow
           , Role.Flags.SpellID.WideStrike,Role.Flags.SpellID.Phoenix,
            Role.Flags.SpellID.Windstorm, Role.Flags.SpellID.Poison,Role.Flags.SpellID.Phoenix
        };
        public void Load()
        {

            if (File.Exists(Program.ServerConfig.DbLocation + "magictype.txt"))
            {
                using (StreamReader read = File.OpenText(Program.ServerConfig.DbLocation + "magictype.txt"))
                {
                    string Linebas;
                    while ((Linebas = read.ReadLine()) != null)
                    {
                        string[] line = Linebas.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        Magic spell = new Magic();

                        spell.ID = ushort.Parse(line[0]);
                        spell.AttackInFly = AttackInFlay(spell);
                        spell.Type = (MagicSort)byte.Parse(line[1]);
                        spell.Name = line[2];
                        spell.Level = byte.Parse(line[7]);
                        spell.UseMana = ushort.Parse(line[8]);
                        if (spell.ID == (ushort)Role.Flags.SpellID.ToxicFog)
                            spell.Damage = int.Parse(line[9]) % 100;
                        else
                            spell.Damage = int.Parse(line[9]);
                        spell.DamagePersent = (float)(spell.Damage % 1000) / 100;
                        //Console.WriteLine($"SpellID {spell.ID} -- {spell.Name} -- {spell.Damage} --- P {spell.DamagePersent}");
                        //spell.CoolDown = ushort.Parse(line[12]);//(ushort)Math.Max(500, uint.Parse(line[11]));

                        spell.Rate = int.Parse(line[11]);
                        //  spell.Damage = (float)(double.Parse(line[10]));

                        //spell.MaxTargets = byte.Parse(line[14]);
                        spell.Experience = uint.Parse(line[17]);
                        spell.NeedLevel = ushort.Parse(line[18]);
                        spell.WeaponType = uint.Parse(line[20]);
                        spell.UseStamina = byte.Parse(line[27]);
                        if (spell.ID == (ushort)Role.Flags.SpellID.ToxicFog)
                            spell.Duration = 20;
                        else
                            spell.Duration = uint.Parse(line[12]);//15--21
                        spell.UseItem = int.Parse(line[29]);
                        spell.CoolDown = ushort.Parse(line[31]);
                        spell.UseArrows = uint.Parse(line[32]);//33

                        spell.Range = byte.Parse(line[13]);//13

                        if (spell.ID == (ushort)Role.Flags.SpellID.ScatterFire)
                            spell.Sector = (ushort)(105 + spell.Level * 15);
                        else
                            spell.Sector = (ushort)(90 + spell.Level * 5);

                        if (spell.WeaponType != 0)
                        {
                            if (spell.WeaponType > 100000)
                            {
                                ushort profone, proftwo = 0;
                                profone = ushort.Parse(spell.WeaponType.ToString()[0].ToString() + spell.WeaponType.ToString()[1].ToString() + spell.WeaponType.ToString()[2].ToString());
                                proftwo = ushort.Parse(spell.WeaponType.ToString()[3].ToString() + spell.WeaponType.ToString()[4].ToString() + spell.WeaponType.ToString()[5].ToString());
                                if (!Server.WeaponSpells.ContainsKey((ushort)profone))
                                    Server.WeaponSpells.Add((ushort)profone, spell.ID);
                                if (!Server.WeaponSpells.ContainsKey((ushort)proftwo))
                                    Server.WeaponSpells.Add((ushort)proftwo, spell.ID);
                            }
                            else
                                if (!Server.WeaponSpells.ContainsKey((ushort)spell.WeaponType))
                                Server.WeaponSpells.Add((ushort)spell.WeaponType, spell.ID);

                        }
                        if (this.ContainsKey(spell.ID))
                        {
                            if (!this[spell.ID].ContainsKey(spell.Level))
                                this[spell.ID].Add(spell.Level, spell);
                        }
                        else
                        {
                            this.Add(spell.ID, new Dictionary<ushort, Magic>());
                            this[spell.ID].Add(spell.Level, spell);
                        }


                    }
                }
            }
            Console.WriteLine("Loading " + this.Count + " DBSkills");

            if (!Server.WeaponSpells.ContainsKey(480))//rage
                Server.WeaponSpells.Add(480, 7020);

            if (Server.WeaponSpells.ContainsKey(480))//sword1
                Server.WeaponSpells[480] = 7020;
            //.WeaponSpells[480] = 7020;

            if (Server.WeaponSpells.ContainsKey(420))//sword1
                Server.WeaponSpells[420] = 5030;

            if (Server.WeaponSpells.ContainsKey(421))//sword2
                Server.WeaponSpells[421] = 5030;


            if (Server.WeaponSpells.ContainsKey(510))//sword2
                Server.WeaponSpells[510] = 1250;


            if (Server.WeaponSpells.ContainsKey(530))//sword2
                Server.WeaponSpells[530] = 5050;


            if (Server.WeaponSpells.ContainsKey(561))//sword2
                Server.WeaponSpells[561] = 5010;

            if (Server.WeaponSpells.ContainsKey(560))//sword2
                Server.WeaponSpells[560] = 1260;

            if (Server.WeaponSpells.ContainsKey(721))//sword2
                Server.WeaponSpells[721] = 1290;

            if (Server.WeaponSpells.ContainsKey(540))//sword2
                Server.WeaponSpells[540] = 1300;


            if (Server.WeaponSpells.ContainsKey(430))//sword2
                Server.WeaponSpells[430] = 7000;


            if (Server.WeaponSpells.ContainsKey(450))//sword2
                Server.WeaponSpells[450] = 7010;


            if (Server.WeaponSpells.ContainsKey(481))//sword2
                Server.WeaponSpells[481] = 7030;


            if (Server.WeaponSpells.ContainsKey(440))//sword2
                Server.WeaponSpells[440] = 7040;


            if (Server.WeaponSpells.ContainsKey(580))//sword2
                Server.WeaponSpells[580] = 5020;

            if (Server.WeaponSpells.ContainsKey(410))//blade
                Server.WeaponSpells.Remove(410);

            if (Server.WeaponSpells.ContainsKey(900))//shield460
                Server.WeaponSpells.Remove(900);

            if (!Server.WeaponSpells.ContainsKey(460))
                Server.WeaponSpells.Add(460, 5040);


            if (Server.WeaponSpells.ContainsKey(601))//ninja sword
                Server.WeaponSpells.Remove(601);

            Add((ushort)Role.Flags.SpellID.ShurikenEffect, this[(ushort)Role.Flags.SpellID.ShurikenVortex]);
        }
        public bool AttackInFlay(Magic DBSpell)
        {
            if (DBSpell.ID == 1045 || DBSpell.ID == 1046 || DBSpell.ID == 1115
    || DBSpell.ID == 1250 || DBSpell.ID == 1260 || DBSpell.ID == 1290
    || DBSpell.ID >= 5010 && DBSpell.ID <= 5050 || DBSpell.ID == 6000
    || DBSpell.ID == 6001 || DBSpell.ID >= 7000 && DBSpell.ID <= 7040
    || DBSpell.ID == 10315 || DBSpell.ID == 10381
    || DBSpell.ID == 10490 || DBSpell.ID == 11000 || DBSpell.ID == 11005
    || DBSpell.ID == 11040 || DBSpell.ID == 11070 || DBSpell.ID == 11110
    || DBSpell.ID == 11140 || DBSpell.ID == 11170 || DBSpell.ID == 11180
    || DBSpell.ID == 11190 || DBSpell.ID == 11230)
                return false;
            else
                return true;
        }
        public uint GetSpell(string name)
        {
            uint spellid = 0;
            foreach (var spells in Values)
            {
                foreach (var spell in spells.Values)
                {
                    if (spell.Name != null)
                        if (spell.Name.Contains(name))
                            spellid = spell.ID;
                }
            }
            return spellid;
        }
        public class Magic
        {
            public ushort ID;
            public string Name;
            public MagicSort Type;
            public byte Level;
            public ushort UseMana;
            public uint UseArrows;
            public int Damage;
            public float DamagePersent;
            public int Rate = 0;
            public uint Experience;
            public byte Range;
            public ushort Sector;
            public uint Duration;
            public uint WeaponType;
            public byte UseStamina;
            public uint GiveHitPoints;
            public bool AttackInFly;
            public ushort NeedLevel = 0;
            public uint CpsCost = 0;
            public byte MaxTargets = 0;
            public byte IncreaseStamin = 0;
            public ushort CoolDown = 100;
            public int UseItem = 0;
            public uint CPUpgradeRatio;
            public int CustomCoolDown = 200;
        }
    }

}
