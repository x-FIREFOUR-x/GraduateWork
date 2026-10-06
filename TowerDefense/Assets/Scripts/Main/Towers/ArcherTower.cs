using System.Collections;

using UnityEngine;


namespace TowerDefense.Main.Towers
{
    public class ArcherTower : Tower
    {
        [Header("Archers")]
        [SerializeField]
        private Archer leftArcher;
        [SerializeField]
        private Archer rightArcher;

        [Header("Bow Setup")]
        // In an archer's own space: the bow tips
        [SerializeField]
        private Vector3 bowTopTip;
        [SerializeField]
        private Vector3 bowBottomTip;
        [SerializeField]
        private float arrowLength = 0.42f;
        // Where the arrow rests on the bow; it lies from the nock through here
        [SerializeField]
        private Vector3 arrowRest;
        // The drawing arm, in an archer's own space: where it turns at the shoulder, the lengths of its two bones, the
        // angles round the shoulder the elbow swings between, level with it, and where the nock is from the wrist,
        // along the forearm and up
        [SerializeField]
        private Vector3 drawShoulder;
        [SerializeField]
        private float upperArmLength = 0.15f;
        [SerializeField]
        private float forearmLength = 0.15f;
        [SerializeField]
        private float elbowRestAngle = -15f;
        [SerializeField]
        private float elbowDrawnAngle = 130f;
        [SerializeField]
        private Vector3 nockFromWrist = new(0f, 0.04f, 0.05f);

        [Header("Captain")]
        [SerializeField]
        private Transform captain;
        [SerializeField]
        private Transform commandArm;
        // How far the sword arm is raised round the shoulder while the archers draw; at 0 the sword points at the target
        [SerializeField]
        private float commandRaisedAngle = -100f;

        [Header("Walking")]
        // While the tower turns, the elves on its deck step round with it instead of gliding: their legs swing in the
        // way they are carried, the knee of the leg coming through bending, the body rising a little at each step
        [SerializeField]
        private Legs leftArcherLegs;
        [SerializeField]
        private Legs rightArcherLegs;
        [SerializeField]
        private Legs captainLegs;
        // How far one step carries them, how far the legs swing, how far the knee bends, how high they rise, in
        // degrees and world units; the turning speed at which they walk fully, in degrees a second
        [SerializeField]
        private float stepLength = 0.42f;
        [SerializeField]
        private float legSwing = 22f;
        [SerializeField]
        private float kneeBend = 40f;
        [SerializeField]
        private float walkBob = 0.035f;
        [SerializeField]
        private float fullWalkTurnSpeed = 40f;

        [Header("Volley")]
        [SerializeField]
        private float releaseTime = 0.08f;
        // Parts of the time between shots spent nocking new arrows and drawing them; together they stay under the whole
        // of it, so the archers are always drawn again before the next volley
        [SerializeField]
        private float nockShare = 0.15f;
        [SerializeField]
        private float drawShare = 0.4f;

        [System.Serializable]
        private class Legs
        {
            public Transform thighLeft;
            public Transform shinLeft;
            public Transform thighRight;
            public Transform shinRight;
        }

        // An elf on the deck as he walks: his body, his legs, where he stands at rest, how far through his step he is
        private class Walker
        {
            public Transform body;
            public Legs legs;
            public Vector3 rest;
            public float phase;
        }

        private Walker[] walkers = new Walker[0];
        private float lastYaw;
        // 0 standing, 1 walking fully, easing between as the turning starts and stops
        private float walking;

        [System.Serializable]
        private class Archer
        {
            public Transform body;
            public Transform upperArm;
            public Transform forearm;
            public Transform stringTop;
            public Transform stringBottom;
            public GameObject loadedArrow;

            [System.NonSerialized]
            public Vector3 size;
        }

        private bool hasArchers;
        private bool hasCaptain;
        private Vector3 captainSize;


