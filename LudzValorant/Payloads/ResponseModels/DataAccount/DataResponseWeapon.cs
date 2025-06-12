namespace LudzValorant.Payloads.ResponseModels.DataAccount
{
    public class DataResponseWeapon
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<DataResponseSkin> Skins { get; set; }
    }
}
