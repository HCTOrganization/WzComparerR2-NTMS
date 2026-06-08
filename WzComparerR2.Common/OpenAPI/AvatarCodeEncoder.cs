using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WzComparerR2.OpenAPI
{
    /// <summary>
    /// Encodes equipment CSV data into MapleStory avatar codes.
    /// Direct port of the verified Python implementation.
    /// </summary>
    public class AvatarCodeEncoder
    {
        private const int AVATAR_BYTES = 128;
        private const int AVATAR_VERSION = 40;
        private const int DEFAULT_ID = 1023;

        private static readonly byte[] AES_KEY = { 0x10, 0x04, 0x3F, 0x11, 0x17, 0xCD, 0x12, 0x15, 0x5D, 0x8E, 0x7A, 0x19, 0x80, 0x11, 0x4F, 0x14 };
        private static readonly byte[] AES_IV = { 0x11, 0x17, 0xCD, 0x10, 0x04, 0x3F, 0x8E, 0x7A, 0x12, 0x15, 0x80, 0x11, 0x5D, 0x19, 0x4F, 0x10 };

        private static readonly int[] WEAPONS_KMS = { -1, 130, 131, 132, 133, 137, 138, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, -1,
                                                       134, 152, 153, -1, 136, 121, 122, 123, 124, 156, 157, 126, 158, 127, 128, 159, 129, 121,
                                                       1214, 1404, 1215 };

        public string ConvertCsvToAvatarCode(string equipIdCsv)
        {
            var avatar = CreateDefaultAvatar();
            ApplyEquipIdCsv(avatar, equipIdCsv);
            var bytes = PackAvatarBytes(avatar);
            var encrypted = AesEncryptNoPadding(bytes);
            return BytesToAvatarCode(encrypted);
        }

        private Dictionary<string, object> CreateDefaultAvatar()
        {
            return new Dictionary<string, object>
            {
                ["version"] = AVATAR_VERSION,
                ["values"] = new Dictionary<string, int>
                {
                    ["gender"] = 0,
                    ["skinID"] = DEFAULT_ID,
                    ["face10k"] = 0,
                    ["faceID"] = DEFAULT_ID,
                    ["faceGender"] = 0,
                    ["hair10k"] = 0,
                    ["hairID"] = DEFAULT_ID,
                    ["hairGender"] = 0,
                    ["capID"] = DEFAULT_ID,
                    ["capGender"] = 0,
                    ["faceAccID"] = DEFAULT_ID,
                    ["faceAccGender"] = 0,
                    ["eyeAccID"] = DEFAULT_ID,
                    ["eyeAccGender"] = 0,
                    ["earAccID"] = DEFAULT_ID,
                    ["earAccGender"] = 0,
                    ["isLongCoat"] = 0,
                    ["coatID"] = DEFAULT_ID,
                    ["coatGender"] = 0,
                    ["pantsID"] = DEFAULT_ID,
                    ["pantsGender"] = 0,
                    ["shoesID"] = DEFAULT_ID,
                    ["shoesGender"] = 0,
                    ["glovesID"] = DEFAULT_ID,
                    ["glovesGender"] = 0,
                    ["capeID"] = DEFAULT_ID,
                    ["capeGender"] = 0,
                    ["subWeaponType"] = 0,
                    ["shieldID"] = DEFAULT_ID,
                    ["shieldGender"] = 0,
                    ["isCashWeapon"] = 0,
                    ["cashWeaponID"] = DEFAULT_ID,
                    ["cashWeaponGender"] = 0,
                    ["weaponID"] = DEFAULT_ID,
                    ["weaponGender"] = 0,
                    ["weaponType"] = 0,
                    ["earType"] = 0,
                    ["mixHairColor"] = 0,
                    ["mixHairRatio"] = 0,
                    ["mixFaceInfo"] = 0,
                    ["unknown1"] = 0,
                    ["jobWingTailType"] = 0,
                    ["jobWingTailTypeDetail"] = 0,
                    ["unknown2"] = 0,
                    ["eventJob"] = 0,
                    ["unknown2_2"] = 0,
                    ["weaponMotionType"] = 0,
                    ["unknown3"] = 0,
                    ["showEffectFlags"] = 0,
                    ["unknown3_2"] = 0,
                    ["emotionFaceAccID"] = DEFAULT_ID,
                    ["emotionFaceAccGender"] = 0,
                    ["ringID1"] = DEFAULT_ID,
                    ["ringGender1"] = 0,
                    ["ringID2"] = DEFAULT_ID,
                    ["ringGender2"] = 0,
                    ["ringID3"] = DEFAULT_ID,
                    ["ringGender3"] = 0,
                    ["ringID4"] = DEFAULT_ID,
                    ["ringGender4"] = 0,
                }
            };
        }

        private void ApplyEquipIdCsv(Dictionary<string, object> avatar, string rawCsv)
        {
            var fields = rawCsv.Split(',').Select(f => f.Trim()).ToArray();
            if (fields.Length < 16)
                throw new ArgumentException($"Equipment ID list requires at least 16 fields, got {fields.Length}");

            var v = (Dictionary<string, int>)avatar["values"];
            v["mixFaceInfo"] = 0;
            v["mixHairColor"] = 0;
            v["mixHairRatio"] = 0;

            void ApplyField(int index, string slot, bool allowFormula = false)
            {
                if (index >= fields.Length) return;
                string field = fields[index].Trim();
                if (string.IsNullOrEmpty(field)) return;

                // Parse formulas like "1001+1*50"
                if (field.Contains("+") && allowFormula)
                {
                    var parts = field.Split('+');
                    var itemIdStr = parts[0].Trim();
                    var formulaStr = parts.Length > 1 ? parts[1].Trim() : "";
                    
                    if (formulaStr.Contains("*"))
                    {
                        try
                        {
                            var formula = formulaStr.Split('*');
                            int mixColor = int.Parse(formula[0]);
                            int mixRatio = int.Parse(formula[1]);
                            
                            if (slot == "face")
                                v["mixFaceInfo"] = mixColor * 100 + mixRatio;
                            else if (slot == "hair")
                            {
                                v["mixHairColor"] = mixColor;
                                v["mixHairRatio"] = mixRatio;
                            }
                        }
                        catch { }
                    }
                    field = itemIdStr;
                }

                Process(field, v, slot);
            }

            // Process fields in order
            ApplyField(1, "skin");
            ApplyField(2, "face", true);
            ApplyField(3, "hair", true);
            ApplyField(4, "cap");
            ApplyField(5, "coat");
            ApplyField(6, "coat");
            ApplyField(7, "pants");
            ApplyField(8, "shoes");
            ApplyField(9, "gloves");
            ApplyField(10, "shield");
            ApplyField(11, "cape");

            // Weapon handling
            bool hasCashWeapon = false;
            bool hasRegularWeapon = false;

            if (fields.Length > 12 && !string.IsNullOrEmpty(fields[12]))
            {
                string weapon = fields[12];
                if (weapon.StartsWith("170"))
                {
                    hasCashWeapon = true;
                    Process(weapon, v, "cashWeapon");
                }
                else
                {
                    Process(weapon, v, "weapon");
                    hasRegularWeapon = IsRegularWeapon(weapon);
                }
            }

            if (hasCashWeapon && !hasRegularWeapon)
                Process("1372243", v, "weapon");

            // Accessories
            ApplyField(13, "earAcc");
            ApplyField(14, "faceAcc");
            ApplyField(15, "eyeAcc");

            // Rings
            for (int i = 1; i <= 4; i++)
            {
                int idx = 24 + i;
                if (idx < fields.Length && !string.IsNullOrEmpty(fields[idx]))
                    ApplyField(idx, $"ring{i}");
            }
        }

        private bool IsRegularWeapon(string idStr)
        {
            try
            {
                int id = int.Parse(idStr.Split('+')[0]);
                return id >= 1210000 && id <= 1689999;
            }
            catch
            {
                return false;
            }
        }

        private void Process(string raw, Dictionary<string, int> v, string slot)
        {
            if (string.IsNullOrEmpty(raw)) return;

            var idPart = raw.Split('+')[0].Trim();
            if (!int.TryParse(idPart, out int id)) return;

            int prefix = id / 10000;
            int gender = (id % 10000) / 1000;
            int baseId = id % 1000;

            // Skin
            if (prefix == 0 || prefix == 1)
                v["skinID"] = baseId;
            // Face
            else if (prefix == 2 || prefix == 5 || prefix == 8)
            {
                v["faceID"] = baseId;
                v["faceGender"] = gender;
                v["face10k"] = (prefix == 5) ? 1 : 0;
            }
            // Hair
            else if (prefix == 3 || prefix == 4 || prefix == 6 || prefix == 7)
            {
                v["hairID"] = baseId;
                v["hairGender"] = gender;
                v["hair10k"] = prefix;
            }
            // Cap
            else if (prefix == 100)
            {
                v["capID"] = baseId;
                v["capGender"] = gender;
            }
            // Face Accessory
            else if (prefix == 101)
            {
                if (slot == "emotionFaceAcc")
                {
                    v["emotionFaceAccID"] = baseId;
                    v["emotionFaceAccGender"] = gender;
                }
                else
                {
                    v["faceAccID"] = baseId;
                    v["faceAccGender"] = gender;
                }
            }
            // Eye Accessory
            else if (prefix == 102)
            {
                v["eyeAccID"] = baseId;
                v["eyeAccGender"] = gender;
            }
            // Ear Accessory
            else if (prefix == 103)
            {
                v["earAccID"] = baseId;
                v["earAccGender"] = gender;
            }
            // Coat (short)
            else if (prefix == 104)
            {
                v["coatID"] = baseId;
                v["coatGender"] = gender;
                v["isLongCoat"] = 0;
            }
            // Coat (long)
            else if (prefix == 105)
            {
                v["coatID"] = baseId;
                v["coatGender"] = gender;
                v["isLongCoat"] = 1;
            }
            // Pants
            else if (prefix == 106)
            {
                v["pantsID"] = baseId;
                v["pantsGender"] = gender;
            }
            // Shoes
            else if (prefix == 107)
            {
                v["shoesID"] = baseId;
                v["shoesGender"] = gender;
            }
            // Gloves
            else if (prefix == 108)
            {
                v["glovesID"] = baseId;
                v["glovesGender"] = gender;
            }
            // Cape
            else if (prefix == 110)
            {
                v["capeID"] = baseId;
                v["capeGender"] = gender;
            }
            // Shield / Blade
            else if (prefix == 109 || prefix == 134 || prefix == 135 || prefix == 172)
            {
                v["shieldID"] = baseId;
                v["shieldGender"] = gender;
                if (prefix == 109)
                    v["subWeaponType"] = 1;
                else if (prefix == 134)
                    v["subWeaponType"] = 2;
                else if (prefix == 135)
                    v["subWeaponType"] = 3;
                else if (prefix == 172)
                    v["subWeaponType"] = 4;
            }
            // Cash Weapon
            else if (prefix == 170)
            {
                v["cashWeaponID"] = baseId;
                v["cashWeaponGender"] = gender;
                v["isCashWeapon"] = 1;
            }
            // Regular Weapons
            else if (prefix >= 121 && prefix <= 168)
            {
                int wt = Array.IndexOf(WEAPONS_KMS, prefix);
                if (wt != -1)
                {
                    v["weaponID"] = baseId;
                    v["weaponGender"] = gender;
                    v["weaponType"] = wt;
                }
            }
            // Rings
            else if (prefix == 111 && slot.StartsWith("ring"))
            {
                try
                {
                    int idx = int.Parse(slot[4].ToString());
                    v[$"ringID{idx}"] = baseId;
                    v[$"ringGender{idx}"] = gender;
                }
                catch { }
            }
        }

        private byte[] PackAvatarBytes(Dictionary<string, object> avatar)
        {
            var v = (Dictionary<string, int>)avatar["values"];
            var bytes = new byte[AVATAR_BYTES];
            int offset = 0;

            void Write(int value, int bits)
            {
                for (int i = 0; i < bits; i++)
                {
                    if ((value & (1 << i)) != 0)
                        bytes[offset / 8] |= (byte)(1 << (offset % 8));
                    offset++;
                }
            }

            int Get(string key, int defaultVal = 0)
            {
                return v.ContainsKey(key) ? v[key] : defaultVal;
            }

            // Write all fields exactly as per Python implementation
            Write(Get("gender", 0), 1);
            Write(Get("skinID", DEFAULT_ID), 10);
            Write(Get("face10k", 0), 1);
            Write(Get("faceID", DEFAULT_ID), 10);
            Write(Get("faceGender", 0), 4);
            Write(Get("hair10k", 0), 4);
            Write(Get("hairID", DEFAULT_ID), 10);
            Write(Get("hairGender", 0), 4);
            Write(Get("capID", DEFAULT_ID), 10);
            Write(Get("capGender", 0), 3);
            Write(Get("faceAccID", DEFAULT_ID), 10);
            Write(Get("faceAccGender", 0), 2);
            Write(Get("eyeAccID", DEFAULT_ID), 10);
            Write(Get("eyeAccGender", 0), 2);
            Write(Get("earAccID", DEFAULT_ID), 10);
            Write(Get("earAccGender", 0), 2);
            Write(Get("isLongCoat", 0), 1);
            Write(Get("coatID", DEFAULT_ID), 10);
            Write(Get("coatGender", 0), 4);
            Write(Get("pantsID", DEFAULT_ID), 10);
            Write(Get("pantsGender", 0), 2);
            Write(Get("shoesID", DEFAULT_ID), 10);
            Write(Get("shoesGender", 0), 4);
            Write(Get("glovesID", DEFAULT_ID), 10);
            Write(Get("glovesGender", 0), 2);
            Write(Get("capeID", DEFAULT_ID), 10);
            Write(Get("capeGender", 0), 4);

            Write(Get("subWeaponType", 0), 3);
            if (Get("subWeaponType", 0) != 0)
            {
                Write(Get("shieldID", DEFAULT_ID), 10);
                Write(Get("shieldGender", 0), 4);
            }

            Write(0, 1); // uk2_1
            Write(Get("isCashWeapon", 0), 1);
            if (Get("isCashWeapon", 0) == 1)
            {
                Write(Get("cashWeaponID", DEFAULT_ID), 10);
                Write(Get("cashWeaponGender", 0), 2);
            }

            Write(Get("weaponID", DEFAULT_ID), 10);
            Write(Get("weaponGender", 0), 2);
            Write(Get("weaponType", 0), 8);
            Write(Get("earType", 0), 4);
            Write(Get("mixHairColor", 0), 4);
            Write(Get("mixHairRatio", 0), 8);
            Write(Get("mixFaceInfo", 0), 10);
            Write(Get("unknown1", 0), 4);
            Write(Get("jobWingTailType", 0), 8);
            Write(Get("jobWingTailTypeDetail", 0), 2);
            Write(Get("unknown2", 0), 6);
            Write(Get("eventJob", 0), 3);
            Write(Get("unknown2_2", 0), 21);
            Write(Get("weaponMotionType", 0), 2);
            Write(Get("unknown3", 0), 11);
            Write(Get("showEffectFlags", 0), 4);
            Write(Get("unknown3_2", 0), 3);
            Write(Get("emotionFaceAccID", DEFAULT_ID), 10);
            Write(Get("emotionFaceAccGender", 0), 2);

            // Prisms (all default to 0/not set)
            for (int pIdx = 0; pIdx < 12; pIdx++)
                Write(0, 1); // hasPrism for each type = 0 (no prisms)

            Write(Get("ringID1", DEFAULT_ID), 10);
            Write(Get("ringGender1", 0), 4);
            Write(Get("ringID2", DEFAULT_ID), 10);
            Write(Get("ringGender2", 0), 4);
            Write(Get("ringID3", DEFAULT_ID), 10);
            Write(Get("ringGender3", 0), 4);
            Write(Get("ringID4", DEFAULT_ID), 10);
            Write(Get("ringGender4", 0), 4);

            bytes[AVATAR_BYTES - AVATAR_BYTES / 16 - 1] = AVATAR_VERSION;
            return bytes;
        }

        private byte[] AesEncryptNoPadding(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = AES_KEY;
                aes.IV = AES_IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                using (var encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        private string BytesToAvatarCode(byte[] bytes)
        {
            const string chars = "ABCDEFGHIJKLMNOP";
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                sb.Append(chars[(b >> 4) & 0xF]);
                sb.Append(chars[b & 0xF]);
            }

            return sb.ToString();
        }
    }
}
