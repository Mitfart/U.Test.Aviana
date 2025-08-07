using UnityEngine;

namespace Ashsvp
{
    public class WheelSkid : MonoBehaviour
    {
        private const float MaxSkidIntensity = 1.0f;

        [HideInInspector] public Skidmarks skidmarks;

        public TireSmoke smoke;

        //[HideInInspector]
        public float radius, skidTotal;

        [HideInInspector] public Vector3 skidPoint, normal;

        private float lastFixedUpdateTime;
        private int lastSkid = -1;
        private AudioSource skidSound;


        private void Start()
        {
            smoke.transform.localPosition = Vector3.up * radius;
            skidSound = GetComponent<AudioSource>();
            skidSound.mute = true;

            lastFixedUpdateTime = Time.time;
        }

        protected void FixedUpdate()
        {
            lastFixedUpdateTime = Time.time;
            SkidLogic();
        }

        public void SkidLogic()
        {
            var intensity = Mathf.Clamp01(skidTotal / MaxSkidIntensity);


            if (skidTotal > 0)
            {
                lastSkid = skidmarks.AddSkidMark(skidPoint, normal, intensity, lastSkid);
                if (smoke && intensity > 0.4f)
                {
                    smoke.playSmoke();
                    skidSound.mute = false;
                }
                else if (smoke)
                {
                    smoke.stopSmoke();
                    skidSound.mute = true;
                }

                skidSound.volume = intensity / 3;
            }
            else
            {
                skidSound.mute = true;
                lastSkid = -1;
                if (smoke) smoke.stopSmoke();
            }
        }
    }
}