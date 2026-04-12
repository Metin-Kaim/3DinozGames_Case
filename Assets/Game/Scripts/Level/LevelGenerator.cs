using System.Collections.Generic;
using UnityEngine;
using Assets.Game.Scripts.Signals;
using Assets.Game.Scripts.Datas.DataValues;
using Assets.Game.Scripts.Datas.UnityValues;
using Assets.Game.Scripts.Enum;

namespace Assets.Game.Scripts.Level
{
    public class LevelGenerator : MonoBehaviour
    {
        public const string FolderName = "LevelDatas";

        [Header("Level data (Resources/LevelDatas)")]
        [Tooltip("Yüklenecek JSON: Resources/LevelDatas/Level_{levelIndex}.json")]

        [Header("Sticks — Spawn")]
        [SerializeField] private StickHandler stickPrefab;
        [SerializeField] private StickLayoutConfig layoutConfig;
        [SerializeField] private Transform stickContainer;
        [SerializeField] private float spacingX = 1.5f;
        [SerializeField] private float spacingY = 1.5f;

        [Header("Chains — Placement")]
        [SerializeField] private Transform chainContainer;
        [SerializeField] private float hookSpacingX = 1.5f;

        [Header("Chains — Hook")]
        [SerializeField] private ChainController chainHookPrefab;

        [Header("Chains — Rings")]
        [SerializeField] private RingHandler ringPrefab;
        [SerializeField] private float ringStepY = 0.4f;
        [SerializeField] private Vector3 firstRingLocalOffset = new Vector3(0f, -0.4f, 0f);

        [Header("Chains — Fork tree")]
        [SerializeField] private float forkBranchSpacingX = 0.4f;

        private int _levelIndex;
        private int _ringSpawnIndexInChain;

        private LevelData _levelData;


        private void OnEnable()
        {
            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onGetStickLocalPositions += GetStickLocalPositions;
            GameSignals.Instance.onGameEnded += OnGameEnded;
        }
        private void OnDisable()
        {
            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onGetStickLocalPositions -= GetStickLocalPositions;
            GameSignals.Instance.onGameEnded -= OnGameEnded;
        }

        private void Start()
        {
            _levelIndex = SaveSignals.Instance.onGetSavedLevelIndex?.Invoke() ?? 1;

            ClearRuntimeLevelOverrides();
            if (!TryApplyLevelFromResources())
            {
                Debug.LogError(
                    $"[LevelGenerator] Seviye oluşturulamadı — JSON yüklenemedi veya geçersiz: Resources/{GetLoadPath(_levelIndex)}");
                return;
            }

            SpawnSticks();
            SpawnChains();
        }

        private void ClearRuntimeLevelOverrides()
        {
            _levelData = null;
        }

        private bool TryApplyLevelFromResources()
        {
            if (!TryLoadLevelDataFromResources(_levelIndex, out LevelData data))
                return false;

            _levelData = data;
            return true;
        }

        private byte[] GetAllRingColorBytesByHook(HookLevelData hook)
        {
            var ringColorBytes = new byte[hook.baseRingCount + hook.branch1BaseRingCount + hook.branch2BaseRingCount];
            int index = 0;
            for (int i = 0; i < hook.baseRingCount; i++)
                ringColorBytes[index++] = hook.trunkRingColorTypes[i];
            for (int i = 0; i < hook.branch1BaseRingCount; i++)
                ringColorBytes[index++] = hook.branch1RingColorTypes[i];
            for (int i = 0; i < hook.branch2BaseRingCount; i++)
                ringColorBytes[index++] = hook.branch2RingColorTypes[i];
            return ringColorBytes;
        }

        private List<Vector3> GetStickLocalPositions(int stickCount)
        {
            var positions = new List<Vector3>();
            if (layoutConfig == null || stickCount <= 0)
                return positions;

            Vector2Int grid = layoutConfig.GetGrid(stickCount);

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
                    float xLocal = startX + c * spacingX;
                    float yLocal = yCenterOffset - r * spacingY;
                    positions.Add(new Vector3(xLocal, yLocal, 0));
                    index++;
                    if (index >= stickCount)
                        return positions;
                }
            }

