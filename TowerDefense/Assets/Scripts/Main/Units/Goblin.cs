using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class Goblin : Unit
    {
        [Header("Goblin")]
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
        [SerializeField]
        private Transform sideRag;

        [Header("Walk")]
        [SerializeField]
        private float stepsPerSecond = 2.4f;
        [SerializeField]
        private float legSwing = 26f;
        [SerializeField]
        private float kneeBend = 40f;
        [SerializeField]
        private float armSwing = 30f;
        [SerializeField]
        private float elbowBend = 25f;
        [SerializeField]
        private float elbowPump = 35f;
        [SerializeField]
        private float bodyTwist = 7f;
        [SerializeField]
        private float weaponArmFactor = 0.5f;
        [SerializeField]
        private float bobHeight = 0.03f;
        [SerializeField]
        private float bodyRoll = 4f;
        [SerializeField]
        private float headNod = 4f;

        [Header("Crouch")]
        [SerializeField]
        private float crouchAngle = 32f;
        [SerializeField]
        private float crouchLean = 22f;
        [SerializeField]
        private Vector3 footSole = new(0f, -0.375f, -0.03f);

        [Header("Cloth")]
        [SerializeField]
        private float flapFollow = 0.9f;
        [SerializeField]
        private float flapDrag = 14f;
        [SerializeField]
        private float flapFlutter = 9f;

        private float phase;
        private float crouchDrop;
        private Vector3 bodyRest;
        private Quaternion bodyRotation, headRest, leftThighRest, leftShinRest, rightThighRest, rightShinRest, leftArmRest, rightArmRest;
        private Quaternion leftForearmRest, rightForearmRest, frontFlapRest, backFlapRest, sideRagRest;


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
            sideRagRest = sideRag.localRotation;

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

            rightArm.localRotation = rightArmRest * Quaternion.Euler(-crouchLean * 0.5f - armSwing * weaponArmFactor * step, 0f, -3f - 3f * step);
            leftArm.localRotation = leftArmRest * Quaternion.Euler(-crouchLean * 0.5f + armSwing * step, 0f, 3f - 3f * step);
            rightForearm.localRotation = rightForearmRest * Quaternion.Euler((-elbowBend - elbowPump * (0.5f + 0.5f * step)) * weaponArmFactor, 0f, 0f);
            leftForearm.localRotation = leftForearmRest * Quaternion.Euler(-elbowBend - elbowPump * (0.5f - 0.5f * step), 0f, 0f);

            body.localPosition = bodyRest + Vector3.up * (bobHeight * Mathf.Abs(Mathf.Cos(phase)) - crouchDrop);
            body.localRotation = bodyRotation * Quaternion.Euler(crouchLean, -bodyTwist * step, bodyRoll * step);
            head.localRotation = headRest * Quaternion.Euler(-crouchLean * 0.8f + headNod * Mathf.Sin(phase * 2f + 0.6f), bodyTwist * 0.8f * step, -bodyRoll * 0.5f * step);

            float flutter = Mathf.Sin(phase * 2f - 0.8f);
            frontFlap.localRotation = frontFlapRest * Quaternion.Euler(Mathf.Min(0f, Mathf.Min(leftLeg, rightLeg) * flapFollow) + flapFlutter * 0.3f * flutter, 0f, 0f);
            backFlap.localRotation = backFlapRest * Quaternion.Euler(flapDrag * pace + flapFlutter * flutter, 0f, -bodyRoll * step);
            sideRag.localRotation = sideRagRest * Quaternion.Euler(flapDrag * 0.6f * pace + flapFlutter * Mathf.Sin(phase - 0.9f), 0f, -flapFlutter * 0.5f * Mathf.Abs(flutter));
        }
    }

}
