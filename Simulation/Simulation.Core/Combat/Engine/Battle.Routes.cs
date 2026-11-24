using System;
using System.Collections.Generic;
using Simulation.Combat.Paths;
using Simulation.Combat.Towers;
using Simulation.Combat.Units;
using Simulation.Geometry;
using Simulation.InputData.Game.Units;


namespace Simulation.Combat.Engine
{
    internal sealed partial class Battle
    {
        private const float TieTolerance = 0.001f;
        private const float MinSegmentLength = 0.0001f;

        private const int ForkStrategyCount = 8;
        private const int MaxUnitTypes = 1024;

        private readonly List<SimUnit> recruitingLeaders = new List<SimUnit>();
        private readonly Dictionary<long, PathNode> staticBranchChoices = new Dictionary<long, PathNode>();
        private int[] unitCountByEdge;
        private float[] remainingMetricByNode;
        private int[] remainingMetricStamp;
        private int branchSearchStamp;

        partial void OnSpawned(SimUnit unit)
        {
            if (unit.IsLeader)
                unit.RoutePlan = new SquadRoutePlan(Graph);

            recruitingLeaders.RemoveAll(leader => !leader.CanRecruit);
            foreach (SimUnit leader in recruitingLeaders)
            {
                if (!leader.Spec.SquadMemberTypes.Contains(unit.Spec.Type))
                    continue;

                unit.RoutePlan = leader.RoutePlan;
                leader.SquadMembers.Add(unit);
                break;
            }

            if (unit.IsLeader)
                recruitingLeaders.Add(unit);
        }

        partial void OnAdvanced(SimUnit unit) => LeaveSquadIfRouteCompleted(unit);

        partial void OnRemoved(SimUnit unit)
        {
            if (!unit.IsLeader)
                return;

            foreach (SimUnit member in unit.SquadMembers)
            {
                if (!member.IsActive)
                    continue;
                member.HasLostLeader = true;
                LeaveSquadIfRouteCompleted(member);
            }
        }

        partial void ChooseBranch(SimUnit unit, PathNode fork, ref PathNode next)
        {
            if (unit.RoutePlan != null && unit.RoutePlan.TryGetNext(fork, out PathNode planned))
            {
                next = planned;
                return;
            }

            next = ChooseByStrategy(unit, fork);
            if (unit.IsLeader && unit.RoutePlan != null)
                unit.RoutePlan.Record(fork, next);
        }

        private static void LeaveSquadIfRouteCompleted(SimUnit unit)
        {
            if (unit.RoutePlan == null || !unit.HasLostLeader || unit.Target == null || unit.RoutePlan.HasPlannedForkAtOrAfter(unit.Target))
                return;
            unit.RoutePlan = null;
        }

        private PathNode ChooseByStrategy(SimUnit unit, PathNode fork)
        {
            ForkStrategyKind strategy = unit.Spec.ForkStrategy;
            switch (strategy)
            {
                case ForkStrategyKind.Shortest:
                case ForkStrategyKind.Longest:
                    return CachedChoice(fork, strategy, 0, () => ChooseBestBranch(fork, SegmentLength, strategy == ForkStrategyKind.Shortest));
                case ForkStrategyKind.Safest:
                case ForkStrategyKind.MostDangerous:
                    return CachedChoice(fork, strategy, unit.TypeIndex, () => ChooseBestBranch(fork, TowerDamageMetric(unit), strategy == ForkStrategyKind.Safest));
                case ForkStrategyKind.LeastCrowded:
                    return ChooseBestBranch(fork, UnitCountMetric(unit), true);
                case ForkStrategyKind.MostCrowded:
                    return ChooseBestBranch(fork, UnitCountMetric(unit), false);
                default:
                    return fork.Next[random.Next(fork.Next.Count)];
            }
        }

        private PathNode CachedChoice(PathNode fork, ForkStrategyKind strategy, int typeIndex, Func<PathNode> choose)
        {
            long key = ((long)fork.Index * ForkStrategyCount + (int)strategy) * MaxUnitTypes + typeIndex;
            if (!staticBranchChoices.TryGetValue(key, out PathNode choice))
            {
                choice = choose();
                staticBranchChoices[key] = choice;
            }
            return choice;
        }

