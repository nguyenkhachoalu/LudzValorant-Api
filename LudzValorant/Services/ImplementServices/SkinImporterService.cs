using HtmlAgilityPack;
using LudzValorant.Entities;
using LudzValorant.Helpers;
using LudzValorant.Payloads.ResponseModels.DataAccount;
using LudzValorant.Payloads.Responses;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.Services.InterfaceServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LudzValorant.Services.ImplementServices
{
    public class SkinImporterService : ISkinImporterService
    {
        private readonly HttpClient _httpClient;
        private readonly IBaseRepository<Skin> _baseSkinRepository;
        private readonly IBaseRepository<Tier> _baseTierRepository;
        private readonly IBaseRepository<Weapon> _baseWeaponRepository;
        private readonly IBaseRepository<GunBuddy> _baseGunBuddyRepository;
        private readonly IBaseRepository<Contract> _baseContractRepository;
        private readonly IBaseRepository<Agent> _baseAgentRepository;
        private readonly string _imageSavePath;

        public SkinImporterService(HttpClient httpClient, IBaseRepository<Skin> baseSkinRepository, IBaseRepository<Tier> baseTierRepository, IBaseRepository<Weapon> baseWeaponRepository, IBaseRepository<GunBuddy> baseGunBuddyRepository, IBaseRepository<Contract> baseContractRepository, IBaseRepository<Agent> baseAgentRepository)
        {
            _httpClient = httpClient;
            _baseSkinRepository = baseSkinRepository;
            _baseTierRepository = baseTierRepository;
            _baseWeaponRepository = baseWeaponRepository;
            _baseGunBuddyRepository = baseGunBuddyRepository;
            _baseContractRepository = baseContractRepository;
            _baseAgentRepository = baseAgentRepository;
            _imageSavePath = Path.Combine("wwwroot", "images", "skins");
            Directory.CreateDirectory(_imageSavePath);
        }
        public async Task ImportAllSkinsAsync()
        {
            await ImportAllTiersFromValorantApi();
            await ImportAllWeaponsFromValorantApi();
            await ImportGunBuddies();
            await ImportContracts();
            await ImportAgents();

            int skinCreated = 0, skinUpdated = 0;
            var valorantApiUrl = "https://valorant-api.com/v1/weapons";
            var response = await _httpClient.GetStringAsync(valorantApiUrl);
            using var doc = JsonDocument.Parse(response);

            var weaponData = doc.RootElement.GetProperty("data");
            foreach (var weapon in weaponData.EnumerateArray())
            {
                var weaponId = weapon.GetProperty("uuid").GetString();
                var weaponName = weapon.GetProperty("displayName").GetString();
                var skins = weapon.GetProperty("skins");

                foreach (var skin in skins.EnumerateArray())
                {
                    await Task.Delay(200);

                    var skinId = skin.GetProperty("uuid").GetString();
                    var name = skin.GetProperty("displayName").GetString();
                    var icon = skin.TryGetProperty("displayIcon", out var iconProp) ? iconProp.GetString() : null;
                    var tierId = skin.TryGetProperty("contentTierUuid", out var tierIdProp) ? tierIdProp.GetString() : null;

                    var price = await GetPriceFromOpgg(name, weaponName);

                    if (string.IsNullOrWhiteSpace(tierId))
                    {
                        continue;
                    }

                    var skinEntity = await _baseSkinRepository.GetByIdAsync(skinId);
                    if (skinEntity == null)
                    {
                        skinEntity = new Skin
                        {
                            Id = skinId,
                            DisplayName = name,
                            DisplayIcon = icon,
                            WeaponId = weaponId,
                            Price = price,
                            TierId = tierId,
                        };
                        await _baseSkinRepository.CreateAsync(skinEntity);
                        skinCreated++;
                    }
                }
            }
        }

        private async Task ImportGunBuddies()
        {
            var response = await _httpClient.GetStringAsync("https://valorant-api.com/v1/buddies");
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");

            foreach (var item in data.EnumerateArray())
            {
                var id = item.GetProperty("uuid").GetString();
                var name = item.GetProperty("displayName").GetString();
                var icon = item.GetProperty("displayIcon").GetString();

                if (await _baseGunBuddyRepository.GetByIdAsync(id) == null)
                {
                    await _baseGunBuddyRepository.CreateAsync(new GunBuddy
                    {
                        Id = id,
                        DisplayName = name,
                        DisplayIcon = icon
                    });
                }
            }
        }

        private async Task ImportContracts()
        {
            var response = await _httpClient.GetStringAsync("https://valorant-api.com/v1/contracts");
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");

            foreach (var item in data.EnumerateArray())
            {
                var id = item.GetProperty("uuid").GetString();
                var name = item.GetProperty("displayName").GetString();

                if (await _baseContractRepository.GetByIdAsync(id) == null)
                {
                    await _baseContractRepository.CreateAsync(new Contract
                    {
                        Id = id,
                        DisplayName = name
                    });
                }
            }
        }

        private async Task ImportAgents()
        {
            var response = await _httpClient.GetStringAsync("https://valorant-api.com/v1/agents");
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");

            foreach (var item in data.EnumerateArray())
            {
                var id = item.GetProperty("uuid").GetString();
                var name = item.GetProperty("displayName").GetString();
                var portrait = item.GetProperty("fullPortrait").GetString();
                if (await _baseAgentRepository.GetByIdAsync(id) == null)
                {
                    await _baseAgentRepository.CreateAsync(new Agent
                    {
                        Id = id,
                        DisplayName = name,
                        FullPortrait = portrait
                    });
                }
            }
        }

        private async Task ImportAllTiersFromValorantApi()
        {
            var url = "https://valorant-api.com/v1/contenttiers";
            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var tiers = doc.RootElement.GetProperty("data");

            foreach (var tier in tiers.EnumerateArray())
            {
                var id = tier.GetProperty("uuid").GetString();
                var name = tier.GetProperty("displayName").GetString();
                var icon = tier.GetProperty("displayIcon").GetString();

                var exists = await _baseTierRepository.GetByIdAsync(id);
                if (exists != null) continue;

                var newTier = new Tier
                {
                    Id = id,
                    Name = name,
                    TierImage = icon
                };

                await _baseTierRepository.CreateAsync(newTier);
            }
        }

        private async Task ImportAllWeaponsFromValorantApi()
        {
            var url = "https://valorant-api.com/v1/weapons";
            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var weapons = doc.RootElement.GetProperty("data");

            foreach (var weapon in weapons.EnumerateArray())
            {
                var id = weapon.GetProperty("uuid").GetString();
                var name = weapon.GetProperty("displayName").GetString();

                var exists = await _baseWeaponRepository.GetByIdAsync(id);
                if (exists != null) continue;

                var newWeapon = new Weapon
                {
                    Id = id,
                    Name = name
                };

                await _baseWeaponRepository.CreateAsync(newWeapon);
            }
        }

        private async Task<decimal> GetPriceFromOpgg(string skinName, string weaponName)
        {
            var url = $"https://op.gg/valorant/skins?weapon={Uri.EscapeDataString(weaponName)}";

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }

            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var skinNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'rounded bg-darkpurple-850')]");
            if (skinNodes == null)
            {
                Console.WriteLine("[ERROR] Skin nodes not found for weapon " + weaponName);
                return 0;
            }

            foreach (var node in skinNodes)
            {
                var nameNode = node.SelectSingleNode(".//span[@class='truncate']");
                var priceNode = node.SelectSingleNode(".//span[contains(@class,'text-darkpurple-300')]");

                if (nameNode?.InnerText.Trim() == skinName)
                {
                    if (priceNode != null && decimal.TryParse(priceNode.InnerText.Replace(",", "").Trim(), out var parsedPrice))
                    {
                        return parsedPrice;
                    }
                }
            }

            return 0;
        }

        public async Task<ResponseObject<DataResponseGetApiAccount>> GetAccountByUrlRiot(string url)
        {
            try
            {
                string accessToken = RiotApiHelper.ExtractAccessToken(url);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return new ResponseObject<DataResponseGetApiAccount>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Access token không hợp lệ hoặc thiếu.",
                        Data = null
                    };
                }
                var userInfo = await RiotApiHelper.GetUserInfo(accessToken);
                string entitlementToken = await RiotApiHelper.GetEntitlementToken(accessToken);
                string clientVersion = await RiotApiHelper.GetClientVersion();



                var playerCard = await RiotApiHelper.GetPlayerCardId(
                    accessToken,
                    entitlementToken,
                    userInfo.RiotPuuid,
                    clientVersion
                );
                var ListItemIds = await RiotApiHelper.GetAllOwnedItems(
                accessToken, entitlementToken, userInfo.RiotPuuid, playerCard.CorrectShard, clientVersion);

                var accountData = new DataResponseGetApiAccount
                {
                    RiotPuuid = userInfo.RiotPuuid,
                    GameName = userInfo.GameName,
                    PlayerCardImage = playerCard.PlayerCardImage,
                    TagLine = userInfo.TagLine,
                    Shard = playerCard.CorrectShard,
                    Region = userInfo.Region,
                    ExpireTime = DateTime.UtcNow.AddDays(1),
                    SkinIds = ListItemIds.SkinIds.ToList(),
                    AgentIds = ListItemIds.AgentIds.ToList(),
                    ContractIds = ListItemIds.ContractIds.ToList(),
                    GunBuddyIds = ListItemIds.GunBuddyIds.ToList(),
                };

                return new ResponseObject<DataResponseGetApiAccount>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Lấy thông tin tài khoản thành công.",
                    Data = accountData
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseGetApiAccount>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi khi lấy thông tin tài khoản: {ex.Message}",
                    Data = null
                };
            }
        }



    }
}
