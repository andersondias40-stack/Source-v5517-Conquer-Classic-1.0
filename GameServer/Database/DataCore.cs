namespace COServer.Database
{
    public class DataCore
    {
        public static AtributesStatus AtributeStatus = new AtributesStatus();


        public static void LoadClient(Role.Player player)
        {

            player.Owner.Inventory = new Role.Instance.Inventory(player.Owner);
            player.Owner.Equipment = new Role.Instance.Equip(player.Owner);
            player.Owner.Warehouse = new Role.Instance.Warehouse(player.Owner);
            player.Owner.MyProfs = new Role.Instance.Proficiency(player.Owner);
            player.Owner.MySpells = new Role.Instance.Spell(player.Owner);
        }
    }
}
