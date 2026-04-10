using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [Header("Sticks")]
    [SerializeField] private GameObject stickPrefab;
    [SerializeField] private StickLayoutConfig layoutConfig;
    [SerializeField] private Transform stickContainer;
    [Range(1, 5)]
    [SerializeField] private int stickCount = 3;
    [SerializeField] private float spacingX = 1.5f;
    [SerializeField] private float spacingY = 1.5f;

    [Header("Chains — Placement")]
    [SerializeField] private Transform chainContainer;
    [Range(1, 3)]
    [SerializeField] private int hookCount = 2;
    [SerializeField] private float hookSpacingX = 1.5f;

    [Header("Chains — Hook")]
    [SerializeField] private GameObject chainHookPrefab;

    [Header("Chains — Links")]
    [FormerlySerializedAs("hookPrefab")]
    [SerializeField] private GameObject chainLinkPrefab;
    [Min(1)]
    [SerializeField] private int linksPerChain = 5;
    [SerializeField] private float chainLinkStepY = 0.4f;
    [SerializeField] private Vector3 firstLinkLocalOffset = new Vector3(0f, -0.4f, 0f);

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
        SpawnSticks();
        SpawnChains();
    }

    private void SpawnSticks()
    {
        Vector2Int grid = layoutConfig.GetGrid(stickCount);

        List<Vector3> localPositions = GenerateStickPositions(stickCount, grid, spacingX, spacingY);

        foreach (Vector3 localPos in localPositions)
        {
            GameObject stick = Instantiate(stickPrefab, stickContainer);
            stick.transform.localPosition = localPos;
        }
    }

    private void SpawnChains()
    {
        if (chainHookPrefab == null || hookCount <= 0)
            return;

        List<Vector3> hookAnchors = GenerateChainPositions(hookCount, hookSpacingX);

        for (int h = 0; h < hookCount; h++)
        {
            float anchorX = hookAnchors[h].x;

            var hookRootGo = new GameObject($"HookChain_{h}");
            Transform hookRoot = hookRootGo.transform;
            hookRoot.SetParent(chainContainer, false);
            hookRoot.localPosition = new Vector3(anchorX, 0f, 0f);
            hookRoot.localRotation = Quaternion.identity;

            Instantiate(chainHookPrefab, hookRoot);

            if (chainLinkPrefab == null || linksPerChain <= 0)
                continue;

            for (int i = 0; i < linksPerChain; i++)
            {
                GameObject linkGo = Instantiate(chainLinkPrefab, hookRoot);
                Transform t = linkGo.transform;
                t.localPosition = firstLinkLocalOffset + new Vector3(0f, -i * chainLinkStepY, 0f);
                t.localRotation = i % 2 == 1
                    ? Quaternion.Euler(-90f, 0f, 90f)
                    : Quaternion.Euler(-90f, 0f, 0f);
            }
        }
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
