using System.Collections;

using UnityEngine;

using TowerDefense.Main.Projectiles;


namespace TowerDefense.Main.Towers
{
    public class CatapultTower : Tower
    {
        [Header("Catapult Parts")]
        [SerializeField]
        private Transform arm;
        [SerializeField]
        private GameObject loadedStone;
        [SerializeField]
        private ParticleSystem launchDust;

        [Header("Swing")]
        [SerializeField]
        private float armRestAngle = 45f;
        [SerializeField]
        private float armFireAngle = 82f;
        [SerializeField]
        private float swingTime = 0.22f;
        [SerializeField]
        private float reloadTime = 1.1f;

        [Header("Gnome")]
        [SerializeField]
        private Transform gnome;
        [SerializeField]
        private Transform gnomeArm;
        [SerializeField]
        private float gnomeJumpHeight = 0.45f;
        private bool hasGnome;
        private Vector3 gnomeStand;
        private Vector3 gnomeSize;
        private Quaternion gnomeFacing;

        private const float wrenchWoundBackAngle = -140f;
        private const float wrenchStruckAngle = 35f;
        private const float wrenchRaisedAngle = -120f;
        private const float percentageOfReloadTimeForGnomeWindUp = 0.35f;
        private const float percentageOfReloadTimeForGnomeJump = 0.5f;


        void Awake()
        {
            hasGnome = gnome != null && gnomeArm != null;
            if (hasGnome)
            {
                gnomeStand = gnome.localPosition;
                gnomeSize = gnome.localScale;
                gnomeFacing = gnome.localRotation;
            }
        }

        void Update()
        {
            AimAndShoot();

            if (!isReloading)
                PoseGnome(0f, Mathf.Sin(Time.time * 3f) * 0.03f, 0f, 0f);
        }

        protected override IEnumerator Shoot()
        {
            Vector3 aimPoint = target.position + offsetTarget;
            float swingStarted = Time.time;

            //Wind-up
            for (float t = 0f; t < 1f; t += Time.deltaTime / swingTime)
            {
                SetArmAngle(Mathf.Lerp(armRestAngle, armFireAngle, t * t));

                if (t < percentageOfReloadTimeForGnomeWindUp)
                {
                    float windUp = t / percentageOfReloadTimeForGnomeWindUp;
                    PoseGnome(0f, 0.12f * windUp, 0f, Mathf.Lerp(0f, wrenchWoundBackAngle, Mathf.SmoothStep(0f, 1f, windUp)));
                }
                else
                {
                    float strike = (t - percentageOfReloadTimeForGnomeWindUp) / (1f - percentageOfReloadTimeForGnomeWindUp);
                    float rise = Mathf.Sin(strike * Mathf.PI);
                    PoseGnome(rise * gnomeJumpHeight * 0.5f, -0.1f * rise, 0f, Mathf.Lerp(wrenchWoundBackAngle, wrenchStruckAngle, strike * strike));
                }
                yield return null;
            }
            SetArmAngle(armFireAngle);

            Launch(aimPoint, Time.time - swingStarted);

            //Reload
            for (float t = 0f; t < 1f; t += Time.deltaTime / reloadTime)
            {
                SetArmAngle(Mathf.Lerp(armFireAngle, armRestAngle, Mathf.SmoothStep(0f, 1f, t)));

                if (t < percentageOfReloadTimeForGnomeJump)
                {
                    float jump = t / percentageOfReloadTimeForGnomeJump;
                    float rise = Mathf.Sin(jump * Mathf.PI);
                    PoseGnome(rise * gnomeJumpHeight, -0.12f * rise, 360f * Mathf.SmoothStep(0f, 1f, jump),
                              Mathf.Lerp(wrenchStruckAngle, wrenchRaisedAngle, Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, jump * 2f))));
                }
                else
                {
                    float settle = (t - percentageOfReloadTimeForGnomeJump) / (1f - percentageOfReloadTimeForGnomeJump);
                    PoseGnome(0f, 0.18f * (1f - settle) * (1f - settle), 0f, Mathf.Lerp(wrenchRaisedAngle, 0f, Mathf.SmoothStep(0f, 1f, settle)));
                }
                yield return null;
            }
            SetArmAngle(armRestAngle);
            PoseGnome(0f, 0f, 0f, 0f);

            loadedStone.SetActive(true);
        }

        // The stone leaves from the arm, not from a fire point, and is thrown at the point aimed at on the wind-up
        private void Launch(Vector3 aimPoint, float timeSpent)
        {
            loadedStone.SetActive(false);

            if (launchDust != null)
                launchDust.Play();

            GameObject stoneObject = Instantiate(projectilePrefab, loadedStone.transform.position, loadedStone.transform.rotation);
            stoneObject.GetComponent<CatapultStone>().Launch(aimPoint, timeSpent);
        }

        private void SetArmAngle(float angle)
        {
            arm.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private void PoseGnome(float height, float squash, float spin, float wrenchAngle)
        {
            if (!hasGnome)
                return;

            gnome.localPosition = gnomeStand + Vector3.up * height;
            gnome.localScale = new Vector3(gnomeSize.x * (1f + squash * 0.5f), gnomeSize.y * (1f - squash), gnomeSize.z * (1f + squash * 0.5f));
            gnome.localRotation = gnomeFacing * Quaternion.Euler(0f, spin, 0f);
            gnomeArm.localRotation = Quaternion.Euler(wrenchAngle, 0f, 0f);
        }
    }

}
