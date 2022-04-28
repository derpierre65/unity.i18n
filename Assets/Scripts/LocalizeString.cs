using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace i18n
{
    [Serializable]
    public class Variable
    {
        public UnityEngine.Object myObject;
        public string fieldName;
    }

    public class LocalizeString : MonoBehaviour
    {
        [SerializeField] public string translationKey;
        [SerializeField] public UnityEvent<string> onUpdateString;

        [SerializeField] private List<Variable> variables = new();

        private delegate void StringUpdate(string value);

        private StringUpdate _onStringUpdate;

        private void OnEnable() => RegisterHandler();
        private void OnDisable() => ClearHandler();

        private void RefreshString()
        {
            var arguments = new List<object>();
            foreach (var variable in variables) {
                var fieldValue = variable.myObject.GetType().GetField(variable.fieldName);
                arguments.Add(fieldValue != null ? fieldValue.GetValue(variable.myObject) : "");
            }

            _onStringUpdate(LocalizationManager.__(translationKey, arguments.ToArray()));
        }

        private void RegisterHandler()
        {
            var wasEmpty = _onStringUpdate == null;

            LocalizationManager.Instance.OnLocaleUpdate += OnLocaleUpdate;
            _onStringUpdate += UpdateString;

            if (wasEmpty) {
                RefreshString();
            }
        }

        private void ClearHandler()
        {
            LocalizationManager.Instance.OnLocaleUpdate -= OnLocaleUpdate;
            _onStringUpdate -= UpdateString;
        }

        private void UpdateString(string value)
        {
            onUpdateString.Invoke(value);
        }

        private void OnLocaleUpdate(string locale)
        {
            RefreshString();
        }
    }
}