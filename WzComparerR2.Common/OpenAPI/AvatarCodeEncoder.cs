using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WzComparerR2.OpenAPI
{
    public class AvatarCodeEncoder
    {
        private const int AVATAR_BYTES = 128;
        private const int AVATAR_VERSION = 40;
        private const int DEFAULT_ID = 1023;

        private static readonly byte[] AES_KEY = [0x10, 0x04, 0x3F, 0x11, 0x17, 0xCD, 0x12, 0x15, 0x5D, 0x8E, 0x7A, 0x19, 0x80, 0x11, 0x4F, 0x14];
        private static readonly byte[] AES_IV = [0x11, 0x17, 0xCD, 0x10, 0x04, 0x3F, 0x8E, 0x7A, 0x12, 0x15, 0x80, 0x11, 0x5D, 0x19, 0x4F, 0x10];

        private readonly int[] WEAPONS_KMS = [-1, 130, 131, 132, 133, 137, 138, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, -1, 134, 152, 153, -1, 136, 121, 122, 123, 124, 156, 157, 126, 158, 127, 128, 159, 129, 121, 1214, 1404, 1215];

        public string ConvertCsvToAvatarCode(string equipIdCsv)
        {
            var avatar = CreateDefaultAvatar();
            ApplyEquipIdCsv(avatar, equipIdCsv);
            var bytes = PackAvatarBytes(avatar);
            var encrypted = AesEncryptNoPadding(bytes);
            return BytesToAvatarCode(encrypted);
        }

        private AvatarData CreateDefaultAvatar()
        {
            var values = new AvatarValues
            {
                Gender = 0,
                SkinID = DEFAULT_ID,
                FaceID = DEFAULT_ID,
                HairID = DEFAULT_ID,
                CapID = DEFAULT_ID,
                FaceAccID = DEFAULT_ID,
                EyeAccID = DEFAULT_ID,
                EarAccID = DEFAULT_ID,
                CoatID = DEFAULT_ID,
                PantsID = DEFAULT_ID,
                ShoesID = DEFAULT_ID,
                GlovesID = DEFAULT_ID,
                CapeID = DEFAULT_ID,
                ShieldID = DEFAULT_ID,
                CashWeaponID = DEFAULT_ID,
                WeaponID = DEFAULT_ID,
                EmotionFaceAccID = DEFAULT_ID,
                RingID1 = DEFAULT_ID,
                RingID2 = DEFAULT_ID,
                RingID3 = DEFAULT_ID,
                RingID4 = DEFAULT_ID,
                // Default flags
                IsNotBlade = 1,
                SubWeaponType = 0,
                IsSubWeapon = 0,
                IsCashWeapon = 0
            };

            return new AvatarData { Version = AVATAR_VERSION, Values = values };
        }

        private void ApplyEquipIdCsv(AvatarData data, string rawCsv)
        {
            var fields = rawCsv.Split(',').Select(f => f.Trim()).ToArray();
            var v = data.Values;

            bool hasCashWeapon = false;
            bool hasRegularWeapon = false;

            // Skin
            if (fields.Length > 1) ProcessItemId(fields[1], v, "skin");

            // Face + Hair
            if (fields.Length > 2) ProcessItemId(fields[2], v, "face");
            if (fields.Length > 3) ProcessItemId(fields[3], v, "hair");

            // Other slots
            if (fields.Length > 4) ProcessItemId(fields[4], v, "cap");
            if (fields.Length > 5) ProcessItemId(fields[5], v, "coat");
            if (fields.Length > 6) ProcessItemId(fields[6], v, "coat");
            if (fields.Length > 7) ProcessItemId(fields[7], v, "pants");
            if (fields.Length > 8) ProcessItemId(fields[8], v, "shoes");
            if (fields.Length > 9) ProcessItemId(fields[9], v, "gloves");
            if (fields.Length > 10) ProcessItemId(fields[10], v, "shield");
            if (fields.Length > 11) ProcessItemId(fields[11], v, "cape");

            // === Weapon / Cash Weapon handling with special rule ===
            if (fields.Length > 12)
            {
                string weaponField = fields[12];

                if (!string.IsNullOrEmpty(weaponField))
                {
                    if (weaponField.StartsWith("170"))
                    {
                        hasCashWeapon = true;
                        ProcessItemId(weaponField, v, "cashWeapon");
                    }
                    else
                    {
                        ProcessItemId(weaponField, v, "weapon");
                        if (IsRegularWeapon(weaponField))
                            hasRegularWeapon = true;
                    }
                }
            }

            // Special Rule: If cash weapon exists but no regular weapon → add 1372243
            if (hasCashWeapon && !hasRegularWeapon)
            {
                ProcessItemId("1372243", v, "weapon");
            }

            // Remaining slots
            if (fields.Length > 13) ProcessItemId(fields[13], v, "earAcc");
            if (fields.Length > 14) ProcessItemId(fields[14], v, "faceAcc");
            if (fields.Length > 15) ProcessItemId(fields[15], v, "eyeAcc");

            for (int i = 1; i <= 4; i++)
            {
                int idx = 24 + i;
                if (fields.Length > idx)
                    ProcessItemId(fields[idx], v, $"ring{i}");
            }
        }

        private bool IsRegularWeapon(string idStr)
        {
            if (!int.TryParse(idStr, out int id)) return false;
            return id >= 1210000 && id <= 1689999;
        }

        private void ProcessItemId(string raw, AvatarValues v, string slot)
        {
            if (string.IsNullOrEmpty(raw)) return;

            // Handle formula like "55074+5*50"
            string idPart = raw.Split('+')[0].Trim();
            if (!int.TryParse(idPart, out int id)) return;

            int prefix = id / 10000;
            int gender = (id % 10000) / 1000;
            int baseId = id % 1000;

            switch (prefix)
            {
                case 0:
                case 1:
                    v.SkinID = baseId; break;
                case 2:
                case 5:
                case 8:
                    v.FaceID = baseId; v.FaceGender = gender; v.Face10k = prefix == 5 ? 1 : 0; break;
                case 3:
                case 4:
                case 6:
                case 7:
                    v.HairID = baseId; v.HairGender = gender; v.Hair10k = prefix; break;
                case 100:
                    v.CapID = baseId; v.CapGender = gender; break;
                case 101:
                    if (slot == "emotionFaceAcc") { v.EmotionFaceAccID = baseId; v.EmotionFaceAccGender = gender; }
                    else { v.FaceAccID = baseId; v.FaceAccGender = gender; }
                    break;
                case 102: v.EyeAccID = baseId; v.EyeAccGender = gender; break;
                case 103: v.EarAccID = baseId; v.EarAccGender = gender; break;
                case 104: v.CoatID = baseId; v.CoatGender = gender; v.IsLongCoat = 0; break;
                case 105: v.CoatID = baseId; v.CoatGender = gender; v.IsLongCoat = 1; break;
                case 106: v.PantsID = baseId; v.PantsGender = gender; break;
                case 107: v.ShoesID = baseId; v.ShoesGender = gender; break;
                case 108: v.GlovesID = baseId; v.GlovesGender = gender; break;
                case 110: v.CapeID = baseId; v.CapeGender = gender; break;
                case 109: v.ShieldID = baseId; v.ShieldGender = gender; v.SubWeaponType = 1; v.IsNotBlade = 1; v.IsSubWeapon = 0; break;
                case 134: v.ShieldID = baseId; v.ShieldGender = gender; v.SubWeaponType = 2; v.IsNotBlade = 0; v.IsSubWeapon = 0; break;
                case 135: v.ShieldID = baseId; v.ShieldGender = gender; v.SubWeaponType = 3; v.IsNotBlade = 1; v.IsSubWeapon = 1; break;
                case 172: v.ShieldID = baseId; v.ShieldGender = gender; v.SubWeaponType = 4; v.IsNotBlade = 1; v.IsSubWeapon = 0; break;
                case 170: v.CashWeaponID = baseId; v.CashWeaponGender = gender; v.IsCashWeapon = 1; break;
            }

            if (prefix >= 121 && prefix <= 168)
            {
                int weaponType = Array.IndexOf(WEAPONS_KMS, prefix);
                if (weaponType != -1)
                {
                    v.WeaponID = baseId;
                    v.WeaponGender = gender;
                    v.WeaponType = weaponType;
                }
            }

            if (prefix == 111 && slot.StartsWith("ring"))
            {
                int ringIdx = int.Parse(slot.Substring(4));
                switch (ringIdx)
                {
                    case 1: v.RingID1 = baseId; v.RingGender1 = gender; break;
                    case 2: v.RingID2 = baseId; v.RingGender2 = gender; break;
                    case 3: v.RingID3 = baseId; v.RingGender3 = gender; break;
                    case 4: v.RingID4 = baseId; v.RingGender4 = gender; break;
                }
            }
        }

        private byte[] PackAvatarBytes(AvatarData data)
        {
            var bytes = new byte[AVATAR_BYTES];
            var v = data.Values;
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

            Write(v.Gender, 1);
            Write(v.SkinID, 10);
            Write(v.Face10k, 1);
            Write(v.FaceID, 10);
            Write(v.FaceGender, 4);
            Write(v.Hair10k, 4);
            Write(v.HairID, 10);
            Write(v.HairGender, 4);
            Write(v.CapID, 10);
            Write(v.CapGender, 3);
            Write(v.FaceAccID, 10);
            Write(v.FaceAccGender, 2);
            Write(v.EyeAccID, 10);
            Write(v.EyeAccGender, 2);
            Write(v.EarAccID, 10);
            Write(v.EarAccGender, 2);
            Write(v.IsLongCoat, 1);
            Write(v.CoatID, 10);
            Write(v.CoatGender, 4);
            Write(v.PantsID, 10);
            Write(v.PantsGender, 2);
            Write(v.ShoesID, 10);
            Write(v.ShoesGender, 4);
            Write(v.GlovesID, 10);
            Write(v.GlovesGender, 2);
            Write(v.CapeID, 10);
            Write(v.CapeGender, 4);

            Write(v.SubWeaponType, 3);
            if (v.SubWeaponType != 0)
            {
                Write(v.ShieldID, 10);
                Write(v.ShieldGender, 4);
            }

            Write(v.Uk2_1, 1);
            Write(v.IsCashWeapon, 1);
            if (v.IsCashWeapon == 1)
            {
                Write(v.CashWeaponID, 10);
                Write(v.CashWeaponGender, 2);
            }

            Write(v.WeaponID, 10);
            Write(v.WeaponGender, 2);
            Write(v.WeaponType, 8);
            Write(v.EarType, 4);
            Write(v.MixHairColor, 4);
            Write(v.MixHairRatio, 8);
            Write(v.MixFaceInfo, 10);
            Write(v.Unknown1, 4);
            Write(v.JobWingTailType, 8);
            Write(v.JobWingTailTypeDetail, 2);
            Write(v.Unknown2, 6);
            Write(v.EventJob, 3);
            Write(v.Unknown2_2, 21);
            Write(v.WeaponMotionType, 2);
            Write(v.Unknown3, 11);
            Write(v.ShowEffectFlags, 4);
            Write(v.Unknown3_2, 3);
            Write(v.EmotionFaceAccID, 10);
            Write(v.EmotionFaceAccGender, 2);

            // Rings
            Write(v.RingID1, 10); Write(v.RingGender1, 4);
            Write(v.RingID2, 10); Write(v.RingGender2, 4);
            Write(v.RingID3, 10); Write(v.RingGender3, 4);
            Write(v.RingID4, 10); Write(v.RingGender4, 4);

            // Version byte
            bytes[AVATAR_BYTES - AVATAR_BYTES / 16 - 1] = AVATAR_VERSION;

            return bytes;
        }

        private byte[] AesEncryptNoPadding(byte[] plainBytes)
        {
            using var aes = Aes.Create();
            aes.Key = AES_KEY;
            aes.IV = AES_IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private string BytesToAvatarCode(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append((char)('A' + (b >> 4)));
                sb.Append((char)('A' + (b & 0x0F)));
            }
            return sb.ToString();
        }

        // Data classes
        private class AvatarData
        {
            public int Version { get; set; }
            public AvatarValues Values { get; set; } = new();
        }

        private class AvatarValues
        {
            public int Gender, SkinID, Face10k, FaceID, FaceGender;
            public int Hair10k, HairID, HairGender;
            public int CapID, CapGender;
            public int FaceAccID, FaceAccGender;
            public int EyeAccID, EyeAccGender;
            public int EarAccID, EarAccGender;
            public int IsLongCoat, CoatID, CoatGender;
            public int PantsID, PantsGender;
            public int ShoesID, ShoesGender;
            public int GlovesID, GlovesGender;
            public int CapeID, CapeGender;
            public int SubWeaponType, ShieldID, ShieldGender;
            public int IsNotBlade, IsSubWeapon;
            public int IsCashWeapon, CashWeaponID, CashWeaponGender;
            public int WeaponID, WeaponGender, WeaponType;
            public int EarType, MixHairColor, MixHairRatio, MixFaceInfo;
            public int Uk2_1, Unknown1, JobWingTailType, JobWingTailTypeDetail;
            public int Unknown2, EventJob, Unknown2_2, WeaponMotionType;
            public int Unknown3, ShowEffectFlags, Unknown3_2;
            public int EmotionFaceAccID, EmotionFaceAccGender;
            public int RingID1, RingGender1, RingID2, RingGender2;
            public int RingID3, RingGender3, RingID4, RingGender4;
        }
    }
}
