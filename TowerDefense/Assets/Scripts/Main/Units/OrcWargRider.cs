using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class OrcWargRider : Unit
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
        private Transform braid;
        [SerializeField]
        private Transform braidTip;
        [SerializeField]
        private Transform weaponArm;
        [SerializeField]
        private Transform weaponForearm;
        [SerializeField]
        private Transform shieldArm;
        [SerializeField]
        private Transform shieldForearm;

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
        private float axeHoldRaise = -58f;
        [SerializeField]
        private float axeHoldForearm = 35f;
        [SerializeField]
        private float axeBounce = 9f;
        [SerializeField]
        private float armLag = 0.8f;
        [SerializeField]
        private float shieldRaise = -5f;
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

        [Header("Braid")]
        [SerializeField]
        private float braidTipLength = 0.14f;
        [SerializeField]
        private float braidGravity = 6f;
        [SerializeField]
        private float braidSpring = 4f;
        [SerializeField]
        private float braidFriction = 2.5f;
        [SerializeField]
        private float braidAirDrag = 1.5f;
        [SerializeField]
        private float braidMaxBend = 50f;
        [SerializeField]
        private float braidForwardLimit = 0.12f;

        private const float braidStep = 1f / 120f;

        private float phase, swingTime, nextSwing;
        private Vector3 wargRest, riderRest;
        private Quaternion wargRotation, wargHeadRest, tailRest, riderRotation, riderHeadRest, braidRest, braidTipRest;
        private Quaternion frontLeftLegRest, frontLeftPawRest, frontRightLegRest, frontRightPawRest;
        private Quaternion hindLeftLegRest, hindLeftPawRest, hindRightLegRest, hindRightPawRest;
        private Quaternion weaponArmRest, weaponForearmRest, shieldArmRest, shieldForearmRest;
        private Vector3 braidRestDirection, braidTipRestDirection;
        private Vector3 braidJoint, braidJointPrevious, braidEnd, braidEndPrevious, braidRootPrevious, braidAirVelocity;


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
            braidRest = braid.localRotation;
            braidTipRest = braidTip.localRotation;
            weaponArmRest = weaponArm.localRotation;
            weaponForearmRest = weaponForearm.localRotation;
            shieldArmRest = shieldArm.localRotation;
            shieldForearmRest = shieldForearm.localRotation;

            braidRestDirection = braidTip.localPosition.normalized;
            braidTipRestDirection = new Vector3(0f, -1f, -0.07f).normalized;
            ResetBraid();
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
            AnimateBraid(Time.deltaTime, pace);
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
            float windup = Mathf.Max(chop, 0f), strike = Mathf.Max(-chop, 0f);
            float twist = swingTwist * chop;

            float hop = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(angle - seatLag), 1.5f) - 0.4f;
            float seat = -seatAbsorb * Bob(angle - seatLag) + riderBounce * hop;
            float slide = seatSlide * Mathf.Sin(angle - seatLag + 1.2f);
            float steadied = -Pitch(angle - torsoLag) * torsoSteady;
            float rock = torsoRock * BobAcceleration(angle - torsoLag);
            float roll = torsoRoll * Mathf.Sin(angle + 0.3f);
            float torsoPitch = riderLean + steadied + rock + strike * 4f;

            rider.localPosition = riderRest + new Vector3(0f, seat, slide);
            rider.localRotation = riderRotation * Quaternion.Euler(torsoPitch, twist, roll);

            float bodyWorldPitch = Pitch(angle) + steadied + rock;
            riderHead.localRotation = riderHeadRest * Quaternion.Euler(-riderLean * 0.8f - bodyWorldPitch * headSteady - strike * 3f, -twist * 0.7f, -roll * headSteady);

            float inertia = axeBounce * Mathf.Sin(angle - seatLag - armLag + 1.57f);
            weaponArm.localRotation = weaponArmRest * Quaternion.Euler(axeHoldRaise + inertia - swingWindup * windup + swingStrike * strike, 0f, 12f);
            weaponForearm.localRotation = weaponForearmRest * Quaternion.Euler(axeHoldForearm + inertia * 0.8f - swingForearmWindup * windup + swingForearmStrike * strike, 0f, 0f);
            shieldArm.localRotation = shieldArmRest * Quaternion.Euler(shieldRaise + inertia * 0.6f - strike * 4f, 0f, 0f);
            shieldForearm.localRotation = shieldForearmRest * Quaternion.Euler(-10f - inertia * 0.4f, 0f, 0f);
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

        private void ResetBraid()
        {
            braid.localRotation = braidRest;
            braidTip.localRotation = braidTipRest;
            braidRootPrevious = braid.position;
            braidJoint = braidJointPrevious = braidTip.position;
            braidEnd = braidEndPrevious = braidTip.position + braidTip.rotation * braidTipRestDirection * BraidTipWorldLength();
            braidAirVelocity = Vector3.zero;
        }

        private float BraidTipWorldLength()
        {
            return braidTipLength * braidTip.lossyScale.y;
        }

        private void AnimateBraid(float deltaTime, float pace)
        {
            braid.localRotation = braidRest;
            braidTip.localRotation = braidTipRest;
            if (deltaTime <= 0f)
                return;

            Vector3 root = braid.position;
            float scale = braid.lossyScale.y;
            float jointLength = braidTip.localPosition.magnitude * scale;
            float endLength = BraidTipWorldLength();
            Vector3 restDirection = braid.rotation * braidRestDirection;
            Vector3 restTipDirection = braidTip.rotation * braidTipRestDirection;
            Vector3 forward = rider.forward;

            Vector3 rootVelocity = (root - braidRootPrevious) / deltaTime;
            if (rootVelocity.magnitude > scale * 40f)
            {
                ResetBraid();
                return;
            }
            braidAirVelocity = Vector3.Lerp(braidAirVelocity, rootVelocity, 1f - Mathf.Exp(-deltaTime * 6f));
            Vector3 wind = -braidAirVelocity * braidAirDrag * pace;
            Vector3 gravity = Vector3.down * braidGravity * scale;

            int steps = Mathf.Clamp(Mathf.CeilToInt(deltaTime / braidStep), 1, 6);
            float step = Mathf.Min(deltaTime, braidStep * 6f) / steps;
            float keep = Mathf.Exp(-braidFriction * step);
            float pull = 1f - Mathf.Exp(-braidSpring * step);

            for (int i = 0; i < steps; i++)
            {
                Vector3 stepRoot = Vector3.Lerp(braidRootPrevious, root, (i + 1f) / steps);
                Integrate(ref braidJoint, ref braidJointPrevious, gravity + wind, keep, step);
                Integrate(ref braidEnd, ref braidEndPrevious, gravity + wind * 1.4f, keep, step);

                braidJoint = Vector3.Lerp(braidJoint, stepRoot + restDirection * jointLength, pull);
                braidJoint = stepRoot + Limit(braidJoint - stepRoot, restDirection, forward).normalized * jointLength;

                Vector3 bentTipDirection = Quaternion.FromToRotation(restDirection, (braidJoint - stepRoot).normalized) * restTipDirection;
                braidEnd = Vector3.Lerp(braidEnd, braidJoint + bentTipDirection * endLength, pull * 0.8f);
                braidEnd = braidJoint + Limit(braidEnd - braidJoint, bentTipDirection, forward).normalized * endLength;
            }
            braidRootPrevious = root;

            braid.rotation = Quaternion.FromToRotation(restDirection, (braidJoint - root).normalized) * braid.rotation;
            Vector3 currentTipDirection = braidTip.rotation * braidTipRestDirection;
            braidTip.rotation = Quaternion.FromToRotation(currentTipDirection, (braidEnd - braidTip.position).normalized) * braidTip.rotation;
        }

        private static void Integrate(ref Vector3 point, ref Vector3 previous, Vector3 acceleration, float keep, float step)
        {
            Vector3 velocity = (point - previous) * keep;
            previous = point;
            point += velocity + acceleration * (step * step);
        }

        private Vector3 Limit(Vector3 direction, Vector3 rest, Vector3 forward)
        {
            direction.Normalize();
            float into = Vector3.Dot(direction - rest, forward);
            if (into > braidForwardLimit)
                direction = (direction - forward * (into - braidForwardLimit)).normalized;
            float current = Vector3.Angle(rest, direction);
            if (current <= braidMaxBend)
                return direction;
            return Vector3.Slerp(rest, direction, braidMaxBend / current);
        }

        private void AnimateLeg(Transform leg, Quaternion legRest, Transform paw, Quaternion pawRest, float offset, float swing, float fold)
        {
            float angle = (phase + offset) * Mathf.PI * 2f;
            leg.localRotation = legRest * Quaternion.Euler(swing * Mathf.Sin(angle), 0f, 0f);
            paw.localRotation = pawRest * Quaternion.Euler(fold * Mathf.Max(0f, -Mathf.Cos(angle)), 0f, 0f);
        }
    }

}
