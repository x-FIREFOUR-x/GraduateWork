using UnityEngine;
using UnityEngine.UI;

using TowerDefense.Main.Units.ForkPathChoice;
using TowerDefense.Main.Units.Squad;
using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Map.Buildings;
using TowerDefense.Storage;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;


namespace TowerDefense.Main.Units
{
    public abstract class Unit : MonoBehaviour
    {
        [SerializeField]
        protected Transform model;

        [field: SerializeField]
        public UnitType UnitType { get; private set; }


        [field: Header("Attributes")]
        [field: SerializeField]
        public float StartHealth { get; private set; } = 100f;
        private float health;

        [field: SerializeField]
        public float StartSpeed { get; private set; } = 10f;
        private float speed;

        [SerializeField]
        private float timeReturnSpeed = 1f;
        private float currentTimeReturnSpeed = 0;

        [field: SerializeField]
        public int Damage { get; private set; } = 10;

        [field: SerializeField]
        public int Price { get; private set; } = 25;


        [Header("Position")]
        [SerializeField]
        private float maxRandomPathOffset = 0.7f;
        private Vector3 pathOffset;
        [SerializeField]
        private float offsetStartPosY;
        public float TravelHeight { get; private set; }

        public PathGraphNode PreviousNode { get; private set; }
        public PathGraphNode TargetNode { get; private set; }
        private IForkPathChoiceStrategy forkPathChoiceStrategy;

        protected SquadRoutePlan SquadRoutePlan { get; set; }
        protected SquadMarker SquadMarker { get; set; }
        private bool hasLostSquadLeader;

        private float movedDistance = 0;
        protected bool IsSuccessor { get; private set; }
        private bool hasSuccessor;

        public bool CanBeTargeted { get; protected set; } = true;
        protected bool CanMove { get; set; } = true;

        public static string unitTag = "Unit";

        [Header("Effects")]
        [SerializeField]
        private GameObject deathEffect;
        private bool isDead;

        protected float SpeedFactor { get { return speed / StartSpeed; } }

        private Vector3 bodyCenter;
        private float groundHeight;

        protected abstract Transform Torso { get; }

        public Vector3 BodyCenterPoint { get { return Torso.TransformPoint(bodyCenter); } }
        public Vector3 GroundPoint { get { return new Vector3(transform.position.x, groundHeight, transform.position.z); } }

        public Vector3 AimPoint(bool atGround)
        {
            return atGround ? GroundPoint : BodyCenterPoint;
        }

        [Header("SetUpHealthBar")]
        [SerializeField]
        private Image healthBar;
        [field: SerializeField]
        public Transform FullHealthBar { get; private set; }
        [SerializeField]
        private float angleX;

        [Header("SquadMarker")]
        [SerializeField]
        protected float squadRingSizeFactor = 1f;

        protected virtual void Start()
        {
            forkPathChoiceStrategy = ForkPathChoiceStrategies.GetByUnitType(UnitType);
            health = StartHealth;
            speed = StartSpeed;

            if (!IsSuccessor)
            {
                PreviousNode = PathGraphHolder.Graph.Start;
                TargetNode = ChooseNextPathNode(PreviousNode);
                LeaveSquadIfRoutePlanCompleted();

                Vector2 randomPathOffset = Random.insideUnitCircle * maxRandomPathOffset;
                pathOffset = new Vector3(randomPathOffset.x, 0, randomPathOffset.y);
                TravelHeight = transform.position.y + pathOffset.y + offsetStartPosY;

                transform.SetPositionAndRotation(
                    new Vector3(transform.position.x + pathOffset.x, TravelHeight, transform.position.z + pathOffset.z),
                    transform.rotation);
            }

            bodyCenter = CalculateBodyCenter();
            groundHeight = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}").GroundHeight;
            InitializeHealthBar();
        }

        private Vector3 CalculateBodyCenter()
        {
            MeshFilter torsoMesh = Torso.GetComponent<MeshFilter>();
            return torsoMesh != null ? torsoMesh.sharedMesh.bounds.center : Vector3.zero;
        }

        void Update()
        {
            Animate();
        }

        void FixedUpdate()
        {
            if (!CanMove)
                return;

            Vector3 targetPosition = new Vector3(TargetNode.Position.x + pathOffset.x, 0, TargetNode.Position.z + pathOffset.z);
            Vector3 position = new Vector3(transform.position.x, 0, transform.position.z);

            Vector3 direction = targetPosition - position;
            transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

            movedDistance += (direction.normalized * speed * Time.deltaTime).magnitude;

            if (Mathf.Abs(Vector3.Distance(position, targetPosition)) <= 0.2f)
            {
                if (!TryGetNextPathNode())
                {
                    DamageEndBuilding();
                    Die();
                }

                Rotate();
            }

            UpdateSpeed();
        }

