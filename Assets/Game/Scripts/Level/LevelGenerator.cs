using System.Collections.Generic;
using UnityEngine;

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

    [Header("Chains")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private Transform chainContainer;
    [Range(1, 3)]
    [SerializeField] private int chainCount = 2;
    [SerializeField] private float chainSpacingX = 1.5f;

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
        if (hookPrefab == null || chainCount <= 0)
            return;

        List<Vector3> localPositions = GenerateChainPositions(chainCount, chainSpacingX);

        foreach (Vector3 localPos in localPositions)
        {
            GameObject chain = Instantiate(hookPrefab, chainContainer);
            chain.transform.localPosition = localPos;
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
