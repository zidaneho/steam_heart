using System.Text.Json;
using SteamScout.Models;

namespace SteamAlertsAPI.Services
{
    public interface ISteamService
    {
        Task<SteamGameData> GetGameDetailsAsync(int appid);
    }
    public class SteamService : ISteamService
    {
        private readonly HttpClient httpClient;
        //api.steampowered.com<interface name>/<method name>/v1/?key=<api key>&format=<format>
        public SteamService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<SteamGameData> GetGameDetailsAsync(int appid)
        {
            string url = $"https://store.steampowered.com/api/appdetails/?appids={appid}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var dict = JsonSerializer.Deserialize<Dictionary<string,SteamAppResponse>>(jsonString);
            if (dict != null && dict.ContainsKey(appid.ToString()))
            {
                var gameInfo = dict[appid.ToString()];
                if (gameInfo.Success)
                {
                    return gameInfo.Data;
                }
            }
            return null;
        }


    }

}
