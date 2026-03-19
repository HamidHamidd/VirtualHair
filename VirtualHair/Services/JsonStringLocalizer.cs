using System.Collections.Concurrent;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace VirtualHair.Services
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _translations = new();
        private readonly string _resourcesPath;

        public JsonStringLocalizer(string resourcesPath)
        {
            _resourcesPath = resourcesPath;
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            var filePath = Path.Combine(_resourcesPath, "translations.json");
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

            if (data == null) return;

            foreach (var lang in data)
            {
                var langDict = new ConcurrentDictionary<string, string>(lang.Value);
                _translations[lang.Key] = langDict;
            }
        }

        public LocalizedString this[string name]
        {
            get
            {
                var culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                var value = GetValue(name, culture);
                return new LocalizedString(name, value ?? name, value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = this[name];
                var value = string.Format(format.Value ?? name, arguments);
                return new LocalizedString(name, value, format.ResourceNotFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
             var culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
             if (_translations.TryGetValue(culture, out var dict))
             {
                 return dict.Select(kvp => new LocalizedString(kvp.Key, kvp.Value, false));
             }
             return Enumerable.Empty<LocalizedString>();
        }

        private string GetValue(string key, string culture)
        {
            if (_translations.TryGetValue(culture, out var dict))
            {
                if (dict.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            return null;
        }
    }

    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly string _resourcesPath;

        public JsonStringLocalizerFactory(IWebHostEnvironment env)
        {
            _resourcesPath = Path.Combine(env.ContentRootPath, "Resources");
        }

        public IStringLocalizer Create(Type resourceSource) => new JsonStringLocalizer(_resourcesPath);
        public IStringLocalizer Create(string baseName, string location) => new JsonStringLocalizer(_resourcesPath);
    }
}
