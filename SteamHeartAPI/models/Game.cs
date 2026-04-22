using System.ComponentModel.DataAnnotations.Schema;

namespace SteamHeartAPI.Models
{
    public class Game
    {
        //This is for data that does not change over time.
        public int Id { get; set; }
        public string Name {get; set;}
        public int AppId {get; set;}

        public string? Genre {get; set;}

        [Column(TypeName = "jsonb")]
        public Dictionary<string,int>? Tags {get; set;}

        public string? ReleaseDate {get; set;}

        public string? Developer {get; set;}

        public string? Publisher {get; set;}

        public string? HeaderImageUrl {get; set;}

    }
}
