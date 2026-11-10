using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace TowerDefense.Main.Units.Squad
{
    public class SquadMarker : MonoBehaviour
    {
        private static readonly List<float> squadHuesInUse = new();

        [Header("Color")]
        [SerializeField]
        private int squadHueCandidatesCount = 16;
        [SerializeField, Range(0f, 1f)]
        private float excludedSquadHueRangeStart = 0.03f;
        [SerializeField, Range(0f, 1f)]
        private float excludedSquadHueRangeEnd = 0.14f;
        [SerializeField, Range(0f, 1f)]
        private float squadColorSaturation = 0.85f;
        [SerializeField, Range(0f, 1f)]
        private float squadColorValue = 1f;
        [SerializeField, Range(0f, 1f)]
        private float dimFactorAfterSquadLeaderLost = 0.35f;

        [Header("Emblem")]
        [SerializeField]
        private float emblemSize = 32f;
        [SerializeField]
        private float emblemGapToHealthBar = 4f;

        [Header("Ring")]
        [SerializeField]
        private Sprite ringSprite;
        [SerializeField, Range(0f, 1f)]
        private float ringEdgeRadiusPart = 0.74f;
        [SerializeField]
        private float ringDiameterToFootprint = 1.05f;
        [SerializeField]
        private float squadLeaderRingDiameterFactor = 1.15f;
        [SerializeField]
        private float ringHeightAboveGround = 0.03f;
        [SerializeField, Range(0f, 1f)]
        private float ringAlpha = 0.55f;
        [SerializeField]
        private float pulseSpeed = 2.5f;
        [SerializeField, Range(0f, 1f)]
        private float pulseAmplitude = 0.2f;

        [Header("Sparks")]
        [SerializeField]
        private Material sparksMaterial;
        [SerializeField]
        private float squadLeaderSparksPerRingLengthPerSecond = 9f;
        [SerializeField]
        private float squadMemberSparksPerRingLengthPerSecond = 5f;
        [SerializeField]
        private int maxSparksCount = 300;
        [SerializeField]
        private float sparkLifetime = 1.3f;
        [SerializeField]
        private float sparkMinSizeToRingDiameter = 0.09f;
        [SerializeField]
        private float sparkMaxSizeToRingDiameter = 0.15f;
        [SerializeField]
        private float sparkStartHeightToRingDiameter = 0.25f;
        [SerializeField]
        private float sparkStartHeightSpreadToRingDiameter = 0.15f;
        [SerializeField]
        private float sparkMinRiseSpeedToRingDiameter = 0.35f;
        [SerializeField]
        private float sparkMaxRiseSpeedToRingDiameter = 0.6f;

        private SquadMarker squadMarkerPrefab;
        private Sprite squadEmblem;
        private Unit unit;
        private SpriteRenderer ring;
        private ParticleSystem sparks;
        private Image emblem;
        private float squadHue;
        private Color squadColor;
        private bool isSquadHueReserved;
        private float dimFactor = 1f;


        public static SquadMarker CreateForSquadLeader(SquadMarker squadMarkerPrefab, Unit squadLeader, Sprite squadEmblem, float squadRingSizeFactor)
        {
            float squadHue = squadMarkerPrefab.ChooseDistinctSquadHue();
            return Create(squadMarkerPrefab, squadLeader, squadEmblem, squadHue, squadRingSizeFactor, true);
        }

        public SquadMarker CreateForSquadMember(Unit squadMember, float squadRingSizeFactor)
        {
            return Create(squadMarkerPrefab, squadMember, squadEmblem, squadHue, squadRingSizeFactor, false);
        }

        private static SquadMarker Create(SquadMarker squadMarkerPrefab, Unit unit, Sprite squadEmblem, float squadHue, float squadRingSizeFactor, bool isSquadLeader)
        {
            SquadMarker squadMarker = Instantiate(squadMarkerPrefab, unit.transform, false);
            squadMarker.name = nameof(SquadMarker);
            squadMarker.Initialize(squadMarkerPrefab, unit, squadEmblem, squadHue, squadRingSizeFactor, isSquadLeader);
            return squadMarker;
        }

        private void Initialize(SquadMarker squadMarkerPrefab, Unit unit, Sprite squadEmblem, float squadHue, float squadRingSizeFactor, bool isSquadLeader)
        {
            this.squadMarkerPrefab = squadMarkerPrefab;
            this.squadEmblem = squadEmblem;
            this.unit = unit;
            this.squadHue = squadHue;
            squadColor = Color.HSVToRGB(squadHue, squadColorSaturation, squadColorValue);
            squadHuesInUse.Add(squadHue);
            isSquadHueReserved = true;

            float ringDiameter = MeasureFootprintDiameter(unit) * ringDiameterToFootprint * squadRingSizeFactor * (isSquadLeader ? squadLeaderRingDiameterFactor : 1f);
            float sparksPerRingLengthPerSecond = isSquadLeader ? squadLeaderSparksPerRingLengthPerSecond : squadMemberSparksPerRingLengthPerSecond;

            ring = CreateRing(ringDiameter);
            sparks = CreateSparks(ringDiameter, sparksPerRingLengthPerSecond);
            emblem = CreateEmblem(unit.FullHealthBar);
            UpdateGroundPosition();
        }

        private float ChooseDistinctSquadHue()
        {
            float bestHue = RandomAllowedSquadHue();
            float bestDistance = DistanceToNearestSquadHueInUse(bestHue);

            for (int candidate = 1; candidate < squadHueCandidatesCount; candidate++)
            {
                float hue = RandomAllowedSquadHue();
                float distance = DistanceToNearestSquadHueInUse(hue);
                if (distance > bestDistance)
                {
                    bestHue = hue;
                    bestDistance = distance;
                }
            }

            return bestHue;
        }

        private float RandomAllowedSquadHue()
        {
            float excludedSquadHueRangeLength = Mathf.Repeat(excludedSquadHueRangeEnd - excludedSquadHueRangeStart, 1f);
            return Mathf.Repeat(excludedSquadHueRangeEnd + Random.value * (1f - excludedSquadHueRangeLength), 1f);
        }

        private static float DistanceToNearestSquadHueInUse(float hue)
        {
            float nearestDistance = 1f;
            foreach (float hueInUse in squadHuesInUse)
            {
                float distance = Mathf.Abs(hue - hueInUse);
                nearestDistance = Mathf.Min(nearestDistance, Mathf.Min(distance, 1f - distance));
            }
            return nearestDistance;
        }

        private static float MeasureFootprintDiameter(Unit unit)
        {
            Transform unitTransform = unit.transform;
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;

            foreach (Renderer renderer in unit.GetComponentsInChildren<Renderer>())
            {
                if (renderer is ParticleSystemRenderer)
                    continue;

                Bounds localBounds = renderer.localBounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 cornerPoint = localBounds.center + Vector3.Scale(localBounds.extents, new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f));
                    Vector3 unitPoint = unitTransform.InverseTransformPoint(renderer.transform.TransformPoint(cornerPoint));
                    min = Vector2.Min(min, new Vector2(unitPoint.x, unitPoint.z));
                    max = Vector2.Max(max, new Vector2(unitPoint.x, unitPoint.z));
                }
            }

            Vector2 size = max - min;
            return Mathf.Sqrt(size.x * size.y);
        }

        private SpriteRenderer CreateRing(float diameter)
        {
            GameObject ringObject = new("SquadRing");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            float scale = diameter / ringSprite.bounds.size.x;
            ringObject.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer ringRenderer = ringObject.AddComponent<SpriteRenderer>();
            ringRenderer.sprite = ringSprite;
            return ringRenderer;
        }

        private ParticleSystem CreateSparks(float ringDiameter, float sparksPerRingLengthPerSecond)
        {
            GameObject sparksObject = new("SquadSparks");
            sparksObject.transform.SetParent(transform, false);
            sparksObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem sparksSystem = sparksObject.AddComponent<ParticleSystem>();
            sparksSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = sparksSystem.main;
            main.loop = true;
            main.startLifetime = sparkLifetime;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(sparkMinSizeToRingDiameter * ringDiameter, sparkMaxSizeToRingDiameter * ringDiameter);
            main.startColor = squadColor;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = maxSparksCount;

            ParticleSystem.EmissionModule emission = sparksSystem.emission;
            float ringRadius = ringDiameter * 0.5f * ringEdgeRadiusPart;
            emission.rateOverTime = sparksPerRingLengthPerSecond * 2f * Mathf.PI * ringRadius;

            ParticleSystem.ShapeModule shape = sparksSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = ringRadius;
            shape.radiusThickness = 0f;
            shape.position = new Vector3(0f, 0f, sparkStartHeightToRingDiameter * ringDiameter);
            shape.randomPositionAmount = sparkStartHeightSpreadToRingDiameter * ringDiameter;

            ParticleSystem.VelocityOverLifetimeModule velocity = sparksSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.z = new ParticleSystem.MinMaxCurve(sparkMinRiseSpeedToRingDiameter * ringDiameter, sparkMaxRiseSpeedToRingDiameter * ringDiameter);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = sparksSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = sparksSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.4f));

            ParticleSystemRenderer sparksRenderer = sparksObject.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.sharedMaterial = sparksMaterial;

            sparksSystem.Play();
            return sparksSystem;
        }

        private Image CreateEmblem(Transform fullHealthBar)
        {
            GameObject emblemObject = new("SquadEmblem", typeof(RectTransform));
            RectTransform emblemTransform = (RectTransform)emblemObject.transform;
            emblemTransform.SetParent(fullHealthBar, false);
            emblemTransform.anchorMin = new Vector2(0f, 0.5f);
            emblemTransform.anchorMax = new Vector2(0f, 0.5f);
            emblemTransform.pivot = new Vector2(1f, 0.5f);
            emblemTransform.anchoredPosition = new Vector2(-emblemGapToHealthBar, 0f);
            emblemTransform.sizeDelta = new Vector2(emblemSize, emblemSize);

            Image emblemImage = emblemObject.AddComponent<Image>();
            emblemImage.sprite = squadEmblem;
            emblemImage.preserveAspect = true;
            emblemImage.raycastTarget = false;
            return emblemImage;
        }

        void LateUpdate()
        {
            UpdateGroundPosition();

            float pulse = 1f - pulseAmplitude * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
            Color color = squadColor;
            color.a *= ringAlpha * dimFactor * pulse;
            ring.color = color;
        }

        private void UpdateGroundPosition()
        {
            Vector3 groundPosition = unit.GroundPoint + Vector3.up * ringHeightAboveGround;
            ring.transform.position = groundPosition;
            sparks.transform.position = groundPosition;
        }

        public void ShowSquadLeaderLost()
        {
            dimFactor = dimFactorAfterSquadLeaderLost;

            ParticleSystem.MainModule main = sparks.main;
            Color sparkColor = squadColor;
            sparkColor.a *= dimFactorAfterSquadLeaderLost;
            main.startColor = sparkColor;

            ParticleSystem.EmissionModule emission = sparks.emission;
            emission.rateMultiplier *= dimFactorAfterSquadLeaderLost;
        }

        void OnDestroy()
        {
            if (isSquadHueReserved)
                squadHuesInUse.Remove(squadHue);

            if (emblem != null)
                Destroy(emblem.gameObject);
        }
    }

}
