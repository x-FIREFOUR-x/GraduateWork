using UnityEngine;

using TowerDefense.Main.Units.Squad;


namespace TowerDefense.Main.Units
{
    public class OrcAlphaWargRider : SquadLeader
    {
        [Header("Warg")]
        [SerializeField]
        private Transform warg;
        [SerializeField]
        private Transform wargHead;
        [SerializeField]
        private Transform tail;
        [SerializeField]
        private Transform frontLeftLeg;
        [SerializeField]
        private Transform frontLeftPaw;
        [SerializeField]
        private Transform frontRightLeg;
        [SerializeField]
        private Transform frontRightPaw;
        [SerializeField]
        private Transform hindLeftLeg;
        [SerializeField]
        private Transform hindLeftPaw;
        [SerializeField]
        private Transform hindRightLeg;
        [SerializeField]
        private Transform hindRightPaw;

        [Header("Rider")]
        [SerializeField]
        private Transform rider;
        [SerializeField]
        private Transform riderHead;
        [SerializeField]
        private Transform weaponArm;
        [SerializeField]
        private Transform weaponForearm;
        [SerializeField]
        private Transform offhandArm;
        [SerializeField]
        private Transform offhandForearm;

        [Header("Gallop")]
        [SerializeField]
        private float stridesPerSecond = 2.6f;
        [SerializeField]
        private float frontSwing = 34f;
        [SerializeField]
        private float hindSwing = 30f;
        [SerializeField]
        private float frontFold = 80f;
        [SerializeField]
        private float hindFold = 50f;
        [SerializeField]
        private float bodyPitch = 6f;
        [SerializeField]
        private float bobHeight = 0.035f;
        [SerializeField]
        private float headNod = 9f;
        [SerializeField]
        private float tailLift = 14f;
        [SerializeField]
        private float tailSway = 10f;
        [SerializeField]
        private Vector4 legPhases = new(0.45f, 0.57f, 0f, 0.12f);

        [Header("Rider seat")]
        [SerializeField]
        private float riderLean = 10f;
        [SerializeField]
        private float riderBounce = 0.02f;
        [SerializeField]
        private float seatAbsorb = 0.55f;
        [SerializeField]
        private float seatLag = 0.55f;
        [SerializeField]
        private float seatSlide = 0.006f;
        [SerializeField]
        private float torsoSteady = 0.75f;
        [SerializeField]
        private float torsoLag = 0.35f;
        [SerializeField]
        private float torsoRock = 2.5f;
        [SerializeField]
        private float torsoRoll = 1.5f;
        [SerializeField]
        private float headSteady = 0.85f;

        [Header("Rider arms")]
        [SerializeField]
        private float swordHoldRaise = -58f;
        [SerializeField]
        private float swordHoldForearm = 35f;
        [SerializeField]
        private float offhandHoldRaise;
        [SerializeField]
        private float offhandHoldForearm;
        [SerializeField]
        private float swordBounce = 9f;
        [SerializeField]
        private float armLag = 0.8f;
        [SerializeField, Range(0f, 0.9f)]
        private float offhandSwingDelay = 0.4f;
        [SerializeField]
        private Vector2 swingEvery = new(2.5f, 5f);
        [SerializeField]
        private float swingDuration = 1.1f;
        [SerializeField]
        private float swingWindup = 50f;
        [SerializeField]
        private float swingStrike = 15f;
        [SerializeField]
        private float swingForearmWindup = 70f;
        [SerializeField]
        private float swingForearmStrike = 50f;
        [SerializeField]
        private float swingTwist = 10f;

        private float phase, swingTime, nextSwing;
        private Vector3 wargRest, riderRest;
        private Quaternion wargRotation, wargHeadRest, tailRest, riderRotation, riderHeadRest;
        private Quaternion frontLeftLegRest, frontLeftPawRest, frontRightLegRest, frontRightPawRest;
        private Quaternion hindLeftLegRest, hindLeftPawRest, hindRightLegRest, hindRightPawRest;
        private Quaternion weaponArmRest, weaponForearmRest, offhandArmRest, offhandForearmRest;
        private Vector3 weaponElbowHinge, offhandElbowHinge;