        void Awake()
        {
            hasArchers = IsComplete(leftArcher) && IsComplete(rightArcher);
            if (hasArchers)
            {
                foreach (Archer archer in new[] { leftArcher, rightArcher })
                    archer.size = archer.body.localScale;
            }

            hasCaptain = captain != null && commandArm != null;
            if (hasCaptain)
                captainSize = captain.localScale;

            SetDraw(1f);
            SetCommand(1f);

            System.Collections.Generic.List<Walker> found = new();
            foreach ((Transform body, Legs legs) in new[] { (leftArcher?.body, leftArcherLegs), (rightArcher?.body, rightArcherLegs), (captain, captainLegs) })
            {
                if (body != null && legs != null && legs.thighLeft != null && legs.thighRight != null && legs.shinLeft != null && legs.shinRight != null)
                    found.Add(new Walker { body = body, legs = legs, rest = body.localPosition });
            }
            walkers = found.ToArray();
            if (rotatePart != null)
                lastYaw = rotatePart.eulerAngles.y;
        }

        void Update()
        {
            AimAndShoot();

            Walk();
            Breathe();
        }

        protected override IEnumerator Shoot()
        {
            // The signal and the loose: the sword comes down, both arrows leave, the strings snap forward
            Fire();
            SetArrowsShown(false);
            for (float t = 0f; t < 1f; t += Time.deltaTime / releaseTime)
            {
                SetDraw(1f - t);
                SetCommand(1f - Mathf.Sqrt(t));
                yield return null;
            }
            SetDraw(0f);
            SetCommand(0f);

            // Nocking: a new arrow from the quiver laid on each bow
            float nockTime = timeBetweenShoots * nockShare;
            yield return new WaitForSeconds(nockTime * 0.5f);
            SetArrowsShown(true);
            yield return new WaitForSeconds(nockTime * 0.5f);

            // Drawing, while the captain raises his sword for the next volley
            float drawTime = timeBetweenShoots * drawShare;
            for (float t = 0f; t < 1f; t += Time.deltaTime / drawTime)
            {
                float eased = Mathf.SmoothStep(0f, 1f, t);
                SetDraw(eased);
                SetCommand(eased);
                yield return null;
            }
            SetDraw(1f);
            SetCommand(1f);
        }

        private void SetDraw(float draw)
        {
            if (!hasArchers)
                return;

            DrawPose(drawShoulder, upperArmLength, forearmLength, Mathf.Lerp(elbowRestAngle, elbowDrawnAngle, draw), arrowRest, nockFromWrist,
                     out Vector3 elbow, out Vector3 aim, out Vector3 nock);
            foreach (Archer archer in new[] { leftArcher, rightArcher })
            {
                archer.upperArm.localPosition = drawShoulder;
                archer.upperArm.localRotation = Quaternion.LookRotation(elbow - drawShoulder, Vector3.up);
                archer.forearm.localPosition = elbow;
                archer.forearm.localRotation = Quaternion.LookRotation(aim, Vector3.up);
                // The arrow's origin is its tip, an arrow's length on from the nock past the rest
                Vector3 direction = (arrowRest - nock + Vector3.forward * 0.001f).normalized;
                archer.loadedArrow.transform.localPosition = nock + direction * arrowLength;
                archer.loadedArrow.transform.localRotation = Quaternion.LookRotation(direction);
                Stretch(archer.stringTop, bowTopTip, nock);
                Stretch(archer.stringBottom, bowBottomTip, nock);
            }
        }

        private void SetCommand(float command)
        {
            if (hasCaptain)
                commandArm.localRotation = Quaternion.Euler(commandRaisedAngle * command, 0f, 0f);
        }

        private void SetArrowsShown(bool shown)
        {
            if (!hasArchers)
                return;

            leftArcher.loadedArrow.SetActive(shown);
            rightArcher.loadedArrow.SetActive(shown);
        }

