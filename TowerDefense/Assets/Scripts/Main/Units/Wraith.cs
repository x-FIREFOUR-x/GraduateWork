using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class Wraith : Unit
    {
        [Header("Wraith")]
        [SerializeField]
        private Transform body;
        [SerializeField]
        private Transform hood;
        [SerializeField]
        private Transform leftArm;
        [SerializeField]
        private Transform rightArm;
        [SerializeField]
        private Transform skirt;
        [SerializeField]
        private Transform cape;
        [SerializeField]
        private Transform capeMiddle;
        [SerializeField]
        private Transform capeLower;

        [Header("Glide")]
        [SerializeField]
        private float glidesPerSecond = 1.1f;
        [SerializeField]
        private float bobHeight = 0.04f;
        [SerializeField]
        private float bodySway = 4f;
        [SerializeField]
        private float hoodNod = 5f;
        [SerializeField]
        private float armSwing = 10f;
        [SerializeField]
        private float clothTrail = 12f;
        [SerializeField]
        private float clothFlutter = 6f;

        [Header("Fall")]
        [SerializeField]
        private float gravity = 16f;
        [SerializeField]
        private float clothLift = 55f;
        [SerializeField]
        private float recoveryTime = 1.2f;
        [SerializeField]
        private float landingSquash = 0.12f;
        [SerializeField]
        private ParticleSystem mist;
        [SerializeField]
        private float fallMistRate = 80f;
        [SerializeField]
        private int landingMistBurst = 70;
        [SerializeField]
        private Color fallMistColor = new(0.005f, 0.005f, 0.01f, 0.95f);
        [SerializeField]
        private float fallMistSize = 2.2f;

        private float phase;
        private bool isFalling;
        private float fallSpeed;
        private float landing = -1f;
        private float baseMistRate;
        private ParticleSystem.MinMaxGradient baseMistColor;
        private float baseMistSize;
        private float motion = 1f;
        private Vector3 bodyRest;
        private Quaternion bodyRotation, hoodRest, leftArmRest, rightArmRest, skirtRest, capeRest, capeMiddleRest, capeLowerRest;


        protected override Transform Torso { get { return body; } }

        protected override void Start()
        {
            base.Start();

            phase = Random.Range(0f, Mathf.PI * 2f);

            bodyRest = body.localPosition;
            bodyRotation = body.localRotation;
            hoodRest = hood.localRotation;
            leftArmRest = leftArm.localRotation;
            rightArmRest = rightArm.localRotation;
            skirtRest = skirt.localRotation;
            capeRest = cape.localRotation;
            capeMiddleRest = capeMiddle.localRotation;
            capeLowerRest = capeLower.localRotation;

            if (IsSuccessor && transform.position.y > TravelHeight)
            {
                isFalling = true;
                CanMove = false;
                CanBeTargeted = false;
                ShroudInMist();
            }
        }

        protected override void Animate()
        {
            if (isFalling)
            {
                Fall();
                return;
            }

            motion = Mathf.MoveTowards(motion, CanMove ? 1f : 0f, Time.deltaTime * 2f);
            float pace = Mathf.Lerp(0.6f, 1f, SpeedFactor);
            phase += Time.deltaTime * glidesPerSecond * pace * Mathf.PI * 2f * motion;

            float sway = Mathf.Sin(phase) * motion;
            float bob = Mathf.Sin(phase * 2f) * motion;
            float drop = Land();
            float slump = drop / landingSquash;

            body.localPosition = bodyRest + Vector3.up * (bobHeight * bob - drop);
            body.localRotation = bodyRotation * Quaternion.Euler(3f + 2f * bob + 25f * slump, bodySway * 0.5f * sway, bodySway * sway);
            hood.localRotation = hoodRest * Quaternion.Euler(hoodNod * Mathf.Sin(phase * 2f + 0.8f) * motion + 12f * slump, -bodySway * 0.4f * sway, 0f);

            leftArm.localRotation = leftArmRest * Quaternion.Euler(armSwing * sway, 0f, 2f * bob);
            rightArm.localRotation = rightArmRest * Quaternion.Euler(-armSwing * 0.5f * sway, 0f, -2f * bob);

            float trail = clothTrail * Mathf.Lerp(0.5f, 1f, SpeedFactor) * motion;
            float flutter = clothFlutter * motion;
            skirt.localRotation = skirtRest * Quaternion.Euler(trail * 0.4f + flutter * 0.5f * Mathf.Sin(phase * 2f - 0.6f), 0f, -bodySway * 0.6f * sway);
            cape.localRotation = capeRest * Quaternion.Euler(trail + flutter * Mathf.Sin(phase * 2f - 1.2f), 0f, flutter * 0.4f * Mathf.Sin(phase - 0.5f));
            capeMiddle.localRotation = capeMiddleRest * Quaternion.Euler(trail * 0.4f + flutter * Mathf.Sin(phase * 2f - 2f), 0f,
                                                                         flutter * 0.5f * Mathf.Sin(phase - 1.1f));
            capeLower.localRotation = capeLowerRest * Quaternion.Euler(trail * 0.4f + flutter * 1.3f * Mathf.Sin(phase * 2f - 2.8f), 0f,
                                                                       flutter * 0.6f * Mathf.Sin(phase - 1.7f));
        }

        private void Fall()
        {
            fallSpeed += gravity * Time.deltaTime;
            float height = transform.position.y - fallSpeed * Time.deltaTime;

            float flare = Mathf.Clamp01(fallSpeed / 6f);
            float flutter = Mathf.Sin(Time.time * 18f);
            skirt.localRotation = skirtRest * Quaternion.Euler(clothLift * 0.35f * flare + 4f * flutter, 0f, 0f);
            cape.localRotation = capeRest * Quaternion.Euler(clothLift * 0.6f * flare + 6f * flutter, 0f, 0f);
            capeMiddle.localRotation = capeMiddleRest * Quaternion.Euler(clothLift * 0.4f * flare + 9f * Mathf.Sin(Time.time * 18f - 1f), 0f, 0f);
            capeLower.localRotation = capeLowerRest * Quaternion.Euler(clothLift * 0.3f * flare + 12f * Mathf.Sin(Time.time * 18f - 2f), 0f, 0f);
            leftArm.localRotation = leftArmRest * Quaternion.Euler(-40f * flare, 0f, -25f * flare);
            rightArm.localRotation = rightArmRest * Quaternion.Euler(-30f * flare, 0f, 20f * flare);

            if (height <= TravelHeight)
            {
                height = TravelHeight;
                isFalling = false;
                landing = 0f;
                BurstMist();
            }

            transform.position = new Vector3(transform.position.x, height, transform.position.z);
        }

        private float Land()
        {
            if (landing < 0f)
                return 0f;

            landing += Time.deltaTime / recoveryTime;
            if (landing >= 1f)
            {
                landing = -1f;
                CanMove = true;
                CanBeTargeted = true;
                ReleaseMist();
                return 0f;
            }

            float squash = landing < 0.2f ? Mathf.SmoothStep(0f, 1f, landing / 0.2f) : 1f - Mathf.SmoothStep(0f, 1f, (landing - 0.2f) / 0.8f);
            return landingSquash * squash;
        }

        private void ShroudInMist()
        {
            if (mist == null)
                return;

            ParticleSystem.EmissionModule emission = mist.emission;
            baseMistRate = emission.rateOverTime.constant;
            emission.rateOverTime = fallMistRate;
            ParticleSystem.MainModule main = mist.main;
            baseMistColor = main.startColor;
            main.startColor = fallMistColor;
            baseMistSize = main.startSizeMultiplier;
            main.startSizeMultiplier = baseMistSize * fallMistSize;
        }

        private void BurstMist()
        {
            if (mist != null)
                mist.Emit(landingMistBurst);
        }

        private void ReleaseMist()
        {
            if (mist == null)
                return;

            ParticleSystem.EmissionModule emission = mist.emission;
            emission.rateOverTime = baseMistRate;
            ParticleSystem.MainModule main = mist.main;
            main.startColor = baseMistColor;
            main.startSizeMultiplier = baseMistSize;
        }
    }

}
