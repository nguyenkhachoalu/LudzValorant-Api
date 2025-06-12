using LudzValorant.Enums;
using LudzValorant.Payloads.ResponseModels.DataAccount;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LudzValorant.Helpers
{
    public static class RiotApiHelper
    {
        private static Dictionary<string, string> _gunBuddyLevelToBuddyMap = new();

        public static string ExtractAccessToken(string url)
        {
            var fragment = new Uri(url).Fragment;
            int start = fragment.IndexOf("access_token=") + "access_token=".Length;
            int end = fragment.IndexOf('&', start);
            if (start < "access_token=".Length) return string.Empty;
            return end > start ? fragment.Substring(start, end - start) : fragment[start..];
        }



        public static async Task<string> GetEntitlementToken(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://entitlements.auth.riotgames.com/api/token/v1", body);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("entitlements_token").GetString();
        }


        public static async Task<string> GetClientVersion()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("https://valorant-api.com/v1/version");
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("data").GetProperty("riotClientVersion").GetString();
        }

        public static async Task<DataResponseAccountRiotHelper> GetUserInfo(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync("https://auth.riotgames.com/userinfo");
            var content = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var account = new DataResponseAccountRiotHelper
            {
                RiotPuuid = root.GetProperty("sub").GetString(),
                GameName = root.TryGetProperty("acct", out var acct) && acct.TryGetProperty("game_name", out var g) ? g.GetString() : "",
                TagLine = root.TryGetProperty("acct", out var acct2) && acct2.TryGetProperty("tag_line", out var t) ? t.GetString() : "",
                Region = root.TryGetProperty("country", out var r) ? r.GetString() : "ap",
                Skins = new List<DataResponseSkinRiotHelper>(),
                ExpireTime = DateTime.UtcNow.AddDays(30)
            };

            return account;

        }


     


        public static async Task<DataResponseGetInforInApi> GetAllOwnedItems(
            string accessToken, string entitlementToken, string puuid, string shard, string clientVersion)
        {
            var result = new DataResponseGetInforInApi
            {
                SkinIds = new(),
                AgentIds = new(),
                ContractIds = new(),
                GunBuddyIds = new()
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Add("X-Riot-Entitlements-JWT", entitlementToken);
            client.DefaultRequestHeaders.Add("X-Riot-ClientVersion", clientVersion);
            client.DefaultRequestHeaders.Add("X-Riot-ClientPlatform", Convert.ToBase64String(Encoding.UTF8.GetBytes(
                "{\"platformType\":\"PC\",\"platformOS\":\"Windows\",\"platformOSVersion\":\"10.0.19042.1.256.64bit\",\"platformChipset\":\"Unknown\"}")));

            var itemTypes = new Dictionary<string, Action<List<string>>>
            {
                { "e7c63390-eda7-46e0-bb7a-a6abdacd2433", ids => result.SkinIds = ids },
                { "01bb38e1-da47-4e6a-9b3d-945fe4655707", ids => result.AgentIds = ids },
                { "f85cb6f7-33e5-4dc8-b609-ec7212301948", ids => result.ContractIds = ids },
                { "dd3bf334-87f3-40bd-b043-682a57a8dc3a", ids => result.GunBuddyIds = ids }
            };

            var weaponResponse = await client.GetAsync("https://valorant-api.com/v1/weapons");
            var weaponContent = await weaponResponse.Content.ReadAsStringAsync();
            using var weaponDoc = JsonDocument.Parse(weaponContent);
            var idToSkinUuid = new Dictionary<string, string>();
            foreach (var weapon in weaponDoc.RootElement.GetProperty("data").EnumerateArray())
            {
                if (!weapon.TryGetProperty("skins", out var skins)) continue;
                foreach (var skin in skins.EnumerateArray())
                {
                    var skinUuid = skin.GetProperty("uuid").GetString();
                    if (skin.TryGetProperty("chromas", out var chromas))
                    {
                        foreach (var chroma in chromas.EnumerateArray())
                            idToSkinUuid[chroma.GetProperty("uuid").GetString()] = skinUuid;
                    }
                    if (skin.TryGetProperty("levels", out var levels))
                    {
                        foreach (var level in levels.EnumerateArray())
                            idToSkinUuid[level.GetProperty("uuid").GetString()] = skinUuid;
                    }
                    idToSkinUuid[skinUuid] = skinUuid;
                }
            }

            if (_gunBuddyLevelToBuddyMap.Count == 0)
            {
                var buddyResponse = await client.GetFromJsonAsync<JsonDocument>("https://valorant-api.com/v1/buddies");
                foreach (var buddy in buddyResponse.RootElement.GetProperty("data").EnumerateArray())
                {
                    var buddyId = buddy.GetProperty("uuid").GetString();
                    foreach (var level in buddy.GetProperty("levels").EnumerateArray())
                    {
                        var levelId = level.GetProperty("uuid").GetString();
                        _gunBuddyLevelToBuddyMap[levelId] = buddyId;
                    }
                }
            }
            foreach (var (itemType, assignAction) in itemTypes)
            {
                var url = $"https://pd.{shard}.a.pvp.net/store/v1/entitlements/{puuid}/{itemType}";
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) continue;

                using var doc = JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("Entitlements", out var entitlements)) continue;

                var itemIds = new List<string>();
                foreach (var ent in entitlements.EnumerateArray())
                {
                    var itemId = ent.GetProperty("ItemID").GetString();
                    if (itemType == "e7c63390-eda7-46e0-bb7a-a6abdacd2433") // Skin
                    {
                        if (idToSkinUuid.TryGetValue(itemId, out var skinUuid))
                            itemIds.Add(skinUuid);
                    }
                    else if (itemType == "dd3bf334-87f3-40bd-b043-682a57a8dc3a") // Gun Buddy
                    {
                        if (_gunBuddyLevelToBuddyMap.TryGetValue(itemId, out var buddyUuid))
                            itemIds.Add(buddyUuid);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(itemId))
                            itemIds.Add(itemId);
                    }
                }

                assignAction(itemIds.Distinct().ToList());
            }

            return result;
        }

        public static async Task<(string PlayerCardImage, string CorrectShard)> GetPlayerCardId(
    string accessToken,
    string entitlementToken,
    string puuid,
    string clientVersion)
        {
            var shardsToTry = new[] { "ap", "na", "eu", "kr" };

            foreach (var shard in shardsToTry)
            {
                var url = $"https://pd.{shard}.a.pvp.net/personalization/v2/players/{puuid}/playerloadout";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Add("X-Riot-Entitlements-JWT", entitlementToken);
                client.DefaultRequestHeaders.Add("X-Riot-ClientVersion", clientVersion);
                client.DefaultRequestHeaders.Add("X-Riot-ClientPlatform", Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    "{\"platformType\":\"PC\",\"platformOS\":\"Windows\",\"platformOSVersion\":\"10.0.19042.1.256.64bit\",\"platformChipset\":\"Unknown\"}")));

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) continue;

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("Identity", out var identity) &&
                    identity.TryGetProperty("PlayerCardID", out var cardIdProp))
                {
                    var cardId = cardIdProp.GetString();
                    if (!string.IsNullOrEmpty(cardId) && cardId != "00000000-0000-0000-0000-000000000000")
                    {
                        // Gọi đến valorant-api để lấy wideArt
                        var cardResponse = await client.GetAsync($"https://valorant-api.com/v1/playercards/{cardId}");
                        if (!cardResponse.IsSuccessStatusCode) continue;

                        var cardJson = await cardResponse.Content.ReadAsStringAsync();
                        using var cardDoc = JsonDocument.Parse(cardJson);

                        var wideArt = cardDoc.RootElement
                            .GetProperty("data")
                            .GetProperty("wideArt")
                            .GetString();

                        if (!string.IsNullOrEmpty(wideArt))
                        {
                            return (wideArt, shard);
                        }
                    }
                }
            }

            return (string.Empty, "ap"); // fallback nếu không tìm thấy shard nào có data
        }

    }
}
