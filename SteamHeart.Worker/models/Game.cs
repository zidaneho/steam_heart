namespace SteamHeartAPI.Models
{
    public class Game
    {
        //This is for data that does not change over time.
        public int Id { get; set; }
        public string Name {get; set;}
        public int AppId {get; set;}
    }
}
