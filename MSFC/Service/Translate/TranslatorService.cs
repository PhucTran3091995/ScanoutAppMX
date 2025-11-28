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

            // 1) Dịch theo đoạn con
            var translated = Translate(english);

            // 2) Tìm key dài nhất xuất hiện trong chuỗi
            var match = _dict
                .Where(kv => ContainsIgnoreCase(english, kv.Key))
                .OrderByDescending(kv => kv.Key.Length)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(match.Key))
            {
                // ❗ Không khớp dict → fallback: trả về chuỗi gốc, không dịch
                return new TranslationResult(english, null, null);
            }

            _guides.TryGetValue(match.Key, out var guide);
            return new TranslationResult(translated, guide?.Explain_Es, match.Key);
        }


        private static bool ContainsIgnoreCase(string src, string val) => src?.IndexOf(val, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
