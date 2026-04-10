using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Assets.Game.Scripts.Signals;
using Assets.Game.Scripts.Datas.UnityValues;
using Assets.Game.Scripts.Datas.DataValues;

namespace Assets.Game.Scripts.Level
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Sticks")]
        [SerializeField] private StickHandler stickPrefab;
        [SerializeField] private StickLayoutConfig layoutConfig;
        [SerializeField] private Transform stickContainer;
        [Range(1, 5)]
        [SerializeField] private int stickCount = 3;
        [SerializeField] private float spacingX = 1.5f;
        [SerializeField] private float spacingY = 1.5f;
        [SerializeField] private float stickDisappearDuration = 0.4f;
        [SerializeField] private float stickRealignDuration = 0.35f;
        [Tooltip("Dolu stick listeden düştükten sonra kalanların hizalanması için kısa bekleme (saniye). 0 = anında.")]
        [SerializeField] private float stickRealignDelay = 0.08f;

        [Header("Chains — Placement")]
        [SerializeField] private Transform chainContainer;
        [Range(1, 3)]
        [SerializeField] private int hookCount = 2;
        [SerializeField] private float hookSpacingX = 1.5f;

        [Header("Chains — Hook")]
        [SerializeField] private GameObject chainHookPrefab;

        [Header("Chains — Rings")]
        [SerializeField] private ChainRingHandler chainRingPrefab;
        [SerializeField] private float ringStepY = 0.4f;
        [SerializeField] private Vector3 firstRingLocalOffset = new Vector3(0f, -0.4f, 0f);

        [Header("Chains — Fork tree")]
        [Tooltip("Kök: baseRingCount = fork öncesi düz segment. Her dal kendi baseRingCount ile fork sonrası halkaları tanımlar; branches ile iç içe fork.")]
        [SerializeField] private ChainForkNode chainRoot = new ChainForkNode { baseRingCount = 5 };
        [SerializeField] private float forkBranchSpacingX = 0.4f;

        private int _ringSpawnIndexInChain;

        private readonly List<StickHandler> _sticks = new();
        private readonly List<List<ChainRingHandler>> _chains = new();
        private readonly Dictionary<int, List<ChainRingHandler>> _chainsByHookIndex = new();

        public IReadOnlyList<StickHandler> Sticks => _sticks;
        public IReadOnlyList<List<ChainRingHandler>> Chains => _chains;
        public IReadOnlyDictionary<int, List<ChainRingHandler>> ChainsByHookIndex => _chainsByHookIndex;

        private void Awake()
        {
            if (stickContainer == null)
            {
                var containerGo = new GameObject("StickContainer");
                containerGo.transform.SetParent(transform, false);
                stickContainer = containerGo.transform;
            }

            if (chainContainer == null)
            {
                var chainGo = new GameObject("ChainContainer");
                chainGo.transform.SetParent(transform, false);
                chainContainer = chainGo.transform;
            }

        }

        private void Start()
        {
            EnsureLevelSignals();
            SpawnSticks();
            RegisterStickProvider();
            SpawnChains();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            if (LevelSignals.Instance != null)
                LevelSignals.Instance.onStickFilled -= OnStickFilled;
        }

        private static void EnsureLevelSignals()
        {
            if (LevelSignals.Instance != null)
                return;

            var go = new GameObject("LevelSignals");
            go.AddComponent<LevelSignals>();
        }

        private void RegisterStickProvider()
        {
            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onGetStick = GetNextAvailableStick;
            LevelSignals.Instance.onStickFilled += OnStickFilled;
        }

        private void OnStickFilled(StickHandler stick)
        {
            if (stick == null)
                return;

            // Listeden hemen çıkar: RealignSticks içindeki DOKill, küçülme tween'ini öldürmesin diye
            // (stick hâlâ _sticks içindeyken realign, aynı frame'de başka stick dolunca patlıyordu).
            if (!_sticks.Remove(stick))
                return;

            ScheduleRealignSticks();

            stick.PlayDisappearAnimation(stickDisappearDuration, () =>
            {
                if (stick != null)
                    Destroy(stick.gameObject);
            });
        }

        private void ScheduleRealignSticks()
        {
            if (stickRealignDelay <= 0f)
            {
                RealignSticks();
                return;
            }

            DOVirtual.DelayedCall(stickRealignDelay, RealignSticksIfAlive).SetTarget(this);
        }

        private void RealignSticksIfAlive()
        {
            if (this == null)
                return;

            RealignSticks();
        }

        private void RealignSticks()
        {
            int count = _sticks.Count;
            if (count == 0)
                return;

            Vector2Int grid = layoutConfig.GetGrid(count);
            List<Vector3> positions = GenerateStickPositions(count, grid, spacingX, spacingY);

            for (int i = 0; i < count; i++)
            {
                StickHandler s = _sticks[i];
                if (s == null)
                    continue;

                Transform t = s.transform;
                t.DOKill();
                t.DOLocalMove(positions[i], stickRealignDuration).SetEase(Ease.OutQuad);
            }
        }

        private StickHandler GetNextAvailableStick()
        {
            foreach (StickHandler stick in _sticks)
            {
                if (stick != null && stick.CanAcceptRing())
                    return stick;
            }

            return null;
        }

        private void SpawnSticks()
        {
            Vector2Int grid = layoutConfig.GetGrid(stickCount);

            List<Vector3> localPositions = GenerateStickPositions(stickCount, grid, spacingX, spacingY);

            foreach (Vector3 localPos in localPositions)
            {
                StickHandler stick = Instantiate(stickPrefab, stickContainer);
                stick.transform.localPosition = localPos;
                _sticks.Add(stick);
            }
        }

        private void SpawnChains()
        {
            if (chainHookPrefab == null || hookCount <= 0)
                return;

            List<Vector3> hookAnchors = GenerateChainPositions(hookCount, hookSpacingX);

            for (int i = 0; i < hookCount; i++)
            {
                float anchorX = hookAnchors[i].x;

                var hookRootGo = new GameObject($"HookChain_{i}");
                Transform hookRoot = hookRootGo.transform;
                hookRoot.SetParent(chainContainer, false);
                hookRoot.localPosition = new Vector3(anchorX, 0f, 0f);
                hookRoot.localRotation = Quaternion.identity;

                GameObject hookInstance = Instantiate(chainHookPrefab, hookRoot);
                ChainController chainController = hookInstance.GetComponent<ChainController>();

                if (chainRingPrefab == null || chainRoot == null)
                    continue;

                _ringSpawnIndexInChain = 0;
                var chainRings = new List<ChainRingHandler>();
                SpawnForkNodeRecursive(chainRoot, hookRoot, null, 0, 0f, i, chainRings);

                _chains.Add(chainRings);
                _chainsByHookIndex[i] = chainRings;

                if (chainController != null)
                    chainController.Init(chainRings);
            }
        }

        private void SpawnForkNodeRecursive(ChainForkNode node, Transform hookRoot, ChainRingHandler attachTo, int startDepth, float localX, int hookIndex, List<ChainRingHandler> chainRings)
        {
            if (node == null || node.baseRingCount < 1)
                return;

            ChainRingHandler prev = attachTo;

            for (int i = 0; i < node.baseRingCount; i++)
            {
                int depth = startDepth + i;
                float y = firstRingLocalOffset.y - depth * ringStepY;
                Vector3 localPos = new Vector3(firstRingLocalOffset.x + localX, y, firstRingLocalOffset.z);
                prev = SpawnRing(hookRoot, localPos, depth, localX, prev, hookIndex, chainRings);
            }

            if (node.branches == null || node.branches.Count == 0)
                return;

            ChainRingHandler forkPoint = prev;
            int childStartDepth = startDepth + node.baseRingCount;

            for (int b = 0; b < node.branches.Count; b++)
            {
                float xOff = GetForkBranchLocalX(b, node.branches.Count, forkBranchSpacingX);
                SpawnForkNodeRecursive(node.branches[b], hookRoot, forkPoint, childStartDepth, localX + xOff, hookIndex, chainRings);
            }
        }

        private float GetForkBranchLocalX(int branchIndex, int branchCount, float forkBranchSpacingX)
        {
            if (branchCount <= 1)
                return 0f;

            float startX = -((branchCount - 1) * 0.5f) * forkBranchSpacingX;
            return startX + branchIndex * forkBranchSpacingX;
        }

        private ChainRingHandler SpawnRing(Transform hookRoot, Vector3 localPosition, int depthFromHook, float localXOffset, ChainRingHandler upperRing, int hookIndex, List<ChainRingHandler> chainRings)
        {
            ChainRingHandler handler = Instantiate(chainRingPrefab, hookRoot);
            handler.gameObject.name = $"Hook{hookIndex}_Ring_{_ringSpawnIndexInChain++}";
            Transform t = handler.transform;
            t.localPosition = localPosition;

            float zTwist = depthFromHook % 2 == 1 ? 90f : 0f;
            // Mesh genelde +/- X’te aynı görünmez; sağ taraftaki (pozitif localX) dallarda Z işaretini yansıt.
            if (localXOffset > 0.0001f)
                zTwist = -zTwist;

            t.localRotation = Quaternion.Euler(-90f, 0f, zTwist);

            handler.ConnectAbove(upperRing);
            chainRings.Add(handler);
            return handler;
        }

        private List<Vector3> GenerateChainPositions(int count, float spacingX)
        {
            List<Vector3> positions = new(count);
            float startX = -(count - 1) * spacingX * 0.5f;

            for (int i = 0; i < count; i++)
                positions.Add(new Vector3(startX + i * spacingX, 0f, 0f));

            return positions;
        }

        private List<Vector3> GenerateStickPositions(int stickCount, Vector2Int grid, float spacingX, float spacingY)
        {
            List<Vector3> positions = new();

            int row = grid.x;
            int col = grid.y;

            float yCenterOffset = row > 1 ? (row - 1) * spacingY * 0.5f : 0f;

            int index = 0;

            for (int r = 0; r < row; r++)
            {
                int itemsInRow = Mathf.Min(col, stickCount - index);

                float startX = -(itemsInRow - 1) * spacingX / 2f;

                for (int c = 0; c < itemsInRow; c++)
                {
                    float x = startX + c * spacingX;
                    float y = yCenterOffset - r * spacingY;

                    positions.Add(new Vector3(x, y, 0));

                    index++;

                    if (index >= stickCount)
                        return positions;
                }
            }

            return positions;
        }
    }
}
