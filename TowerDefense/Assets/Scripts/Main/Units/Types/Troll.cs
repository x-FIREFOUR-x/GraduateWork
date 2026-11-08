using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class Troll : Unit
    {
        [Header("Troll")]
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
        private Transform rightHand;
        [SerializeField]
        private Transform leftFrontDrape;
        [SerializeField]
        private Transform rightFrontDrape;
        [SerializeField]
        private Transform leftBackDrape;
        [SerializeField]
        private Transform rightBackDrape;

        [Header("Walk")]
        [SerializeField]
        private float stepsPerSecond = 1.5f;
        [SerializeField]
        private float legSwing = 22f;
        [SerializeField]
        private float kneeBend = 30f;
        [SerializeField]
        private float armSwing = 20f;
        [SerializeField]
        private float elbowBend = 10f;
        [SerializeField]
        private float elbowPump = 20f;
        [SerializeField]
        private float bodyTwist = 6f;
        [SerializeField]
        private float bodySway = 6f;
        [SerializeField]
        private float bobHeight = 0.035f;
        [SerializeField]
        private float headNod = 3f;

        [Header("Crouch")]
        [SerializeField]
        private float crouchAngle = 14f;
        [SerializeField]
        private float crouchLean = 14f;
        [SerializeField]
        private Vector3 footSole = new(0f, -0.415f, -0.035f);

        [Header("Hammer")]
        [SerializeField]
        private Vector4[] hammerPoses =
        {
            new(-165f, 80f, 20f, 4f),
            new(-30f, 80f, 15f, 12f),
            new(-75f, 0f, 0f, 10f),
        };
        [SerializeField]
        private float[] hammerTimes = { 0f, 0.45f, 0.75f };
        [SerializeField]
        private float hammerCycle = 0.5f;

        [Header("Cloth")]
        [SerializeField]
        private float drapeDrag = 10f;
        [SerializeField]
        private float drapeFlutter = 6f;
        [SerializeField]
        private float drapeSpring = 70f;
        [SerializeField]
        private float drapeDamping = 9f;
        [SerializeField]
        private float backFlapSpread = 28f;

        private class Flap
        {
            public Transform Transform;
            public Quaternion Rest;
            public Vector3 Axis;
            public Vector2 Angle, Velocity;

            public Flap(Transform transform, Vector3 axis)
            {
                Transform = transform;
                Rest = transform.localRotation;
                Axis = axis;
            }
        }

        private float phase;
        private Flap leftFrontFlap, rightFrontFlap, leftBackFlap, rightBackFlap;
        private float crouchDrop;
        private Vector3 bodyRest;
        private Quaternion bodyRotation, headRest, leftThighRest, leftShinRest, rightThighRest, rightShinRest, leftArmRest, rightArmRest;
        private Quaternion leftForearmRest, rightForearmRest, rightHandRest;


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
            rightHandRest = rightHand.localRotation;
            leftFrontFlap = new Flap(leftFrontDrape, Vector3.right);
            rightFrontFlap = new Flap(rightFrontDrape, Vector3.right);
            leftBackFlap = new Flap(leftBackDrape, Quaternion.Euler(0f, backFlapSpread, 0f) * Vector3.right);
            rightBackFlap = new Flap(rightBackDrape, Quaternion.Euler(0f, -backFlapSpread, 0f) * Vector3.right);

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

            Vector4 hammer = HammerPose(Mathf.Repeat(phase * hammerCycle / (Mathf.PI * 2f), 1f));
            rightArm.localRotation = rightArmRest * Quaternion.Euler(-crouchLean * 0.4f + hammer.x, 0f, -2f * step + hammer.w);
            rightForearm.localRotation = rightForearmRest * Quaternion.Euler(hammer.y, 0f, 0f);
            rightHand.localRotation = rightHandRest * Quaternion.Euler(hammer.z, 0f, 0f);
            leftArm.localRotation = leftArmRest * Quaternion.Euler(-crouchLean * 0.6f + armSwing * step, 0f, 4f - 3f * step);
            leftForearm.localRotation = leftForearmRest * Quaternion.Euler(-elbowBend - elbowPump * (0.5f - 0.5f * step), 0f, 0f);

            float stomp = 1f - Mathf.Abs(Mathf.Cos(phase));
            body.localPosition = bodyRest + Vector3.up * (bobHeight * (1f - stomp * stomp) - crouchDrop) + Vector3.right * (bobHeight * 0.6f * step);
            body.localRotation = bodyRotation * Quaternion.Euler(crouchLean, -bodyTwist * step, bodySway * step);
            head.localRotation = headRest * Quaternion.Euler(-crouchLean * 0.7f + headNod * Mathf.Sin(phase * 2f + 0.6f), bodyTwist * 0.7f * step, -bodySway * 0.6f * step);

            float drag = drapeDrag * pace;
            float sway = bodySway * 0.3f * step;
            AnimateFlap(leftFrontFlap, new Vector2(-drapeFlutter * 0.3f * (0.5f + 0.5f * Flutter(-1f)), sway));
            AnimateFlap(rightFrontFlap, new Vector2(-drapeFlutter * 0.3f * (0.5f + 0.5f * Flutter(1f)), sway));
            AnimateFlap(leftBackFlap, new Vector2(drag * 0.4f + drapeFlutter * 0.4f * Flutter(-1f), -sway));
            AnimateFlap(rightBackFlap, new Vector2(drag * 0.4f + drapeFlutter * 0.4f * Flutter(1f), -sway));
        }

        private Vector4 HammerPose(float time)
        {
            int count = hammerPoses.Length;
            int i = count - 1;
            while (i > 0 && time < hammerTimes[i])
                i--;
            float end = i + 1 < count ? hammerTimes[i + 1] : 1f;
            float u = (time - hammerTimes[i]) / (end - hammerTimes[i]);

            Vector4 p0 = hammerPoses[(i + count - 1) % count], p1 = hammerPoses[i];
            Vector4 p2 = hammerPoses[(i + 1) % count], p3 = hammerPoses[(i + 2) % count];
            Vector4 m1 = Tangent(p0, p1, p2), m2 = Tangent(p1, p2, p3);
            float u2 = u * u, u3 = u2 * u;
            return (2f * u3 - 3f * u2 + 1f) * p1 + (u3 - 2f * u2 + u) * m1 + (3f * u2 - 2f * u3) * p2 + (u3 - u2) * m2;
        }

        private static Vector4 Tangent(Vector4 previous, Vector4 current, Vector4 next)
        {
            Vector4 tangent = Vector4.zero;
            for (int k = 0; k < 4; k++)
            {
                float before = current[k] - previous[k], after = next[k] - current[k];
                if (before * after > 0f)
                    tangent[k] = Mathf.Sign(after) * Mathf.Min(0.5f * Mathf.Abs(before + after), 3f * Mathf.Min(Mathf.Abs(before), Mathf.Abs(after)));
            }
            return tangent;
        }

        private float Flutter(float side)
        {
            return Mathf.Sin(phase * 2f - 0.8f + side * 0.7f);
        }

        private void AnimateFlap(Flap flap, Vector2 target)
        {
            flap.Velocity += (target - flap.Angle) * (drapeSpring * Time.deltaTime);
            flap.Velocity *= Mathf.Exp(-drapeDamping * Time.deltaTime);
            flap.Angle += flap.Velocity * Time.deltaTime;
            flap.Transform.localRotation = flap.Rest * Quaternion.AngleAxis(flap.Angle.x, flap.Axis) * Quaternion.Euler(0f, 0f, flap.Angle.y);
        }
    }

}
