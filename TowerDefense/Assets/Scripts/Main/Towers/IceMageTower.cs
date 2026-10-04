using UnityEngine;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Towers
{
    public class IceMageTower : Tower
    {
        [Header("Attributes")]
        [SerializeField]
        private float damage = 10f;
        [SerializeField]
        private float percentSlowing = 0.5f;

        [Header("Effects")]
        [SerializeField]
        private LineRenderer frostBeam;
        [SerializeField]
        private ParticleSystem hitEffect;
        [SerializeField]
        private Light hitLight;
        [SerializeField]
        private Light crystalLight;

        [Header("Mage")]
        [SerializeField]
        private Transform mage;
        [SerializeField]
        private Transform staffArm;
        [SerializeField]
        private Transform staffCrystal;
        [SerializeField]
        private Transform shards;
        [SerializeField]
        private float staffRaisedAngle = -55f;
        [SerializeField]
        private float castBlendSpeed = 4f;
        [SerializeField]
        private float shardsTurnSpeed = 35f;

        private float cast;

        private bool hasMage;
        private Vector3 mageSize;
        private Quaternion mageFacing;
        private Quaternion staffRest;
        private Vector3 crystalRest;
        private Vector3 crystalSize;
        private float crystalLightIntensity;


        void Awake()
        {
            hasMage = mage != null && staffArm != null && staffCrystal != null;
            if (hasMage)
            {
                mageSize = mage.localScale;
                mageFacing = mage.localRotation;
                staffRest = staffArm.localRotation;
                crystalRest = staffCrystal.localPosition;
                crystalSize = staffCrystal.localScale;
            }

            if (crystalLight != null)
                crystalLightIntensity = crystalLight.intensity;
        }

        void Update()
        {
            if (target != null)
            {
                RotateToTarget();
                ActivateBeam();

                Vector3 direction = pointStartFire.position - (target.position + offsetTarget);
                hitEffect.transform.position = (target.position + offsetTarget) + direction.normalized;
                hitEffect.transform.rotation = Quaternion.LookRotation(direction);

                target.GetComponent<Enemy>().TakeDamage(damage * Time.deltaTime);

                target.GetComponent<Enemy>().Slow(percentSlowing);
            }
            else
            {
                DeactivateBeam();
            }

            Animate();
        }

        private void ActivateBeam()
        {
            if (!frostBeam.enabled)
            {
                frostBeam.enabled = true;
                hitEffect.Play();
                hitLight.enabled = true;
            }

            frostBeam.SetPosition(0, pointStartFire.position);
            frostBeam.SetPosition(1, target.position + offsetTarget);
            frostBeam.widthMultiplier = 1f + 0.2f * Mathf.Sin(Time.time * 23f) + 0.1f * Mathf.Sin(Time.time * 41f);
        }

        private void DeactivateBeam()
        {
            if (frostBeam.enabled)
            {
                frostBeam.enabled = false;
                hitEffect.Stop();
                hitLight.enabled = false;
            }
        }

        private void Animate()
        {
            cast = Mathf.MoveTowards(cast, target != null ? 1f : 0f, Time.deltaTime * castBlendSpeed);
            float casting = Mathf.SmoothStep(0f, 1f, cast);
            float time = Time.time;

            if (hasMage)
            {
                // Breathing while waiting, leaning a little into the cast
                float breath = Mathf.Sin(time * 2.2f) * 0.02f * (1f - casting);
                mage.localScale = new Vector3(mageSize.x * (1f - breath * 0.5f), mageSize.y * (1f + breath), mageSize.z * (1f - breath * 0.5f));
                mage.localRotation = mageFacing * Quaternion.Euler(6f * casting, 0f, 0f);

                staffArm.localRotation = staffRest * Quaternion.Euler(staffRaisedAngle * casting, 0f, 0f);

                // The crystal floats above the staff and turns, faster and pulsing while he casts
                staffCrystal.localPosition = crystalRest + Vector3.up * (Mathf.Sin(time * 2f) * 0.025f);
                staffCrystal.Rotate(Vector3.up, (60f + 420f * casting) * Time.deltaTime, Space.Self);
                staffCrystal.localScale = crystalSize * (1f + casting * 0.15f * Mathf.Sin(time * 12f));
            }

            if (crystalLight != null)
                crystalLight.intensity = crystalLightIntensity * (1f + casting * (1.5f + 0.5f * Mathf.Sin(time * 15f)));

            if (shards != null)
                shards.Rotate(Vector3.up, (shardsTurnSpeed + 90f * casting) * Time.deltaTime, Space.Self);
        }

        public override float DamageInSecond()
        {
            return damage;
        }
    }

}
