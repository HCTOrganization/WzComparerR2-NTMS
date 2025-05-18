using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AES = System.Security.Cryptography.Aes;

#if NET6_0_OR_GREATER
using HtmlAgilityPack;
using MapleStory.OpenAPI;
using System.Net; 
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;


namespace WzComparerR2.OpenAPI
{
    public class NexonOpenAPI
    {
        public NexonOpenAPI(string apiKey)
        {
            APIKey = apiKey;
            if (API == null)
            {
                API = new MapleStoryAPI(apiKey);
            }
        }

        private string APIKey;
        private MapleStoryAPI API;

        public bool CheckSameAPIKey(string apiKey)
        {
            return APIKey == apiKey;
        }

        public async Task<string> GetCharacterOCID(string characterName)
        {
            try
            {
                var character = await API.GetCharacter(characterName);
                return character.OCID;
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    case MapleStoryAPIErrorCode.OPENAPI00004:
                        throw new Exception(Utils.GetExceptionMsg(e, forceMsg: $"角色名不正確。"));
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> isJMSUnderMaintenance()
        {
            try
            {
                using (HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
                {
                    string url = "https://maplestory.nexon.co.jp/maintenance";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return true;
            }
        }

        public async Task<string> GetAvatarCode(string characterName, string region)
        {
            string serviceBackend = "";
            string avatarCode = "";
            string jmsBaseUrl = "https://maplestory.nexon.co.jp";
            switch (region)
            {
                default:
                    return avatarCode;
                case "KMS":
                    serviceBackend = "https://maple.dakgg.io/api/v1/bypass/characters/" + Uri.EscapeDataString(characterName); // Used Maple GG API
                    break;
                case "JMS":
                    serviceBackend = $"{jmsBaseUrl}/community/avatar/search/?writer=" + Uri.EscapeDataString(characterName);
                    break;
                case "GMS-NA":
                    serviceBackend = "https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=weekly&character_name=" + Uri.EscapeDataString(characterName);
                    break;
                case "GMS-EU":
                    serviceBackend = "https://www.nexon.com/api/maplestory/no-auth/ranking/v2/eu?type=overall&id=weekly&character_name=" + Uri.EscapeDataString(characterName);
                    break;
                case "MSEA":
                    serviceBackend = "https://msea.dakgg.io/api/v1/bypass/characters/" + Uri.EscapeDataString(characterName); // Used Maple GG API
                    break;
                case "TMS":
                    serviceBackend = "https://tw-event.beanfun.com/MapleStory/api/UnionWebRank/GetRank";
                    break;
                case "MSN":
                    serviceBackend = "https://msu.io/maplestoryn/api/gateway/msn/ranking/by-name?characterName=" + Uri.EscapeDataString(characterName);
                    break;
            }
            try
            {
                if (region == "JMS")
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                    string html = await client.GetStringAsync(serviceBackend);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var avatarLinks = doc.DocumentNode.SelectNodes("//a[@href]")
                        ?.Select(node => WebUtility.HtmlDecode(node.GetAttributeValue("href", "")))
                        .Where(href => href.StartsWith("/mypage/avatar"))
                        .Select(href => $"{jmsBaseUrl}{href}")
                        .ToList();

                    if (avatarLinks != null && avatarLinks.Count == 2)
                    {
                        string avatarHtml = await client.GetStringAsync(avatarLinks[0]);
                        HtmlDocument avatarDoc = new HtmlDocument();
                        avatarDoc.LoadHtml(avatarHtml);

                        avatarCode = avatarDoc.DocumentNode.SelectNodes("//img[@src]")
                            ?.Select(node => node.GetAttributeValue("src", ""))
                            .FirstOrDefault(src => src.StartsWith("//avatar-maplestory.nexon.co.jp")).Replace("//avatar-maplestory.nexon.co.jp/Character/", "").Replace(".png", "");
                    }
                    else if (avatarLinks.Count > 2)
                    {
                        throw new Exception("請準確輸入角色名。");
                    }
                    else
                    {
                        throw new Exception("找不到角色。請在JMS官方網站上註冊您的代表角色。");
                    }
                }
                else if (region.StartsWith("GMS"))
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                    var response = await client.GetAsync(serviceBackend);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using JsonDocument doc = JsonDocument.Parse(json);
                        JsonElement root = doc.RootElement;
                        JsonElement ranksArray = root.GetProperty("ranks");

                        if (ranksArray.GetArrayLength() > 0)
                        {
                            avatarCode = ranksArray[0].GetProperty("characterImgURL").GetString().Replace("https://msavatar1.nexon.net/Character/", "").Replace(".png", "");
                        }
                    }
                    else
                    {
                        avatarCode = "";
                    }
                }
                else if (region == "MSEA" || region == "KMS")
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                    var response = await client.GetAsync(serviceBackend);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using JsonDocument doc = JsonDocument.Parse(json);
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("data", out JsonElement dataElement) &&
                            dataElement.TryGetProperty("characterBasic", out JsonElement characterBasicElement) &&
                            characterBasicElement.TryGetProperty("character_image", out JsonElement characterImageElement))
                        {
                            avatarCode = characterImageElement.GetString().Replace("https://open.api.nexon.com/static/maplestorysea/character/look/", "").Replace("https://open.api.nexon.com/static/maplestory/character/look/", "");
                        }
                    }
                    else
                    {
                        avatarCode = "";
                    }
                }
                else if (region == "TMS")
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                    string jsonPayload = $"{{\"RankType\":1,\"GameWorldId\":\"-1\",\"CharacterName\":\"{characterName}\"}}";
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(serviceBackend, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using JsonDocument doc = JsonDocument.Parse(json);
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("Data", out JsonElement dataElement) &&
                            dataElement.TryGetProperty("CharacterLookCipherText", out JsonElement characterLookCipherText))
                        {
                            avatarCode = characterLookCipherText.GetString();
                        }
                    }
                    else
                    {
                        avatarCode = "";
                    }
                }
                else if (region == "MSN")
                {
                    EdgeWebView webView = new EdgeWebView();
                    EdgeWebView.webViewUri = serviceBackend;
                    webView.ShowDialog();
                    var json = webView.jsonResult;
                    using JsonDocument doc = JsonDocument.Parse(json);
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("ranking", out JsonElement rankingData) &&
                        rankingData.TryGetProperty("imageUrl", out JsonElement imageUrl))
                    {
                        avatarCode = imageUrl.GetString().Replace("https://market-static.msu.io/msu/platform/charimages/transient/", "").Replace(".png", "");
                    }
                }
            }
            catch
            {
                throw new Exception("找不到角色。");
            }

            return avatarCode;
        }

        public async Task<UnpackedAvatarData> GetAvatarResult(string ocid)
        {
            try
            {
                var basic = await API.GetCharacterBasic(ocid);
                var m = Regex.Match(basic.CharacterImage, @"look/([A-Z]+)$");
                if (m.Success)
                {
                    var data = m.Groups[1].Value;

                    var decrypted = Utils.Decrypt(data);
                    var version = Utils.CheckVer(decrypted);

                    var unpackedData = new UnpackedAvatarData(version);

                    Utils.Unpack(unpackedData, decrypted);
                    unpackedData.SetProperties();

                    return unpackedData;
                }
                return null;
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<UnpackedAvatarData> ParseAvatarCode(string avatarCode)
        {
            try
            {
                var decrypted = Utils.Decrypt(avatarCode);
                var version = Utils.CheckVer(decrypted);

                var unpackedData = new UnpackedAvatarData(version);

                Utils.Unpack(unpackedData, decrypted);
                unpackedData.SetProperties();

                return unpackedData;
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<UnpackedAvatarData> ParseCharacterLookCipherText(string cipherText)
        {
            if (cipherText.StartsWith("0x"))
                cipherText = cipherText.Substring(2);

            int length = cipherText.Length / 2;
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                bytes[i] = Convert.ToByte(cipherText.Substring(i * 2, 2), 16);
            }
            try
            {
                var decrypted = bytes;
                var version = Utils.CheckVer(decrypted);

                var unpackedData = new UnpackedAvatarData(version);

                Utils.Unpack(unpackedData, decrypted);
                unpackedData.SetProperties();

                return unpackedData;
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<LoadedAvatarData> GetAvatarResult2(string ocid)
        {
            var result = new LoadedAvatarData();

            await GetAvatarItems(ocid, result);
            await GetAvatarCashItems(ocid, result);
            await GetAvatarBeautyEquipment(ocid, result);

            return result;
        }

        private async Task GetAvatarItems(string ocid, LoadedAvatarData result)
        {
            try
            {
                result.ItemList = new List<string>();

                var item = await API.GetCharacterItemEquipment(ocid);
                result.Preset = item.PresetNo ?? 0;

                foreach (var it in item.ItemEquipment)
                {
                    var m = Regex.Match(it.ItemIcon, @"icon/([A-Z]+)$");
                    if (m.Success)
                        result.ItemList.Add(Utils.GetItemID(m.Groups[1].Value));
                }
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task GetAvatarCashItems(string ocid, LoadedAvatarData result)
        {
            try
            {
                result.CashBaseItemList = new List<string>();
                result.CashPresetItemList = new List<string>();

                var item = await API.GetCharacterCashItemEquipment(ocid);
                result.CashPreset = item.PresetNo ?? 0;

                foreach (var it in item.CashItemEquipmentBase)
                {
                    var m = Regex.Match(it.CashItemIcon, @"icon/([A-Z]+)$");
                    if (m.Success)
                        result.CashBaseItemList.Add(Utils.GetItemID(m.Groups[1].Value));
                }

                if (result.CashPreset > 0)
                {
                    foreach (var it in result.CashPreset == 1 ? item.CashItemEquipmentPreset1 : result.CashPreset == 2 ? item.CashItemEquipmentPreset2 : item.CashItemEquipmentPreset3)
                    {
                        var m = Regex.Match(it.CashItemIcon, @"icon/([A-Z]+)$");
                        if (m.Success)
                            result.CashPresetItemList.Add(Utils.GetItemID(m.Groups[1].Value));
                    }
                }
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task GetAvatarBeautyEquipment(string ocid, LoadedAvatarData result)
        {
            try
            {
                var item = await API.GetCharacterBeautyEquipment(ocid);

                result.Gender = item.CharacterGender == "남" ? 0 : 1;

                result.HairInfo = new Dictionary<string, string>
                {
                    { "HairName", item.CharacterHair?.HairName ?? "" },
                    { "BaseColor", item.CharacterHair?.BaseColor ?? "" },
                    { "MixColor", item.CharacterHair?.MixColor ?? "" },
                    { "MixRate", item.CharacterHair?.MixRate ?? "0" },
                };

                result.FaceInfo = new Dictionary<string, string>
                {
                    { "FaceName", item.CharacterFace?.FaceName ?? "" },
                    { "BaseColor", item.CharacterFace?.BaseColor ?? "" },
                    { "MixColor", item.CharacterFace?.MixColor ?? "" },
                    { "MixRate", item.CharacterFace?.MixRate ?? "0" },
                };

                result.SkinInfo = new Dictionary<string, string>
                {
                    { "SkinName", item.CharacterSkin?.SkinName ?? "" },
                    { "ColorStyle", item.CharacterSkin?.ColorStyle ?? "" },
                    { "Hue", item.CharacterSkin?.Hue.ToString() ?? "" },
                    { "Saturation", item.CharacterSkin?.Saturation.ToString() ?? "" },
                    { "Brightness", item.CharacterSkin?.Brightness.ToString() ?? "" },
                };
            }
            catch (MapleStoryAPIException e)
            {
                switch (e.ErrorCode)
                {
                    default:
                        throw new Exception(Utils.GetExceptionMsg(e));
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task Debug()
        {
            var data = "";
            var cname = "창섭";
            var ocid = await GetCharacterOCID(cname);
            var basic = await API.GetCharacterBasic(ocid);
            var m = Regex.Match(basic.CharacterImage, @"look/([A-Z]+)$");
            if (m.Success)
                data = m.Groups[1].Value;

            var decrypted = Utils.Decrypt(data);
            var version = Utils.CheckVer(decrypted);

            var str = "";
            for (int i = 0; i < decrypted.Length; i++)
            {
                str += Convert.ToString(decrypted[decrypted.Length - 1 - i], 2).PadLeft(8, '0');
            }

            var result = new UnpackedAvatarData(version);

            Utils.Unpack(result, decrypted);

            foreach (var c in result.Unpacked)
            {
                System.Diagnostics.Debug.Write(c.Name.PadLeft(20, ' '));
                System.Diagnostics.Debug.Write(" ");
                System.Diagnostics.Debug.Write(Convert.ToString(c.Value, 2).PadLeft(c.Bits, '0').PadLeft(32, ' '));
                System.Diagnostics.Debug.Write(" ");
                System.Diagnostics.Debug.WriteLine(c.Value.ToString().PadLeft(10, ' '));
            }

            result.SetProperties();

            return;
        }
    }

    public static class Utils
    {
        public static string GetExceptionMsg(MapleStoryAPIException e, string forceMsg = "", [CallerMemberName] string funcName = "")
        {
            string msg;
            if (!string.IsNullOrEmpty(forceMsg))
            {
                msg = forceMsg;
            }
            else
            {
                switch (e.ErrorCode)
                {
                    case MapleStoryAPIErrorCode.OPENAPI00001:
                        msg = "內部伺服器錯誤";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00002:
                        msg = "您沒有權限。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00003:
                        msg = "請連線並更新您的角色。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00004:
                        msg = "輸入值無效。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00005:
                        msg = "API 金鑰無效。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00006:
                        msg = "無效的遊戲或 API PATH";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00007:
                        msg = "API 呼叫量超出範圍。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00009:
                        msg = "正在準備資料。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00010:
                        msg = "正在檢查遊戲。";
                        break;
                    case MapleStoryAPIErrorCode.OPENAPI00011:
                        msg = "API檢查中。";
                        break;
                    default:
                        msg = e.Message;
                        break;
                }
            }

            return $"{msg} ({e.ErrorCode.ToString()})\r\n地點: {funcName}";
        }

        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }

        public static string GetItemID(string text)
        {
            var id = "";
            int count = 0;
            if (!string.IsNullOrEmpty(text))
            {
                foreach (var c in text)
                {
                    var idx = CharTable[count++].IndexOf(c);
                    id += idx >= 0 ? idx : 0;
                }
            }
            return id;
        }

        // https://github.com/KENNYSOFT
        public static byte[] Decrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;

            byte[] crypt = new byte[data.Length / 2];
            for (int i = 0; i < crypt.Length; i++)
            {
                crypt[i] = (byte)(data[i * 2] - 'A' << 4 | data[i * 2 + 1] - 'A');
            }

            using var aes = AES.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            aes.Key = aesKey;
            aes.IV = iv;

            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(crypt, 0, crypt.Length);
            }
        }

        public static void Unpack(UnpackedAvatarData res, byte[] pack)
        {
            var offset = 0;
            for (int k = 0; k < res.Unpacked.Count; k++)
            {
                int value = 0;
                for (int i = 0; i < res.Unpacked[k].Bits; i++)
                {
                    if ((pack[(offset + i) / 8] & 1 << (offset + i) % 8) != 0)
                    {
                        value |= 1 << i;
                    }
                }
                res.Unpacked[k].Value = value;
                offset += res.Unpacked[k].Bits;

                if (value == 1 && res.Unpacked[k].Name == "isCashWeapon")
                {
                    res.Unpacked.InsertRange(k + 1, new[] { new DataInfo("cashWeaponID", 10), new DataInfo("cashWeaponGender", 2) });
                }
                foreach (var type in new[] { "Cap", "Coat", "Pants", "Shoes", "Gloves", "Cape", "Weapon", "Skin" })
                {
                    if (res.Version >= 27 && value == 1 && res.Unpacked[k].Name == $"has{type}Prism")
                    {
                        res.Unpacked.InsertRange(k + 1, new[] { new DataInfo($"{type.ToLower()}PrismColorType", 3), new DataInfo($"{type.ToLower()}PrismBrightness", 8), new DataInfo($"{type.ToLower()}PrismSaturation", 8), new DataInfo($"{type.ToLower()}PrismHue", 9) });
                    }
                }
            }
            return;
        }

        public static int CheckVer(byte[] pack)
        {
            return pack[pack.Length - 9];
        }

        private static readonly byte[] aesKey = {0x10, 0x04, 0x3F, 0x11,
                                        0x17, 0xCD, 0x12, 0x15,
                                        0x5D, 0x8E, 0x7A, 0x19,
                                        0x80, 0x11, 0x4F, 0x14 };

        private static readonly byte[] iv = {0x11, 0x17, 0xCD, 0x10,
                                    0x04, 0x3F, 0x8E, 0x7A,
                                    0x12, 0x15, 0x80, 0x11,
                                    0x5D, 0x19, 0x4F, 0x10 };

        public static readonly Dictionary<int, List<DataInfo>> Structure = new Dictionary<int, List<DataInfo>>
        {
            // not fully decoded yet
            { 26, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 2), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown1", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown2", 2), new DataInfo("jobWingTail", 2), new DataInfo("unknown3", 30), new DataInfo("weaponMotionType", 2), new DataInfo("unknown4", 11) } },
            { 27, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 2), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown1", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown2", 2), new DataInfo("jobWingTail", 2), new DataInfo("unknown3", 30), new DataInfo("weaponMotionType", 2), new DataInfo("unknown4", 11), new DataInfo("hasCapPrism", 1), new DataInfo("hasCoatPrism", 1), new DataInfo("hasPantsPrism", 1), new DataInfo("hasShoesPrism", 1), new DataInfo("hasGlovesPrism", 1), new DataInfo("hasCapePrism", 1), new DataInfo("hasWeaponPrism", 1), new DataInfo("hasSkinPrism", 1) } },
            { 28, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 2), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown1", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown2", 2), new DataInfo("jobWingTail", 2), new DataInfo("unknown3", 30), new DataInfo("weaponMotionType", 2), new DataInfo("unknown4", 11), new DataInfo("hasCapPrism", 1), new DataInfo("hasCoatPrism", 1), new DataInfo("hasPantsPrism", 1), new DataInfo("hasShoesPrism", 1), new DataInfo("hasGlovesPrism", 1), new DataInfo("hasCapePrism", 1), new DataInfo("hasWeaponPrism", 1), new DataInfo("hasSkinPrism", 1), new DataInfo("ringID1", 10), new DataInfo("ringGender1", 4), new DataInfo("ringID2", 10), new DataInfo("ringGender2", 4), new DataInfo("ringID3", 10), new DataInfo("ringGender3", 4), new DataInfo("ringID4", 10), new DataInfo("ringGender4", 4), new DataInfo("unknown5", 32), new DataInfo("unknown6", 32), new DataInfo("unknown7", 32), new DataInfo("unknown8", 16) } },
            { 29, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 4), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown1", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown2", 2), new DataInfo("jobWingTail", 2), new DataInfo("unknown3", 30), new DataInfo("weaponMotionType", 2), new DataInfo("unknown4", 11), new DataInfo("hasCapPrism", 1), new DataInfo("hasCoatPrism", 1), new DataInfo("hasPantsPrism", 1), new DataInfo("hasShoesPrism", 1), new DataInfo("hasGlovesPrism", 1), new DataInfo("hasCapePrism", 1), new DataInfo("hasWeaponPrism", 1), new DataInfo("hasSkinPrism", 1), new DataInfo("ringID1", 10), new DataInfo("ringGender1", 4), new DataInfo("ringID2", 10), new DataInfo("ringGender2", 4), new DataInfo("ringID3", 10), new DataInfo("ringGender3", 4), new DataInfo("ringID4", 10), new DataInfo("ringGender4", 4), new DataInfo("unknown5", 32), new DataInfo("unknown6", 32), new DataInfo("unknown7", 32), new DataInfo("unknown8", 16) } },
        };

        public static readonly int[] WeaponsKMS = { -1, 130, 131, 132, 133, 137, 138, 140, 141, 142,
            143, 144, 145, 146, 147, 148, 149, -1, 134, 152,
            153, -1, 136, 121, 122, 123, 124, 156, 157, 126,
            158, 127, 128, 159, 129, 121, 1214, 1404 };


        // https://github.com/HikariCalyx/WzComparerR2-JMS/blob/d9f2dab7691c5f7b9989c36a40186f1b6f3a9bf7/README.md
        private static readonly string[] CharTable = new string[]
        {
            "KL________",
            "FEHGBA____",
            "PONMLKJIHG",
            "CDABGHEFKL",
            "LKJIPONMDC",
            "HGFEDCBAPO",
            "OPMNKLIJGH",
            "BADCFEHGJI",
        };
    }

    public class DataInfo
    {
        public DataInfo(string name, int bits)
        {
            Name = name;
            Bits = bits;
        }

        public string Name { get; set; }
        public int Bits { get; set; }
        public int Value { get; set; }
    }
}
#endif