        private bool TryGetNextPathNode()
        {
            if (TargetNode.IsEnd)
            {
                return false;
            }
            else
            {
                PreviousNode = TargetNode;
                TargetNode = ChooseNextPathNode(PreviousNode);
                LeaveSquadIfRoutePlanCompleted();
                return true;
            }
        }

        private PathGraphNode ChooseNextPathNode(PathGraphNode currentNode)
        {
            if (!currentNode.IsFork)
                return currentNode.Next[0];

            if (SquadRoutePlan != null && SquadRoutePlan.TryGetNextBranchNodeAtFork(currentNode, out PathGraphNode nextBranchNodeFromSquadRoutePlan))
                return nextBranchNodeFromSquadRoutePlan;

            return ChooseNextBranchNodeAtForkByStrategy(currentNode);
        }

        protected virtual PathGraphNode ChooseNextBranchNodeAtForkByStrategy(PathGraphNode forkStartNode)
        {
            return forkPathChoiceStrategy.ChooseNextNodeAtFork(this, forkStartNode);
        }

        public void JoinSquad(SquadRoutePlan squadRoutePlan, SquadMarker squadLeaderMarker)
        {
            SquadRoutePlan = squadRoutePlan;
            SquadMarker = squadLeaderMarker.CreateForSquadMember(this, squadRingSizeFactor);
        }

        public void LoseSquadLeader()
        {
            hasLostSquadLeader = true;
            SquadMarker.ShowSquadLeaderLost();
            LeaveSquadIfRoutePlanCompleted();
        }

        private void LeaveSquadIfRoutePlanCompleted()
        {
            if (SquadRoutePlan == null || !hasLostSquadLeader || TargetNode == null || SquadRoutePlan.HasPlannedForkNodeAtOrAfter(TargetNode))
                return;

            SquadRoutePlan = null;
            Destroy(SquadMarker.gameObject);
            SquadMarker = null;
        }

        protected virtual void Animate()
        {
        }

        protected virtual void OnKilled()
        {
            if (deathEffect != null)
                Instantiate(deathEffect, model.position, model.rotation);
        }

        public void BecomeSuccessorOf(Unit previous)
        {
            PreviousNode = previous.PreviousNode;
            TargetNode = previous.TargetNode;
            pathOffset = previous.pathOffset;
            TravelHeight = previous.TravelHeight - previous.offsetStartPosY + offsetStartPosY;
            movedDistance = previous.movedDistance;
            model.rotation = previous.model.rotation;

            IsSuccessor = true;
            previous.hasSuccessor = true;
        }

        private void Die()
        {
            isDead = true;
            Destroy(gameObject);

            if (UnitsHeuristicsCalculator.instance != null && !hasSuccessor)
                UnitsHeuristicsCalculator.instance.AddDistanceMovedUnit(movedDistance);
        }

        private void DamageEndBuilding()
        {
            GameObject[] endBuilding = GameObject.FindGameObjectsWithTag(EndBuilding.endBuildingTag);

            if (endBuilding[0] != null)
            {
                endBuilding[0].GetComponent<EndBuilding>().TakeDamage(Damage);
            }


        }

        private void UpdateSpeed()
        {
            currentTimeReturnSpeed -= Time.deltaTime;

            if (currentTimeReturnSpeed <= 0f)
            {
                speed = StartSpeed;
            }
        }

        private void InitializeHealthBar()
        {
            Vector3 angles = transform.localEulerAngles;
            angles.x = angleX;
            angles.y = -angles.y;
            FullHealthBar.localEulerAngles = angles;
        }

        private void Rotate()
        {
            Vector3 dir = TargetNode.Position + pathOffset - model.position;
            dir.y = 0;
            dir.Normalize();
            Quaternion quaternion = Quaternion.LookRotation(dir, Vector3.up);
            model.rotation = quaternion;
        }


        public void TakeDamage(float damage)
        {
            health -= damage;

            healthBar.fillAmount = health / StartHealth;

            if (health <= 0 && !isDead)
            {
                OnKilled();
                Die();
            }
        }

        public void Slow(float percentSlowing)
        {
            speed = StartSpeed * (1 - percentSlowing);

            currentTimeReturnSpeed = timeReturnSpeed;
        }

        public virtual void UpgradeHealth(float upByPercentage)
        {
            StartHealth += StartHealth * upByPercentage;
        }
    }

}
