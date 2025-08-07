using System;
using Infrastructure.Services.Tick;
using Infrastructure.Services.Time;
using UnityEngine;

namespace Utils
{
    public class Countdown : ITickable
    {
        public event Action OnStart;
        public event Action OnTick;
        public event Action OnComplete;
        
        private readonly float _duration;
        private readonly ITimeService _time;

        private float _startTime;
        private float _lastTickTime;
        
        public bool Clocking { get; private set; }
        public float ElapsedTime => _lastTickTime - _startTime;
        public float ElapsedTimeClamped => Mathf.Clamp(ElapsedTime, 0f, _duration);


        public Countdown(float duration, ITimeService time)
        {
            _duration = duration;
            _time = time;
            Clocking = false;
        }

        public void Start()
        {
            _startTime = _time.Current;
            _lastTickTime = _startTime;
            
            Clocking = true;
            
            OnStart?.Invoke();
        }

        public void Tick()
        {
            if (!Clocking) 
                return;
            
            _lastTickTime = _time.Current;
            OnTick?.Invoke();
            
            if (ElapsedTime >= _duration)
            {
                Clocking = false;
                OnComplete?.Invoke();
            }
        }
    }
}