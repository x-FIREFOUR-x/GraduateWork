using System.Collections;

using UnityEngine;

using TowerDefense.Main.Projectiles;


namespace TowerDefense.Main.Towers
{
    public class BallistaTower : Tower
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject boltPrefab;

        [Header("Ballista Parts")]
        // Everything that kicks back on the shot
        [SerializeField]
        private Transform ballista;
        [SerializeField]
        private Transform bowArmLeft;
        [SerializeField]
        private Transform bowArmRight;
        // Unit long pieces stretched from each bow arm tip to the slider, and from the windlass drum to the slider
        [SerializeField]
        private Transform stringLeft;
        [SerializeField]
        private Transform stringRight;
        [SerializeField]
        private Transform winchRope;
        [SerializeField]
        private Transform slider;
        [SerializeField]
        private Transform winch;
        [SerializeField]
        private GameObject loadedBolt;
        [SerializeField]
        private ParticleSystem releaseDust;

        [Header("Ballista Setup")]
        [SerializeField]
        private float bowArmLength = 1.15f;
        [SerializeField]
        private float bowReleasedAngle = 6f;
        [SerializeField]
        private float bowDrawnAngle = 30f;
        [SerializeField]
        private float sliderReleasedZ = 1.05f;
        [SerializeField]
        private float sliderDrawnZ = -0.87f;
        // Where the rope leaves the windlass drum, in the ballista space
        [SerializeField]
        private Vector3 ropeAnchor;
        [SerializeField]
        private float winchTurns = 2f;

        [Header("Shot")]
        [SerializeField]
        private float releaseTime = 0.06f;
        [SerializeField]
        private float recoilDistance = 0.12f;
        // Parts of the time between shots spent winding the slider back and laying the next bolt; together they stay
        // under the whole of it, so the ballista is always loaded again before it may shoot
        [SerializeField]
        private float windShare = 0.6f;
        [SerializeField]
        private float loadShare = 0.15f;

        [Header("Dwarf")]
        [SerializeField]
        private Transform dwarf;
        [SerializeField]
        private Transform dwarfArmLeft;
        [SerializeField]
        private Transform dwarfArmRight;
        // How far his arms swing up and down, and how far he leans in and out, following the crank handles round
        [SerializeField]
        private float crankSwing = 30f;
        [SerializeField]
        private float crankLean = 4f;

        // 0 with the string slack and the slider at the front, 1 fully drawn
        private float draw = 1f;
        // 1 right after a shot, falling back to 0 as the ballista settles
        private float recoil;
        private bool isReloading;

        private bool hasParts;
        private bool hasDwarf;
        private Vector3 ballistaRest;
        private Vector3 sliderRest;
        private Vector3 dwarfStand;
        private Vector3 dwarfSize;
        private Quaternion dwarfFacing;
        private Vector3 boltSize;


        void Awake()
        {
            hasParts = ballista != null && bowArmLeft != null && bowArmRight != null && stringLeft != null && stringRight != null && slider != null;
            if (hasParts)
            {
                ballistaRest = ballista.localPosition;
                sliderRest = slider.localPosition;
            }

            hasDwarf = dwarf != null && dwarfArmLeft != null && dwarfArmRight != null;
            if (hasDwarf)
            {
                dwarfStand = dwarf.localPosition;
                dwarfSize = dwarf.localScale;
                dwarfFacing = dwarf.localRotation;
            }

            if (loadedBolt != null)
                boltSize = loadedBolt.transform.localScale;

            SetDraw(1f);
        }

        void Update()
        {
            if (target != null)
            {
                RotateToTarget();

                if (timeToNextFire <= 0f && !isReloading)
                {
                    StartCoroutine(Shoot());
                    timeToNextFire = timeBetweenShoots;
                }
            }
            timeToNextFire -= Time.deltaTime;

            if (!isReloading)
                PoseDwarf(0f, Mathf.Sin(Time.time * 2.4f) * 0.015f, 0f, 0f);

            recoil = Mathf.MoveTowards(recoil, 0f, Time.deltaTime * 5f);
            if (hasParts)
            {
                float kick = recoil * recoil;
                ballista.localPosition = ballistaRest + Vector3.back * (recoilDistance * kick);
                ballista.localRotation = Quaternion.Euler(-4f * kick, 0f, 0f);
            }
        }

