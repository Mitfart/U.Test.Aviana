using UnityEngine;
using UnityEngine.Splines;

namespace Ghost
{
    [RequireComponent(typeof(SplineContainer))]
    public class Ghost : MonoBehaviour
    {
        public SplineContainer splineContainer;
        public SplineAnimate body;
        public GameObject car;

        

        private void Awake()
        {
            splineContainer.Spline.Clear();
            splineContainer.Spline.Add(transform.position);
        }

        private void OnEnable() => body.Play();
        private void OnDisable() => body.Pause();

        public void SetPath(Spline spline)
        {
            splineContainer.Spline.Clear();
            splineContainer.Spline.Copy(spline);
            
            body.Restart(autoplay: true);
        }
    }
}