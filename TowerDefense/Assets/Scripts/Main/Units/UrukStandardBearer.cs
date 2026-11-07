using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class UrukStandardBearer : Unit
    {
        [Header("Uruk")]
        [SerializeField]
        private Transform body;
        [SerializeField]
        private Transform head;
        [SerializeField]
        private Transform leftThigh;
        [SerializeField]
        private Transform leftShin;
        [SerializeField]
        private Transform rightThigh;
        [SerializeField]
        private Transform rightShin;
        [SerializeField]
        private Transform leftArm;
        [SerializeField]
        private Transform rightArm;
        [SerializeField]
        private Transform leftForearm;
        [SerializeField]
        private Transform rightForearm;
        [SerializeField]
        private Transform frontFlap;
        [SerializeField]
        private Transform backFlap;

        [Header("Standard")]
        [SerializeField]
        private Transform standard;
        [SerializeField]
        private Transform banner;
        [SerializeField]
        private Transform bannerMiddle;
        [SerializeField]
        private Transform bannerLower;

        [Header("March")]
        [SerializeField]
        private float stepsPerSecond = 1.9f;
        [SerializeField]
        private float legSwing = 24f;
        [SerializeField]
        private float kneeBend = 34f;
        [SerializeField]
        private float armSwing = 22f;
        [SerializeField]
        private float elbowBend = 18f;
        [SerializeField]
        private float elbowPump = 22f;
        [SerializeField]
        private float standardArmSwing = 4f;
        [SerializeField]
        private float bodyTwist = 5f;
        [SerializeField]
        private float bodyRoll = 3f;
        [SerializeField]
        private float bobHeight = 0.03f;
        [SerializeField]
        private float headNod = 3f;

        [Header("Crouch")]
        [SerializeField]
        private float crouchAngle = 12f;
        [SerializeField]
        private float crouchLean = 5f;
        [SerializeField]
        private Vector3 footSole = new(0f, -0.41f, 0f);

        [Header("Kilt")]
        [SerializeField]
        private float flapFollow = 0.8f;
        [SerializeField]
        private float flapDrag = 10f;
        [SerializeField]
        private float flapFlutter = 6f;

        [Header("Banner")]
        [SerializeField]
        private float standardTilt = -3f;
        [SerializeField]
        private float standardSway = 3f;
        [SerializeField]
        private float bannerDrag = 16f;
        [SerializeField]
        private float bannerFlutter = 9f;
        [SerializeField]
        private float bannerWaves = 1.6f;

        [Header("Fall")]
        [SerializeField]
        private float poleThickness = 0.05f;

        private float phase;
        private float crouchDrop;
        private Vector3 bodyRest;
        private Quaternion bodyRotation, headRest, leftThighRest, leftShinRest, rightThighRest, rightShinRest, leftArmRest, rightArmRest;
        private Quaternion leftForearmRest, rightForearmRest, frontFlapRest, backFlapRest, standardRest, bannerRest, bannerMiddleRest, bannerLowerRest;


        protected override Transform Torso { get { return body; } }

        protected override void Start()
        {
            base.Start();

            phase = Random.Range(0f, Mathf.PI * 2f);

            bodyRest = body.localPosition;
            bodyRotation = body.localRotation;
            headRest = head.localRotation;
            leftThighRest = leftThigh.localRotation;
            leftShinRest = leftShin.localRotation;
            rightThighRest = rightThigh.localRotation;
            rightShinRest = rightShin.localRotation;
            leftArmRest = leftArm.localRotation;
            rightArmRest = rightArm.localRotation;
            leftForearmRest = leftForearm.localRotation;
            rightForearmRest = rightForearm.localRotation;
            frontFlapRest = frontFlap.localRotation;
            backFlapRest = backFlap.localRotation;
            standardRest = Quaternion.Inverse(model.rotation) * standard.rotation;
            bannerRest = banner.localRotation;
            bannerMiddleRest = bannerMiddle.localRotation;
            bannerLowerRest = bannerLower.localRotation;

            crouchDrop = MeasureCrouchDrop();
        }

        private float MeasureCrouchDrop()
        {
            Transform model = body.parent;
            float standing = model.InverseTransformPoint(leftShin.TransformPoint(footSole)).y;

            body.localRotation = bodyRotation * Quaternion.Euler(crouchLean, 0f, 0f);
            leftThigh.localRotation = leftThighRest * Quaternion.Euler(-crouchAngle - crouchLean, 0f, 0f);
            leftShin.localRotation = leftShinRest * Quaternion.Euler(crouchAngle * 2f, 0f, 0f);
            float crouched = model.InverseTransformPoint(leftShin.TransformPoint(footSole)).y;

            body.localRotation = bodyRotation;
            leftThigh.localRotation = leftThighRest;
            leftShin.localRotation = leftShinRest;

            return crouched - standing;
        }

        protected override void Animate()
        {
            float pace = Mathf.Lerp(0.5f, 1f, SpeedFactor);
            phase += Time.deltaTime * stepsPerSecond * pace * Mathf.PI;

            float step = Mathf.Sin(phase);
            float thighCrouch = -crouchAngle - crouchLean;
            float leftLeg = thighCrouch - legSwing * step;
            float rightLeg = thighCrouch + legSwing * step;

            leftThigh.localRotation = leftThighRest * Quaternion.Euler(leftLeg, 0f, 0f);
            rightThigh.localRotation = rightThighRest * Quaternion.Euler(rightLeg, 0f, 0f);
            leftShin.localRotation = leftShinRest * Quaternion.Euler(crouchAngle * 2f + kneeBend * Mathf.Max(0f, Mathf.Sin(phase + 1.2f)), 0f, 0f);
            rightShin.localRotation = rightShinRest * Quaternion.Euler(crouchAngle * 2f + kneeBend * Mathf.Max(0f, Mathf.Sin(phase + 1.2f + Mathf.PI)), 0f, 0f);

            rightArm.localRotation = rightArmRest * Quaternion.Euler(-crouchLean - standardArmSwing * step, 0f, -1f - standardArmSwing * 0.3f * step);
            rightForearm.localRotation = rightForearmRest * Quaternion.Euler(standardArmSwing * 0.5f * step, 0f, 0f);
            leftArm.localRotation = leftArmRest * Quaternion.Euler(-crouchLean * 0.5f + armSwing * step, 0f, 3f - 2f * step);
            leftForearm.localRotation = leftForearmRest * Quaternion.Euler(-elbowBend - elbowPump * (0.5f - 0.5f * step), 0f, 0f);

            body.localPosition = bodyRest + Vector3.up * (bobHeight * Mathf.Abs(Mathf.Cos(phase)) - crouchDrop);
            body.localRotation = bodyRotation * Quaternion.Euler(crouchLean, -bodyTwist * step, bodyRoll * step);
            head.localRotation = headRest * Quaternion.Euler(-crouchLean * 0.8f + headNod * Mathf.Sin(phase * 2f + 0.6f), bodyTwist * 0.8f * step, -bodyRoll * 0.5f * step);

            float flutter = Mathf.Sin(phase * 2f - 0.8f);
            frontFlap.localRotation = frontFlapRest * Quaternion.Euler(Mathf.Min(0f, Mathf.Min(leftLeg, rightLeg) * flapFollow) + flapFlutter * 0.3f * flutter, 0f, 0f);
            backFlap.localRotation = backFlapRest * Quaternion.Euler(flapDrag * pace + flapFlutter * flutter, 0f, -bodyRoll * step);

            float sway = Mathf.Sin(phase - 0.6f);
            float bounce = Mathf.Cos(phase * 2f);
            standard.rotation = model.rotation * standardRest * Quaternion.Euler(standardTilt + standardSway * 0.4f * bounce, 0f, standardSway * sway);

            float gust = Time.time * bannerWaves * Mathf.PI * 2f + phase * 0.5f;
            float drag = bannerDrag * pace;
            float lean = -standardSway * sway;
            banner.localRotation = bannerRest * Quaternion.Euler(-drag * 0.45f - bannerFlutter * 0.4f * Billow(gust), 0f, lean * 0.5f);
            bannerMiddle.localRotation = bannerMiddleRest * Quaternion.Euler(-drag * 0.3f - bannerFlutter * Billow(gust - 1.1f), 0f,
                                                                             lean * 0.4f + bannerFlutter * 0.25f * Mathf.Sin(gust * 0.5f - 0.7f));
            bannerLower.localRotation = bannerLowerRest * Quaternion.Euler(-drag * 0.25f - bannerFlutter * 1.4f * Billow(gust - 2.2f), 0f,
                                                                           lean * 0.3f + bannerFlutter * 0.4f * Mathf.Sin(gust * 0.5f - 1.5f));
        }

        private static float Billow(float angle)
        {
            return 0.5f + 0.5f * Mathf.Sin(angle);
        }

        protected override void OnKilled()
        {
            base.OnKilled();

            standard.SetParent(null, true);
            UnitCorpse corpse = standard.gameObject.AddComponent<UnitCorpse>();
            corpse.Fall(GroundPoint.y, poleThickness * standard.lossyScale.y, Random.value < 0.5f ? -1f : 1f);
        }
    }

}
