using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace i18n
{
    public class LocalizationManager : ScriptableObject
    {
        public delegate void LanguageChange(string value);

        public LanguageChange OnLocaleUpdate;

        private Dictionary<string, string> _translations = new();

        private static LocalizationManager _instance;

        public static LocalizationManager Instance
        {
            get {
                if (_instance == null) {
                    _instance = GetOrCreateManager();
                }

                return _instance;
            }
        }

        private static LocalizationManager GetOrCreateManager()
        {
            if (_instance != null) {
                return _instance;
            }

            var manager = CreateInstance<LocalizationManager>();
            manager.name = "LocalizationManager";

            return manager;
        }

        public void SetLocale(string locale)
        {
            LoadLocaleTranslations(locale);
            PlayerPrefs.SetString("locale", locale);
            OnLocaleUpdate.Invoke(locale);
        }

        private void Awake()
        {
            LoadLocaleTranslations(PlayerPrefs.GetString("locale", "en")); // TODO replace "en"
        }

        public static string __(string translationKey, object[] arguments = null)
        {
            var text = Instance._translations.ContainsKey(translationKey) ? Instance._translations[translationKey] : translationKey;

            return arguments == null ? text : string.Format(text, arguments);
        }

        private void LoadLocaleTranslations(string locale)
        {
            Debug.Log($"[LocalizationManager] Loading locale {locale}");
            _translations = new Dictionary<string, string>();

            var test = Resources.LoadAll<TextAsset>($"i18n/{locale}");
            foreach (var textAsset in test) {
                LoadStrings(JsonConvert.DeserializeObject<Dictionary<string, object>>(textAsset.text), textAsset.name == locale ? null : new List<string> { textAsset.name });
            }

            Debug.Log($"[LocalizationManager] {locale}: Loaded {_translations.Count} strings");
        }

        private void LoadStrings(Dictionary<string, object> temp, IReadOnlyCollection<string> dots = null)
        {
            foreach (var (key, value) in temp) {
                var keyDots = dots != null ? new List<string>(dots) : new List<string>();

                if (value is string or JValue) {
                    keyDots.Add(key);
                    _translations.Add(string.Join(".", keyDots), value.ToString());
                }
                else if (value is JArray array) {
                    keyDots.Add(key);
                    for (var index = 0; index < array.Count; index++) {
                        var tempDots = new List<string>(keyDots) { index.ToString() };
                        _translations.Add(string.Join(".", tempDots), array[index].ToString());
                    }
                }
                else if (value is JObject subValues) {
                    var objectValues = new Dictionary<string, object>();
                    foreach (var o in subValues) {
                        objectValues[o.Key] = o.Value;
                    }

                    keyDots.Add(key);
                    LoadStrings(objectValues, keyDots);
                }
                else {
                    throw new Exception($"Unsupported type in i18n json: {value.GetType()}");
                }
            }
        }
    }
}