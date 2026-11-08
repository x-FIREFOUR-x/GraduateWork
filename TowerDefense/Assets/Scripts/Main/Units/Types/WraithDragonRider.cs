using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class WraithDragonRider : Unit
    {
        [Header("Dragon")]
        [SerializeField]
        private Transform body;
        [SerializeField]
        private Transform neck;
        [SerializeField]
        private Transform head;
        [SerializeField]
        private Transform jaw;
        [SerializeField]
        private Transform leftWing;
        [SerializeField]
        private Transform leftWingOuter;
        [SerializeField]
        private Transform rightWing;
        [SerializeField]
        private Transform rightWingOuter;
        [SerializeField]
        private Transform leftLeg;
        [SerializeField]
        private Transform rightLeg;
        [SerializeField]
        private Transform tail;
        [SerializeField]
        private Transform tailTip;

        [Header("Rider")]
        [SerializeField]
        private Transform rider;
        [SerializeField]
        private Transform riderCape;
        [SerializeField]
        private Transform riderCapeMiddle;
        [SerializeField]
        private Transform riderCapeLower;
        [SerializeField]
        private Transform riderStand;
        [SerializeField]
        private Wraith wraithPrefab;

        [Header("Flight")]
        [SerializeField]
        private float flapsPerSecond = 1.1f;
        [SerializeField]
        private float wingAngle = 34f;
        [SerializeField]
        private float wingOuterAngle = 24f;
        [SerializeField]
        private float wingOuterLag = 0.8f;
        [SerializeField]
        private float bobHeight = 0.12f;
        [SerializeField]
        private float bodyPitch = 5f;
        [SerializeField]
        private float neckWave = 7f;
        [SerializeField]
        private float tailWave = 9f;
        [SerializeField]
        private float legSway = 6f;
        [SerializeField]
        private float capeFlutter = 12f;
        [SerializeField]
        private float capeWaves = 2.4f;

        [Header("Fall")]
        [SerializeField]
        private float corpseThickness = 0.25f;

        private float phase;
        private Vector3 bodyRest;
        private Quaternion bodyRotation, neckRest, headRest, jawRest, leftWingRest, leftWingOuterRest, rightWingRest, rightWingOuterRest;
        private Quaternion leftLegRest, rightLegRest, tailRest, tailTipRest, riderCapeRest, riderCapeMiddleRest, riderCapeLowerRest;


        protected override Transform Torso { get { return body; } }

        protected override void Start()
        {
            base.Start();

            phase = Random.Range(0f, Mathf.PI * 2f);

            bodyRest = body.localPosition;
            bodyRotation = body.localRotation;
            neckRest = neck.localRotation;
            headRest = head.localRotation;
            jawRest = jaw.localRotation;
            leftWingRest = leftWing.localRotation;
            leftWingOuterRest = leftWingOuter.localRotation;
            rightWingRest = rightWing.localRotation;
            rightWingOuterRest = rightWingOuter.localRotation;
            leftLegRest = leftLeg.localRotation;
            rightLegRest = rightLeg.localRotation;
            tailRest = tail.localRotation;
            tailTipRest = tailTip.localRotation;
            riderCapeRest = riderCape.localRotation;
            riderCapeMiddleRest = riderCapeMiddle.localRotation;
            riderCapeLowerRest = riderCapeLower.localRotation;
        }

        protected override void Animate()
        {
            float pace = Mathf.Lerp(0.6f, 1f, SpeedFactor);
            phase += Time.deltaTime * flapsPerSecond * pace * Mathf.PI * 2f;

            float flap = Mathf.Sin(phase);
            float outerFlap = Mathf.Sin(phase - wingOuterLag);
            float lift = -Mathf.Sin(phase - 0.7f);

            leftWing.localRotation = leftWingRest * Quaternion.Euler(0f, 0f, -wingAngle * flap);
            rightWing.localRotation = rightWingRest * Quaternion.Euler(0f, 0f, wingAngle * flap);
            leftWingOuter.localRotation = leftWingOuterRest * Quaternion.Euler(0f, 0f, -wingOuterAngle * outerFlap);
            rightWingOuter.localRotation = rightWingOuterRest * Quaternion.Euler(0f, 0f, wingOuterAngle * outerFlap);

            body.localPosition = bodyRest + Vector3.up * (bobHeight * lift);
            body.localRotation = bodyRotation * Quaternion.Euler(bodyPitch * Mathf.Sin(phase + 0.9f), 0f, 0f);

            float wave = Mathf.Sin(phase * 0.5f);
            neck.localRotation = neckRest * Quaternion.Euler(-neckWave * 0.6f * lift, neckWave * wave, 0f);
            head.localRotation = headRest * Quaternion.Euler(neckWave * 0.8f * lift, -neckWave * 0.7f * wave, 0f);
            jaw.localRotation = jawRest * Quaternion.Euler(4f + 4f * Mathf.Sin(phase * 0.35f), 0f, 0f);

            tail.localRotation = tailRest * Quaternion.Euler(tailWave * 0.5f * Mathf.Sin(phase - 1.2f), tailWave * Mathf.Sin(phase * 0.5f - 1f), 0f);
            tailTip.localRotation = tailTipRest * Quaternion.Euler(tailWave * 0.6f * Mathf.Sin(phase - 2f), tailWave * 1.4f * Mathf.Sin(phase * 0.5f - 2.1f), 0f);

            leftLeg.localRotation = leftLegRest * Quaternion.Euler(legSway * Mathf.Sin(phase - 1.5f), 0f, 0f);
            rightLeg.localRotation = rightLegRest * Quaternion.Euler(legSway * Mathf.Sin(phase - 1.7f), 0f, 0f);

            float gust = Time.time * capeWaves * Mathf.PI * 2f;
            riderCape.localRotation = riderCapeRest * Quaternion.Euler(capeFlutter * 0.5f * Mathf.Sin(gust) + 4f * lift, 0f, capeFlutter * 0.3f * Mathf.Sin(gust * 0.5f));
            riderCapeMiddle.localRotation = riderCapeMiddleRest * Quaternion.Euler(capeFlutter * Mathf.Sin(gust - 1.1f), 0f, capeFlutter * 0.5f * Mathf.Sin(gust * 0.5f - 0.8f));
            riderCapeLower.localRotation = riderCapeLowerRest * Quaternion.Euler(capeFlutter * 1.4f * Mathf.Sin(gust - 2.2f), 0f, capeFlutter * 0.7f * Mathf.Sin(gust * 0.5f - 1.6f));
        }

        protected override void OnKilled()
        {
            base.OnKilled();

            Wraith wraith = Instantiate(wraithPrefab, riderStand.position, transform.rotation);
            wraith.BecomeSuccessorOf(this);

            rider.gameObject.SetActive(false);
            leftWing.localRotation = leftWingRest * Quaternion.Euler(0f, 0f, -55f);
            rightWing.localRotation = rightWingRest * Quaternion.Euler(0f, 0f, 55f);

            model.SetParent(null, true);
            UnitCorpse corpse = model.gameObject.AddComponent<UnitCorpse>();
            corpse.Fall(wraith.TravelHeight, corpseThickness * model.lossyScale.y, Random.value < 0.5f ? -1f : 1f);
        }

        public override void UpgradeHealth(float upByPercentage)
        {
            base.UpgradeHealth(upByPercentage);

            if (wraithPrefab != null)
                wraithPrefab.UpgradeHealth(upByPercentage);
        }
    }

}
