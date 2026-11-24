using System;
using System.Collections.Generic;
using System.Text;

namespace WzComparerR2.Common
{
    /// <summary>
    /// Two-set (dubeolsik) hangul keyboard layouts.
    /// </summary>
    public enum HangulKeyboardLayout
    {
        /// <summary>KS X 5002, the South Korean standard layout.</summary>
        Dubeolsik,

        /// <summary>KPS 9256 (국규 9256), the North Korean standard layout.</summary>
        Kps9256,
    }

    /// <summary>
    /// Precomposed hangul syllable (U+AC00..U+D7A3) to two-set keyboard letters.
    /// Shifted jamo are mapped to uppercase letters, e.g. ㅃ = Q.
    /// Compound medials/finals expand to their component keystrokes, e.g. ㅘ = "hk"/"hj", ㄺ = "fr"/"rs".
    /// Both layouts use the same jamo inventory and typing rules; only the key positions differ.
    /// </summary>
    public static class HangulConverter
    {
        private const int SyllableBase = 0xAC00;
        private const int SyllableCount = 11172;
        private const int InitialCount = 19;
        private const int MedialCount = 21;
        private const int FinalCount = 28;

        // KS X 5002 (남한 표준)
        // ㄱ ㄲ ㄴ ㄷ ㄸ ㄹ ㅁ ㅂ ㅃ ㅅ ㅆ ㅇ ㅈ ㅉ ㅊ ㅋ ㅌ ㅍ ㅎ
        private static readonly string[] KsX5002InitialKeys =
        {
            "r", "R", "s", "e", "E", "f", "a", "q", "Q", "t",
            "T", "d", "w", "W", "c", "z", "x", "v", "g",
        };

        // ㅏ ㅐ ㅑ ㅒ ㅓ ㅔ ㅕ ㅖ ㅗ ㅘ ㅙ ㅚ ㅛ ㅜ ㅝ ㅞ ㅟ ㅠ ㅡ ㅢ ㅣ
        private static readonly string[] KsX5002MedialKeys =
        {
            "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho",
            "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l",
        };

        // (none) ㄱ ㄲ ㄳ ㄴ ㄵ ㄶ ㄷ ㄹ ㄺ ㄻ ㄼ ㄽ ㄾ ㄿ ㅀ ㅁ ㅂ ㅄ ㅅ ㅆ ㅇ ㅈ ㅊ ㅋ ㅌ ㅍ ㅎ
        private static readonly string[] KsX5002FinalKeys =
        {
            "", "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa",
            "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T",
            "d", "w", "c", "z", "x", "v", "g",
        };

        // KPS 9256 (북한 국규 9256) — same jamo order as above.
        // Note: ㅊ and ㅍ are swapped relative to KS X 5002 (here V = ㅊ, C = ㅍ).
        // Q ㅂ/ㅃ  W ㅁ  E ㄷ/ㄸ  R ㄹ  T ㅎ  Y ㅕ  U ㅜ  I ㅓ  O ㅐ/ㅒ  P ㅔ/ㅖ
        // A ㅈ/ㅉ  S ㄱ/ㄲ  D ㅇ  F ㄴ  G ㅅ/ㅆ  H ㅗ  J ㅏ  K ㅣ  L ㅡ
        // Z ㅋ  X ㅌ  C ㅍ  V ㅊ  B ㅠ  N ㅛ  M ㅑ
        // ㄱ ㄲ ㄴ ㄷ ㄸ ㄹ ㅁ ㅂ ㅃ ㅅ ㅆ ㅇ ㅈ ㅉ ㅊ ㅋ ㅌ ㅍ ㅎ
        private static readonly string[] Kps9256InitialKeys =
        {
            "s", "S", "f", "e", "E", "r", "w", "q", "Q", "g",
            "G", "d", "a", "A", "v", "z", "x", "c", "t",
        };

        // ㅏ ㅐ ㅑ ㅒ ㅓ ㅔ ㅕ ㅖ ㅗ ㅘ ㅙ ㅚ ㅛ ㅜ ㅝ ㅞ ㅟ ㅠ ㅡ ㅢ ㅣ
        private static readonly string[] Kps9256MedialKeys =
        {
            "j", "o", "m", "O", "i", "p", "y", "P", "h", "hj", "ho",
            "hk", "n", "u", "ui", "up", "uk", "b", "l", "lk", "k",
        };

        // (none) ㄱ ㄲ ㄳ ㄴ ㄵ ㄶ ㄷ ㄹ ㄺ ㄻ ㄼ ㄽ ㄾ ㄿ ㅀ ㅁ ㅂ ㅄ ㅅ ㅆ ㅇ ㅈ ㅊ ㅋ ㅌ ㅍ ㅎ
        private static readonly string[] Kps9256FinalKeys =
        {
            "", "s", "S", "sg", "f", "fa", "ft", "e", "r", "rs", "rw",
            "rq", "rg", "rx", "rc", "rt", "w", "q", "qg", "g", "G",
            "d", "a", "v", "z", "x", "c", "t",
        };

        /// <summary>
        /// KS X 5002 table. Key: Unicode code point of a precomposed hangul syllable.
        /// Value: keystrokes that type that syllable.
        /// Note: C# has no const Dictionary, so these are immutable-by-convention
        /// static readonly tables built once from the jamo key tables above.
        /// </summary>
        public static readonly Dictionary<int, string> DubeolsikKeys =
            CreateKeys(KsX5002InitialKeys, KsX5002MedialKeys, KsX5002FinalKeys);

