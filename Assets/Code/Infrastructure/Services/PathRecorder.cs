using Infrastructure.Services.Time;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Infrastructure.Services.Path
{
    public class PathRecorder : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;

        public float recordFrequency;

        public Spline PrevRecordedSpline { get; private set; }
        public bool HasRecord { get; private set; }

        private ITimeService _time;
        private float _lastRecordTime;


        private void Awake()
        {
            splineContainer.Spline.Clear();
            splineContainer.Spline.Closed = false;

            PrevRecordedSpline = new Spline();
        }


        public PathRecorder Construct(ITimeService time)
        {
            _time = time;
            return this;
        }


        public void Record(Transform obj)
        {
            if (_time.Current - _lastRecordTime < recordFrequency)
                return;

            splineContainer.Spline.Add(new BezierKnot(
                obj.position,
                float3.zero,
                float3.zero,
                obj.rotation
            ));

            _lastRecordTime = _time.Current;
        }

        public void SaveRecord()
        {
            PrevRecordedSpline.Clear();
            PrevRecordedSpline.Copy(splineContainer.Spline);
            
            splineContainer.Spline.Clear();
            HasRecord = true;
        }
    }
}