        protected override Transform Torso { get { return warg; } }

        protected override void Start()
        {
            base.Start();

            phase = Random.Range(0f, 1f);
            swingTime = -1f;
            nextSwing = Random.Range(swingEvery.x, swingEvery.y);

            wargRest = warg.localPosition;
            wargRotation = warg.localRotation;
            wargHeadRest = wargHead.localRotation;
            tailRest = tail.localRotation;
            frontLeftLegRest = frontLeftLeg.localRotation;
            frontLeftPawRest = frontLeftPaw.localRotation;
            frontRightLegRest = frontRightLeg.localRotation;
            frontRightPawRest = frontRightPaw.localRotation;
            hindLeftLegRest = hindLeftLeg.localRotation;
            hindLeftPawRest = hindLeftPaw.localRotation;
            hindRightLegRest = hindRightLeg.localRotation;
            hindRightPawRest = hindRightPaw.localRotation;
            riderRest = rider.localPosition;
            riderRotation = rider.localRotation;
            riderHeadRest = riderHead.localRotation;
            weaponArmRest = weaponArm.localRotation;
            weaponForearmRest = weaponForearm.localRotation;
            offhandArmRest = offhandArm.localRotation;
            offhandForearmRest = offhandForearm.localRotation;
            weaponElbowHinge = ElbowHinge(weaponForearm);
            offhandElbowHinge = ElbowHinge(offhandForearm);
        }

        protected override void Animate()
        {
            float pace = Mathf.Lerp(0.5f, 1f, SpeedFactor);
            phase = Mathf.Repeat(phase + Time.deltaTime * stridesPerSecond * pace, 1f);
            float angle = phase * Mathf.PI * 2f;

            AnimateLeg(frontLeftLeg, frontLeftLegRest, frontLeftPaw, frontLeftPawRest, legPhases.x, frontSwing, frontFold);
            AnimateLeg(frontRightLeg, frontRightLegRest, frontRightPaw, frontRightPawRest, legPhases.y, frontSwing, frontFold);
            AnimateLeg(hindLeftLeg, hindLeftLegRest, hindLeftPaw, hindLeftPawRest, legPhases.z, hindSwing, hindFold);
            AnimateLeg(hindRightLeg, hindRightLegRest, hindRightPaw, hindRightPawRest, legPhases.w, hindSwing, hindFold);

            float pitch = Pitch(angle);
            warg.localPosition = wargRest + Vector3.up * Bob(angle);
            warg.localRotation = wargRotation * Quaternion.Euler(pitch, 0f, 0f);
            wargHead.localRotation = wargHeadRest * Quaternion.Euler(-pitch * 0.6f + headNod * Mathf.Sin(angle + 2.4f), 0f, 0f);
            tail.localRotation = tailRest * Quaternion.Euler(-tailLift * (0.5f + 0.5f * Mathf.Sin(angle - 0.6f)), tailSway * Mathf.Sin(angle * 0.5f), 0f);

            AnimateRider(angle);
        }

        private float Pitch(float angle)
        {
            return bodyPitch * Mathf.Sin(angle + 0.9f);
        }

        private float Bob(float angle)
        {
            return bobHeight * Mathf.Sin(angle * 2f + 0.4f) * 0.4f + bobHeight * Mathf.Sin(angle + 2.2f);
        }

        private float BobAcceleration(float angle)
        {
            return -(Mathf.Sin(angle * 2f + 0.4f) * 1.6f + Mathf.Sin(angle + 2.2f)) / 2.6f;
        }

