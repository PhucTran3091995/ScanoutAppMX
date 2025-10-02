using FlaUI.Core.AutomationElements;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFC.Service.Translate
{
    public record TranslationResult(string Translated, string ExplainEs, string MatchedKey);

    public interface ITranslatorService
    {
        string Translate(string english);                      // chỉ dịch
        TranslationResult TranslateAndGuide(string english);   // dịch + hướng dẫn
    }
    public class TranslatorService : ITranslatorService
    {
        private readonly Dictionary<string, string> _dict;
        private readonly Dictionary<string, TranslationGuide> _guides;
        public TranslatorService(IOptions<TranslationConfig> cfg)
        {
            _dict = cfg.Value.Translations ?? new();
            _guides = cfg.Value.Guides ?? new();
        }


        public string Translate(string english)
        {
            if (string.IsNullOrWhiteSpace(english)) return english;

            string result = english;

            foreach (var kv in _dict)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;

                // Thay từng đoạn con nếu xuất hiện trong chuỗi
                result = result.Replace(kv.Key, kv.Value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }
        public TranslationResult TranslateAndGuide(string english)
        {
            if (string.IsNullOrWhiteSpace(english))
                return new TranslationResult(english, null, null);

            // 1) dịch theo đoạn con (đã có)
            var translated = Translate(english);

            // 2) tìm key dài nhất xuất hiện trong chuỗi
            var match = _dict.Where(kv => ContainsIgnoreCase(english, kv.Key))
                             .OrderByDescending(kv => kv.Key.Length)
                             .FirstOrDefault();

            if (string.IsNullOrEmpty(match.Key))
                return new TranslationResult(translated, null, null); // <-- không guide

            _guides.TryGetValue(match.Key, out var guide);
            return new TranslationResult(translated, guide?.Explain_Es, match.Key);
        }

        private static bool ContainsIgnoreCase(string src, string val) => src?.IndexOf(val, StringComparison.OrdinalIgnoreCase) >= 0;

        private static string ReplaceIgnoreCase(string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue)) return source;
            int idx = 0;
            var sb = new StringBuilder();
            while (idx < source.Length)
            {
                int found = source.IndexOf(oldValue, idx, StringComparison.OrdinalIgnoreCase);
                if (found < 0)
                {
                    sb.Append(source, idx, source.Length - idx);
                    break;
                }
                sb.Append(source, idx, found - idx);
                sb.Append(newValue);
                idx = found + oldValue.Length;
            }
            return sb.ToString();
        }
    }
}
