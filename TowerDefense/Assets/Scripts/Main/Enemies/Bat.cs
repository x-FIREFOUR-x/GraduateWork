using UnityEngine;


namespace TowerDefense.Main.Enemies
{
    public class Bat : Enemy
    {
        [Header("Bat")]
        [SerializeField]
        private Transform body;
        [SerializeField]
        private Transform leftWing;
        [SerializeField]
        private Transform leftWingOuter;
        [SerializeField]
        private Transform rightWing;
        [SerializeField]
        private Transform rightWingOuter;
        [SerializeField]
        private Transform leftEar;
        [SerializeField]
        private Transform rightEar;
        [SerializeField]
        private Transform tail;

        [Header("Flight")]
        [SerializeField]
        private float flapsPerSecond = 2.6f;
        [SerializeField]
        private float wingAngle = 38f;
        [SerializeField]
        private float wingOuterAngle = 28f;
        [SerializeField]
        private float wingOuterLag = 0.7f;
        [SerializeField]
        private float bobHeight = 0.12f;
        [SerializeField]
        private float bodyPitch = 6f;
        [SerializeField]
        private float earAngle = 8f;
        [SerializeField]
        private float tailAngle = 12f;

        private float phase;
        private Vector3 bodyRest;
        private Quaternion bodyRotation, leftWingRest, leftWingOuterRest, rightWingRest, rightWingOuterRest, leftEarRest, rightEarRest, tailRest;


        protected override void Start()
        {
            base.Start();

            phase = Random.Range(0f, Mathf.PI * 2f);

            bodyRest = body.localPosition;
            bodyRotation = body.localRotation;
            leftWingRest = leftWing.localRotation;
            leftWingOuterRest = leftWingOuter.localRotation;
            rightWingRest = rightWing.localRotation;
            rightWingOuterRest = rightWingOuter.localRotation;
            leftEarRest = leftEar.localRotation;
            rightEarRest = rightEar.localRotation;
            tailRest = tail.localRotation;
        }

        protected override void Animate()
        {
            float pace = Mathf.Lerp(0.55f, 1f, SpeedFactor);
            phase += Time.deltaTime * flapsPerSecond * pace * Mathf.PI * 2f;

            float flap = Mathf.Sin(phase);
            float outerFlap = Mathf.Sin(phase - wingOuterLag);
            float lift = -Mathf.Sin(phase - 0.6f);

            leftWing.localRotation = leftWingRest * Quaternion.Euler(0f, 0f, -wingAngle * flap);
            rightWing.localRotation = rightWingRest * Quaternion.Euler(0f, 0f, wingAngle * flap);
            leftWingOuter.localRotation = leftWingOuterRest * Quaternion.Euler(0f, 0f, -wingOuterAngle * outerFlap);
            rightWingOuter.localRotation = rightWingOuterRest * Quaternion.Euler(0f, 0f, wingOuterAngle * outerFlap);

            body.localPosition = bodyRest + Vector3.up * (bobHeight * lift);
            body.localRotation = bodyRotation * Quaternion.Euler(bodyPitch * Mathf.Sin(phase + 0.9f), 0f, 0f);

            float twitch = Mathf.Sin(phase * 0.5f + 1.3f);
            leftEar.localRotation = leftEarRest * Quaternion.Euler(earAngle * twitch, 0f, earAngle * 0.5f * flap);
            rightEar.localRotation = rightEarRest * Quaternion.Euler(earAngle * twitch, 0f, -earAngle * 0.5f * flap);

            tail.localRotation = tailRest * Quaternion.Euler(tailAngle * Mathf.Sin(phase - 1.4f), tailAngle * 0.6f * Mathf.Sin(phase * 0.5f), 0f);
        }
    }

}
