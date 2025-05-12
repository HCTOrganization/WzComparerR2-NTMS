using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using AES = System.Security.Cryptography.Aes;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

#if NET6_0_OR_GREATER
using MapleStory.OpenAPI;
using System.Net.Http;
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
                System.Diagnostics.Debug.Write(c.Name);
                System.Diagnostics.Debug.Write(" ");
                System.Diagnostics.Debug.WriteLine(Convert.ToString(c.Value, 2).PadLeft(c.Bits, '0'));
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
                        msg = e.Message；
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
            { 26, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 2), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown4", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown5", 32), new DataInfo("unknown6", 23), new DataInfo("ringID1", 10), new DataInfo("ringGender1", 4), new DataInfo("ringID2", 10), new DataInfo("ringGender2", 4), new DataInfo("ringID3", 10), new DataInfo("ringGender3", 4), new DataInfo("ringID4", 10), new DataInfo("ringGender4", 4) } },
            { 27, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 2), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown4", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown5", 32), new DataInfo("unknown6", 23), new DataInfo("ringID1", 10), new DataInfo("ringGender1", 4), new DataInfo("ringID2", 10), new DataInfo("ringGender2", 4), new DataInfo("ringID3", 10), new DataInfo("ringGender3", 4), new DataInfo("ringID4", 10), new DataInfo("ringGender4", 4) } },
            { 28, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 2), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown4", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown5", 32), new DataInfo("unknown6", 23), new DataInfo("ringID1", 10), new DataInfo("ringGender1", 4), new DataInfo("ringID2", 10), new DataInfo("ringGender2", 4), new DataInfo("ringID3", 10), new DataInfo("ringGender3", 4), new DataInfo("ringID4", 10), new DataInfo("ringGender4", 4) } },
            { 29, new List<DataInfo>() {new DataInfo("gender", 1), new DataInfo("skinID", 10), new DataInfo("face50k", 1), new DataInfo("faceID", 10), new DataInfo("faceGender", 4), new DataInfo("hair10k", 4), new DataInfo("hairID", 10), new DataInfo("hairGender", 4), new DataInfo("capID", 10), new DataInfo("capGender", 3), new DataInfo("faceAccID", 10), new DataInfo("faceAccGender", 2), new DataInfo("eyeAccID", 10), new DataInfo("eyeAccGender", 2), new DataInfo("earAccID", 10), new DataInfo("earAccGender", 2), new DataInfo("isLongCoat", 1), new DataInfo("coatID", 10), new DataInfo("coatGender", 4), new DataInfo("pantsID", 10), new DataInfo("pantsGender", 2), new DataInfo("shoesID", 10), new DataInfo("shoesGender", 4), new DataInfo("glovesID", 10), new DataInfo("glovesGender", 2), new DataInfo("capeID", 10), new DataInfo("capeGender", 2), new DataInfo("isNotBlade", 1), new DataInfo("isSubWeapon", 1), new DataInfo("shieldID", 10), new DataInfo("shieldGender", 4), new DataInfo("isCashWeapon", 1), new DataInfo("weaponID", 10), new DataInfo("weaponGender", 2), new DataInfo("weaponType", 8), new DataInfo("earType", 4), new DataInfo("mixHairColor", 4), new DataInfo("mixHairRatio", 7), new DataInfo("unknown4", 1), new DataInfo("mixFaceInfo", 10), new DataInfo("unknown5", 32), new DataInfo("unknown6", 23), new DataInfo("ringID1", 10), new DataInfo("ringGender1", 4), new DataInfo("ringID2", 10), new DataInfo("ringGender2", 4), new DataInfo("ringID3", 10), new DataInfo("ringGender3", 4), new DataInfo("ringID4", 10), new DataInfo("ringGender4", 4) } },
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