        private void Walk()
        {
            if (rotatePart == null || walkers.Length == 0 || Time.deltaTime <= 0f)
                return;

            float yaw = rotatePart.eulerAngles.y;
            float turnSpeed = Mathf.DeltaAngle(lastYaw, yaw) / Time.deltaTime;
            lastYaw = yaw;
            walking = Mathf.MoveTowards(walking, Mathf.Clamp01(Mathf.Abs(turnSpeed) / fullWalkTurnSpeed), Time.deltaTime * 3f);

            foreach (Walker walker in walkers)
            {
                Vector3 fromMiddle = walker.body.position - rotatePart.position;
                fromMiddle.y = 0f;
                float speed = Mathf.Abs(turnSpeed) * Mathf.Deg2Rad * fromMiddle.magnitude;
                walker.phase += speed / stepLength * Mathf.PI * Time.deltaTime;

                // The way he is carried, in his own space: forwards or back, and to the side
                Vector3 heading = Vector3.Cross(Vector3.up, fromMiddle).normalized * Mathf.Sign(turnSpeed);
                Vector3 local = walker.body.InverseTransformDirection(heading);
                local.y = 0f;
                local = local.sqrMagnitude < 0.0001f ? Vector3.forward : local.normalized;
                float sideways = Mathf.Abs(local.x);

                // Forwards or back the legs swing opposite ways. To the side each leg only ever steps out to its own side
                // and comes back, the two by turns, so they never cross
                float sine = Mathf.Sin(walker.phase);
                float cosine = Mathf.Cos(walker.phase);
                float stride = sine * legSwing * walking * local.z;
                float outLeft = legSwing * 0.55f * walking * sideways * Mathf.Max(0f, sine);
                float outRight = legSwing * 0.55f * walking * sideways * Mathf.Max(0f, -sine);
                walker.legs.thighLeft.localRotation = Quaternion.Euler(stride, 0f, -outLeft);
                walker.legs.thighRight.localRotation = Quaternion.Euler(-stride, 0f, outRight);

                // The knee bends as its leg is lifted and carried through, straight again as the foot comes down
                float liftLeft = Mathf.Max(0f, Mathf.Lerp(cosine, sine, sideways));
                float liftRight = Mathf.Max(0f, Mathf.Lerp(-cosine, -sine, sideways));
                walker.legs.shinLeft.localRotation = Quaternion.Euler(kneeBend * walking * liftLeft, 0f, 0f);
                walker.legs.shinRight.localRotation = Quaternion.Euler(kneeBend * walking * liftRight, 0f, 0f);

                // The body rises a little as each leg passes under it
                walker.body.localPosition = walker.rest + Vector3.up * (Mathf.Abs(sine) * walkBob * walking);
            }
        }

        private void Breathe()
        {
            float time = Time.time;
            if (hasArchers)
            {
                Swell(leftArcher.body, leftArcher.size, Mathf.Sin(time * 2.1f) * 0.012f);
                Swell(rightArcher.body, rightArcher.size, Mathf.Sin(time * 2.1f + 1.7f) * 0.012f);
            }
            if (hasCaptain)
                Swell(captain, captainSize, Mathf.Sin(time * 1.8f + 0.6f) * 0.012f);
        }

        private static void Swell(Transform body, Vector3 size, float breath)
        {
            body.localScale = new Vector3(size.x * (1f - breath * 0.5f), size.y * (1f + breath), size.z * (1f - breath * 0.5f));
        }

        public static void DrawPose(Vector3 shoulder, float upperLength, float lowerLength, float elbowAngle, Vector3 arrowRest, Vector3 nockFromWrist,
                                    out Vector3 elbow, out Vector3 aim, out Vector3 nock)
        {
            elbow = shoulder + Quaternion.Euler(0f, elbowAngle, 0f) * Vector3.forward * upperLength;
            aim = arrowRest - elbow;
            aim.y = 0f;
            aim.Normalize();

            Vector3 wrist = elbow + aim * lowerLength;
            nock = wrist + aim * nockFromWrist.z + Vector3.up * nockFromWrist.y;
        }

        private static bool IsComplete(Archer archer)
        {
            return archer != null && archer.body != null && archer.upperArm != null && archer.forearm != null && archer.stringTop != null && archer.stringBottom != null
                && archer.loadedArrow != null;
        }
    }

}