            return positions;
        }

        private void SpawnSticks()
        {
            if (stickPrefab == null || layoutConfig == null || stickContainer == null)
                return;

            List<Vector3> stickLocalPositions = GetStickLocalPositions(_levelData.stickCount);

            for (int i = 0; i < stickLocalPositions.Count; i++)
            {
                Vector3 localPos = stickLocalPositions[i];
                StickHandler stick = Instantiate(stickPrefab, stickContainer);
                stick.transform.localPosition = localPos;
                stick.Init((ColorType)_levelData.stickColorTypes[i]);
                LevelSignals.Instance?.onStickSpawned?.Invoke(stick);
            }
        }

        private void SpawnChains()
        {
            if (chainHookPrefab == null)
                return;

            List<Vector3> chainHookPositions = GetChainHookPositions(_levelData.hookCount, hookSpacingX);

            for (int i = 0; i < _levelData.hookCount; i++)
            {
                float hookPosX = chainHookPositions[i].x;

                var hookRootGo = new GameObject($"HookChain_{i}");
                Transform hookRoot = hookRootGo.transform;
                hookRoot.SetParent(chainContainer, false);
                hookRoot.localPosition = new Vector3(hookPosX, 0f, 0f);
                hookRoot.localRotation = Quaternion.identity;

                ChainController chainController = Instantiate(chainHookPrefab, hookRoot);

                HookLevelData hook = _levelData.hooks[i];

                if (ringPrefab == null || hook == null)
                    continue;

                _ringSpawnIndexInChain = 0;
                var chainRings = new List<RingHandler>();
                SpawnChainRings(hook, hookRoot, i, chainRings);

                byte[] allRingColorBytes = GetAllRingColorBytesByHook(hook);
                chainController.Init(chainRings, allRingColorBytes);
            }
        }

        private void SpawnChainRings(HookLevelData hook, Transform hookRoot, int hookIndex, List<RingHandler> chainRings)
        {
            if (hook == null)
                return;

            const float localX = 0f;
            RingHandler prev = null;
            const int startDepth = 0;

            for (int i = 0; i < hook.baseRingCount; i++)
            {
                int depth = startDepth + i;
                float y = firstRingLocalOffset.y - depth * ringStepY;
                Vector3 localPos = new Vector3(firstRingLocalOffset.x + localX, y, firstRingLocalOffset.z);
                prev = SpawnRing(hookRoot, localPos, depth, localX, prev, hookIndex, chainRings);
            }

            if (!hook.fork)
                return;

            RingHandler forkPoint = prev;
            int childStartDepth = startDepth + hook.baseRingCount;

            float branch1LocalX = GetForkBranchLocalX(0, 2, forkBranchSpacingX);
            SpawnBranchRingStack(hook.branch1BaseRingCount, hookRoot, forkPoint, childStartDepth, localX + branch1LocalX, hookIndex, chainRings);

            float branch2LocalX = GetForkBranchLocalX(1, 2, forkBranchSpacingX);
            SpawnBranchRingStack(hook.branch2BaseRingCount, hookRoot, forkPoint, childStartDepth, localX + branch2LocalX, hookIndex, chainRings);
        }

        private void SpawnBranchRingStack(int ringCount, Transform hookRoot, RingHandler attachTo, int startDepth, float localX, int hookIndex, List<RingHandler> chainRings)
        {
            if (ringCount < 1)
                return;

            RingHandler prev = attachTo;
            for (int i = 0; i < ringCount; i++)
            {
                int depth = startDepth + i;
                float y = firstRingLocalOffset.y - depth * ringStepY;
                Vector3 localPos = new Vector3(firstRingLocalOffset.x + localX, y, firstRingLocalOffset.z);
                prev = SpawnRing(hookRoot, localPos, depth, localX, prev, hookIndex, chainRings);
            }
        }

        private float GetForkBranchLocalX(int branchIndex, int branchCount, float forkBranchSpacingX)
        {
            if (branchCount <= 1)
                return 0f;

            float startX = -((branchCount - 1) * 0.5f) * forkBranchSpacingX;
            return startX + branchIndex * forkBranchSpacingX;
        }

        private RingHandler SpawnRing(Transform hookRoot, Vector3 localPosition, int depthFromHook, float localXOffset, RingHandler upperRing, int hookIndex, List<RingHandler> chainRings)
        {
            RingHandler handler = Instantiate(ringPrefab, hookRoot);
            handler.gameObject.name = $"Hook{hookIndex}_Ring_{_ringSpawnIndexInChain++}";

            handler.transform.localPosition = localPosition;

            float zTwist = depthFromHook % 2 == 1 ? 90f : 0f;
            if (localXOffset > 0.0001f)
                zTwist = -zTwist;

            handler.transform.localRotation = Quaternion.Euler(-90f, 0f, zTwist);

            handler.ConnectAbove(upperRing);
            chainRings.Add(handler);
            return handler;
        }

        private List<Vector3> GetChainHookPositions(int count, float spacingX)
        {
            List<Vector3> positions = new(count);
            float startX = -(count - 1) * spacingX * 0.5f;

            for (int i = 0; i < count; i++)
                positions.Add(new Vector3(startX + i * spacingX, 0f, 0f));

            return positions;
        }

        private string GetLoadPath(int levelIndex) => $"{FolderName}/Level_{levelIndex}";

        private bool TryLoadLevelDataFromResources(int levelIndex, out LevelData data)
        {
            data = null;
            if (levelIndex < 1)
                return false;

            string path = GetLoadPath(levelIndex);
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
                return false;

            data = JsonUtility.FromJson<LevelData>(textAsset.text);
            return data != null && data.hooks != null && data.hooks.Length > 0;
        }

        private void OnGameEnded(bool isWin, float delay)
        {
            if (isWin)
                _levelIndex++;
            SaveSignals.Instance.onSaveLevelIndex?.Invoke(_levelIndex);
        }

    }
}