        private void AnimateRider(float angle)
        {
            float chop = UpdateSwing();
            float offhandChop = swingTime > offhandSwingDelay ? Chop((swingTime - offhandSwingDelay) / (1f - offhandSwingDelay)) : 0f;
            float windup = Mathf.Max(chop, 0f), strike = Mathf.Max(-chop, 0f);
            float offhandWindup = Mathf.Max(offhandChop, 0f), offhandStrike = Mathf.Max(-offhandChop, 0f);
            float twist = swingTwist * (chop - offhandChop);

            float hop = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(angle - seatLag), 1.5f) - 0.4f;
            float seat = -seatAbsorb * Bob(angle - seatLag) + riderBounce * hop;
            float slide = seatSlide * Mathf.Sin(angle - seatLag + 1.2f);
            float steadied = -Pitch(angle - torsoLag) * torsoSteady;
            float rock = torsoRock * BobAcceleration(angle - torsoLag);
            float roll = torsoRoll * Mathf.Sin(angle + 0.3f);
            float torsoPitch = riderLean + steadied + rock + Mathf.Max(strike, offhandStrike) * 4f;

            rider.localPosition = riderRest + new Vector3(0f, seat, slide);
            rider.localRotation = riderRotation * Quaternion.Euler(torsoPitch, twist, roll);

            float bodyWorldPitch = Pitch(angle) + steadied + rock;
            riderHead.localRotation = riderHeadRest * Quaternion.Euler(-riderLean * 0.8f - bodyWorldPitch * headSteady - Mathf.Max(strike, offhandStrike) * 3f, -twist * 0.7f, -roll * headSteady);

            float inertia = swordBounce * Mathf.Sin(angle - seatLag - armLag + 1.57f);
            float offhandInertia = swordBounce * Mathf.Sin(angle - seatLag - armLag + 2.2f);
            weaponArm.localRotation = weaponArmRest * Quaternion.Euler(swordHoldRaise + inertia - swingWindup * windup + swingStrike * strike, 0f, 12f);
            weaponForearm.localRotation = weaponForearmRest * Quaternion.AngleAxis(swordHoldForearm + inertia * 0.8f - swingForearmWindup * windup + swingForearmStrike * strike, weaponElbowHinge);
            offhandArm.localRotation = offhandArmRest * Quaternion.Euler(offhandHoldRaise + offhandInertia - swingWindup * offhandWindup + swingStrike * offhandStrike, 0f, -12f);
            offhandForearm.localRotation = offhandForearmRest * Quaternion.AngleAxis(offhandHoldForearm + offhandInertia * 0.4f - swingForearmWindup * offhandWindup, offhandElbowHinge);
        }

        private static Vector3 ElbowHinge(Transform forearm)
        {
            Vector3 hinge = Vector3.Cross(Vector3.forward, forearm.localPosition);
            return hinge.sqrMagnitude > 1e-8f ? hinge.normalized : Vector3.right;
        }

        private float UpdateSwing()
        {
            if (swingTime < 0f)
            {
                nextSwing -= Time.deltaTime;
                if (nextSwing > 0f)
                    return 0f;
                swingTime = 0f;
            }

            swingTime += Time.deltaTime / swingDuration;
            if (swingTime >= 1f)
            {
                swingTime = -1f;
                nextSwing = Random.Range(swingEvery.x, swingEvery.y);
                return 0f;
            }
            return Chop(swingTime);
        }

        private static float Chop(float t)
        {
            if (t < 0.5f)
                return Mathf.SmoothStep(0f, 1f, t / 0.5f);
            if (t < 0.62f)
                return Mathf.Lerp(1f, -1f, Mathf.SmoothStep(0f, 1f, (t - 0.5f) / 0.12f));
            return Mathf.Lerp(-1f, 0f, Mathf.SmoothStep(0f, 1f, (t - 0.62f) / 0.38f));
        }

        private void AnimateLeg(Transform leg, Quaternion legRest, Transform paw, Quaternion pawRest, float offset, float swing, float fold)
        {
            float angle = (phase + offset) * Mathf.PI * 2f;
            leg.localRotation = legRest * Quaternion.Euler(swing * Mathf.Sin(angle), 0f, 0f);
            paw.localRotation = pawRest * Quaternion.Euler(fold * Mathf.Max(0f, -Mathf.Cos(angle)), 0f, 0f);
        }
    }

}