        /// <summary>
        /// KPS 9256 table (국규 9256), same shape as <see cref="DubeolsikKeys"/>.
        /// </summary>
        public static readonly Dictionary<int, string> Kps9256Keys =
            CreateKeys(Kps9256InitialKeys, Kps9256MedialKeys, Kps9256FinalKeys);

        /// <summary>
        /// Gets the precomputed syllable table for a layout.
        /// </summary>
        public static Dictionary<int, string> GetKeys(HangulKeyboardLayout layout)
        {
            switch (layout)
            {
                case HangulKeyboardLayout.Kps9256:
                    return Kps9256Keys;
                default:
                    return DubeolsikKeys;
            }
        }

        /// <summary>
        /// Gets the keystrokes for a precomposed hangul syllable.
        /// </summary>
        public static bool TryGetKeystrokes(HangulKeyboardLayout layout, char hangul, out string keys)
        {
            return GetKeys(layout).TryGetValue(hangul, out keys);
        }

        /// <summary>
        /// Gets the KS X 5002 dubeolsik keystrokes for a precomposed hangul syllable.
        /// </summary>
        public static bool TryGetDubeolsik(char hangul, out string keys)
        {
            return DubeolsikKeys.TryGetValue(hangul, out keys);
        }

        /// <summary>
        /// Replaces every precomposed hangul syllable with its keystrokes in the given layout.
        /// Other characters (latin, digits, punctuation, jamo) pass through unchanged,
        /// so the result can be used to normalize both search text and query:
        /// ToDubeolsik("메이플스토리") == "apdlvmftmxhfl",
        /// ToKeystrokes("메이플스토리", Kps9256) == "wpdkclrglxhrk".
        /// </summary>
        public static string ToKeystrokes(string text, HangulKeyboardLayout layout)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Search normalization calls this for every candidate string, so skip the
            // allocation entirely when there is no hangul syllable to convert.
            int firstSyllable = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= SyllableBase && text[i] < SyllableBase + SyllableCount)
                {
                    firstSyllable = i;
                    break;
                }
            }
            if (firstSyllable < 0)
                return text;

            Dictionary<int, string> map = GetKeys(layout);
            var sb = new StringBuilder(text.Length * 2);
            sb.Append(text, 0, firstSyllable);
            for (int i = firstSyllable; i < text.Length; i++)
            {
                string keys;
                if (map.TryGetValue(text[i], out keys))
                    sb.Append(keys);
                else
                    sb.Append(text[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts hangul syllables to KS X 5002 dubeolsik keystrokes.
        /// </summary>
        public static string ToDubeolsik(string text)
        {
            return ToKeystrokes(text, HangulKeyboardLayout.Dubeolsik);
        }

        /// <summary>
        /// Converts hangul syllables to KPS 9256 keystrokes.
        /// </summary>
        public static string ToKps9256(string text)
        {
            return ToKeystrokes(text, HangulKeyboardLayout.Kps9256);
        }

        /// <summary>
        /// Returns true if <paramref name="text"/> contains <paramref name="query"/> after both are
        /// normalized to keystrokes of the given layout, so a query typed on a Korean keyboard
        /// layout ("apdlvmftmxhfl") matches hangul text ("메이플스토리"). Letters are compared
        /// case-insensitively by default, which also folds shifted jamo (ㅂ/ㅃ, ㄱ/ㄲ, ...).
        /// </summary>
        public static bool ContainsKeystrokes(string text, string query, HangulKeyboardLayout layout,
            bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(query))
                return true;
            if (string.IsNullOrEmpty(text))
                return false;

            string comparisonText = ToKeystrokes(text, layout);
            string comparisonQuery = ToKeystrokes(query, layout);
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return comparisonText.IndexOf(comparisonQuery, comparison) >= 0;
        }

        /// <summary>
        /// Returns true if <paramref name="text"/> contains <paramref name="query"/> when both are
        /// normalized to KS X 5002 dubeolsik keystrokes.
        /// </summary>
        public static bool ContainsDubeolsik(string text, string query, bool ignoreCase = true)
        {
            return ContainsKeystrokes(text, query, HangulKeyboardLayout.Dubeolsik, ignoreCase);
        }

        /// <summary>
        /// Returns true if <paramref name="text"/> contains <paramref name="query"/> when both are
        /// normalized to KPS 9256 keystrokes.
        /// </summary>
        public static bool ContainsKps9256(string text, string query, bool ignoreCase = true)
        {
            return ContainsKeystrokes(text, query, HangulKeyboardLayout.Kps9256, ignoreCase);
        }

        private static Dictionary<int, string> CreateKeys(string[] initialKeys, string[] medialKeys, string[] finalKeys)
        {
            var map = new Dictionary<int, string>(SyllableCount);
            int index = 0;

            for (int initial = 0; initial < InitialCount; initial++)
            {
                for (int medial = 0; medial < MedialCount; medial++)
                {
                    for (int final = 0; final < FinalCount; final++)
                    {
                        // Unicode syllable = base + (initial * 21 + medial) * 28 + final
                        map.Add(SyllableBase + index, initialKeys[initial] + medialKeys[medial] + finalKeys[final]);
                        index++;
                    }
                }
            }

            return map;
        }
    }
}