        private PathNode ChooseBestBranch(PathNode fork, Func<PathNode, PathNode, float> segmentMetric, bool minimize)
        {
            if (remainingMetricByNode == null)
            {
                remainingMetricByNode = new float[Graph.Nodes.Count];
                remainingMetricStamp = new int[Graph.Nodes.Count];
            }
            branchSearchStamp++;

            PathNode forkEnd = Graph.ForkEnd(fork);
            PathNode best = null;
            float bestMetric = 0f;
            foreach (PathNode branch in fork.Next)
            {
                float metric = BranchMetric(fork, branch, forkEnd, segmentMetric, minimize);
                float signedMetric = minimize ? metric : -metric;
                if (best == null || signedMetric < bestMetric - TieTolerance)
                {
                    best = branch;
                    bestMetric = signedMetric;
                }
            }
            return best;
        }

        private float BranchMetric(PathNode from, PathNode to, PathNode forkEnd, Func<PathNode, PathNode, float> segmentMetric, bool minimize)
        {
            return segmentMetric(from, to) + RemainingMetric(to, forkEnd, segmentMetric, minimize);
        }

        private float RemainingMetric(PathNode node, PathNode forkEnd, Func<PathNode, PathNode, float> segmentMetric, bool minimize)
        {
            if (node == forkEnd || node.IsEnd)
                return 0f;
            if (remainingMetricStamp[node.Index] == branchSearchStamp)
                return remainingMetricByNode[node.Index];

            float result = minimize ? float.MaxValue : float.MinValue;
            foreach (PathNode next in node.Next)
            {
                float metric = BranchMetric(node, next, forkEnd, segmentMetric, minimize);
                result = minimize ? Math.Min(result, metric) : Math.Max(result, metric);
            }
            remainingMetricByNode[node.Index] = result;
            remainingMetricStamp[node.Index] = branchSearchStamp;
            return result;
        }

        private static float SegmentLength(PathNode from, PathNode to) => Vec3.Distance(from.Position, to.Position);

        private Func<PathNode, PathNode, float> UnitCountMetric(SimUnit unit)
        {
            if (unitCountByEdge == null)
                unitCountByEdge = new int[Graph.EdgeCount];
            Array.Clear(unitCountByEdge, 0, unitCountByEdge.Length);
            for (int i = 0; i < units.Count; i++)
            {
                SimUnit other = units[i];
                if (other != unit && other.IsActive && other.EdgeIndex >= 0)
                    unitCountByEdge[other.EdgeIndex]++;
            }
            return (from, to) => unitCountByEdge[from.EdgeTo(to)];
        }

        private Func<PathNode, PathNode, float> TowerDamageMetric(SimUnit unit)
        {
            var threats = new List<SimTower>();
            foreach (SimTower tower in towers)
            {
                if (tower.CanTarget(unit))
                    threats.Add(tower);
            }

            float height = unit.TravelHeight;
            return (from, to) =>
            {
                float damage = 0f;
                foreach (SimTower tower in threats)
                {
                    float range = tower.Spec.ShootRange;
                    float heightDifference = height - tower.Position.Y;
                    if (Math.Abs(heightDifference) >= range)
                        continue;

                    float rangeAtHeight = (float)Math.Sqrt(range * range - heightDifference * heightDifference);
                    damage += tower.Spec.DamagePerSecond * LengthInRange(from.Position, to.Position, tower.Position, rangeAtHeight);
                }
                return damage;
            };
        }

        private static float LengthInRange(Vec3 start, Vec3 end, Vec3 center, float range)
        {
            float segmentX = end.X - start.X;
            float segmentZ = end.Z - start.Z;
            float length = (float)Math.Sqrt(segmentX * segmentX + segmentZ * segmentZ);
            if (length < MinSegmentLength)
                return 0f;

            float directionX = segmentX / length;
            float directionZ = segmentZ / length;
            float toCenterX = center.X - start.X;
            float toCenterZ = center.Z - start.Z;
            float closest = toCenterX * directionX + toCenterZ * directionZ;
            float lineDistanceSquared = toCenterX * toCenterX + toCenterZ * toCenterZ - closest * closest;
            float halfChordSquared = range * range - lineDistanceSquared;
            if (halfChordSquared <= 0f)
                return 0f;

            float halfChord = (float)Math.Sqrt(halfChordSquared);
            float enter = Math.Max(0f, closest - halfChord);
            float exit = Math.Min(length, closest + halfChord);
            return Math.Max(0f, exit - enter);
        }
    }
}
