using System;
using TMPro;
using UnityEngine;

namespace Utils
{
    public class TypeWriter : MonoBehaviour
    {
        public event Action OnStartTyping;
        public event Action OnTypeChar;
        public event Action OnEndTyping;

        [SerializeField] private TMP_Text textMesh;
        [SerializeField] private float typeSpeed;
        public bool typeOnEnable;

        private bool _initialized;
        private float _typeDelay;

        private Awaitable _typeTask;


        private void OnEnable()
        {
            Init();
            
            if (typeOnEnable) StartTyping();
        }

        private void OnDisable()
        {
            CancelTyping();
        }

        private void Init()
        {
            if (_initialized)
                return;
            
            _typeDelay = 1f / typeSpeed;
            _initialized = true;
        }

        
        public void StartTyping()
        {
            CancelTyping();

            _typeTask = TypeTextTask();
        }

        private void CancelTyping()
        {
            _typeTask?.Cancel();
            _typeTask = null;
        }

        public void SetText(string text) => textMesh.text = text;

        public void SetTypeSpeed(float value) => typeSpeed = value;

        
        private async Awaitable TypeTextTask()
        {
            OnStartTyping?.Invoke();
            var info = textMesh.textInfo;
            
            textMesh.maxVisibleCharacters = -1; // Start from empty text

            while (textMesh.maxVisibleCharacters <= info.characterCount)
            {
                textMesh.maxVisibleCharacters++;
                OnTypeChar?.Invoke();
                await Awaitable.WaitForSecondsAsync(_typeDelay);
            }

            OnEndTyping?.Invoke();
        }
    }
}