        private IEnumerator Shoot()
        {
            isReloading = true;

            // Release: the bolt leaves, the string snaps forward, the ballista kicks back and the dwarf flinches
            Fire();
            if (loadedBolt != null)
                loadedBolt.SetActive(false);
            if (releaseDust != null)
                releaseDust.Play();
            recoil = 1f;

            for (float t = 0f; t < 1f; t += Time.deltaTime / releaseTime)
            {
                SetDraw(1f - t * t);
                PoseDwarf(-8f * t, 0f, -20f * t, 0f);
                yield return null;
            }
            SetDraw(0f);

            // Winding: he turns the cranks, his arms going round with the handles and his body leaning in and out with
            // them, the slider coming back with every turn
            float windTime = timeBetweenShoots * windShare;
            for (float t = 0f; t < 1f; t += Time.deltaTime / windTime)
            {
                float wound = Mathf.SmoothStep(0f, 1f, t);
                SetDraw(wound);

                float turn = wound * winchTurns * Mathf.PI * 2f;
                float effort = Mathf.Sin(t * Mathf.PI);
                PoseDwarf(Mathf.Lerp(-8f, 0f, Mathf.Min(1f, t * 4f)) + crankLean * (1f - Mathf.Cos(turn)) * 0.5f, -0.02f * effort,
                          crankSwing * Mathf.Sin(turn), 0f);
                yield return null;
            }
            SetDraw(1f);

            // Loading: the next bolt slides into the groove
            if (loadedBolt != null)
                loadedBolt.SetActive(true);
            float loadTime = timeBetweenShoots * loadShare;
            for (float t = 0f; t < 1f; t += Time.deltaTime / loadTime)
            {
                SetBoltLength(Mathf.SmoothStep(0.2f, 1f, t));
                PoseDwarf(4f * Mathf.Sin(t * Mathf.PI), 0f, 0f, 0f);
                yield return null;
            }
            SetBoltLength(1f);

            isReloading = false;
        }

        private void Fire()
        {
            foreach (Transform start in pointStartFire)
            {
                if (start == null)
                    continue;

                GameObject boltObject = Instantiate(boltPrefab, start.position, start.rotation);
                Projectile bolt = boltObject.GetComponent<Projectile>();

                if (bolt != null)
                    bolt.Seek(target, offsetTarget);
            }
        }

        private void SetDraw(float value)
        {
            draw = value;
            if (!hasParts)
                return;

            float angle = Mathf.Lerp(bowReleasedAngle, bowDrawnAngle, draw);
            bowArmLeft.localRotation = Quaternion.Euler(0f, -angle, 0f);
            bowArmRight.localRotation = Quaternion.Euler(0f, angle, 0f);

            slider.localPosition = new Vector3(sliderRest.x, sliderRest.y, Mathf.Lerp(sliderReleasedZ, sliderDrawnZ, draw));

            Vector3 claw = slider.localPosition;
            Stretch(stringLeft, bowArmLeft.localPosition + bowArmLeft.localRotation * (Vector3.left * bowArmLength), claw);
            Stretch(stringRight, bowArmRight.localPosition + bowArmRight.localRotation * (Vector3.right * bowArmLength), claw);

            if (winchRope != null)
                Stretch(winchRope, ropeAnchor, claw + Vector3.back * 0.3f);
            if (winch != null)
                winch.localRotation = Quaternion.Euler(-draw * winchTurns * 360f, 0f, 0f);
        }

        private static void Stretch(Transform piece, Vector3 from, Vector3 to)
        {
            Vector3 span = to - from;
            piece.localPosition = from;
            piece.localRotation = Quaternion.LookRotation(span);
            piece.localScale = new Vector3(1f, 1f, span.magnitude);
        }

        private void SetBoltLength(float length)
        {
            if (loadedBolt != null)
                loadedBolt.transform.localScale = new Vector3(boltSize.x, boltSize.y, boltSize.z * length);
        }

        private void PoseDwarf(float lean, float bob, float armsDown, float armsOut)
        {
            if (!hasDwarf)
                return;

            dwarf.localPosition = dwarfStand + Vector3.up * bob;
            dwarf.localScale = new Vector3(dwarfSize.x * (1f - bob), dwarfSize.y * (1f + bob), dwarfSize.z * (1f - bob));
            dwarf.localRotation = dwarfFacing * Quaternion.Euler(lean, 0f, 0f);
            dwarfArmLeft.localRotation = Quaternion.Euler(armsDown, 0f, -armsOut);
            dwarfArmRight.localRotation = Quaternion.Euler(armsDown, 0f, armsOut);
        }

        public override float DamageInSecond()
        {
            return boltPrefab.GetComponent<Projectile>().Damage * CountProjectileEntitys / timeBetweenShoots;
        }
    }

}
