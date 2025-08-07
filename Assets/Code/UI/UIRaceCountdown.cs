using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class UIRaceCountdown : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        
        public string startMsg;
        public string tickMsgFormat;
        public string completeMsg;

        public float completeMsgDuration;

        private Countdown _countdown;
        
        private int _lastMsgSecond;
        private Awaitable _completeAnim;


        private void OnEnable()
        {
            if (_countdown != null)
                SubEvents(_countdown);
        }

        private void OnDisable()
        {
            if (_countdown != null)
                UnsubEvents(_countdown);
        }


        public UIRaceCountdown Construct(Countdown countdown)
        {
            SubEvents(_countdown = countdown);
            return this;
        }


        private void SubEvents(Countdown countdown)
        {
            countdown.OnStart += OnStart;
            countdown.OnTick += OnTick;
            countdown.OnComplete += OnComplete;
        }
        private void UnsubEvents(Countdown countdown)
        {
            countdown.OnStart -= OnStart;
            countdown.OnTick -= OnTick;
            countdown.OnComplete -= OnComplete;
        }

        
        private void OnStart()
        {
            if (_completeAnim is { IsCompleted: false })
                _completeAnim.Cancel();
            
            _lastMsgSecond = -1;
            
            if (!string.IsNullOrWhiteSpace(startMsg)) 
                SetMassage(startMsg);
        }

        private void OnTick()
        {
            if (CountdownSeconds() > _lastMsgSecond) 
                SetMassage(tickMsgFormat);
        }

        private void OnComplete()
        {
            SetMassage(completeMsg);

            _completeAnim = CompleteAnim();
        }

        private async Awaitable CompleteAnim()
        {
            await Awaitable.WaitForSecondsAsync(completeMsgDuration);
            SetMassage(string.Empty);
        }


        private void SetMassage(string s)
        {
            _lastMsgSecond = CountdownSeconds();
            textMesh.text = string.Format(s, _lastMsgSecond);
        }
        
        
        private int CountdownSeconds() => Mathf.FloorToInt(_countdown.ElapsedTime);
    }
}