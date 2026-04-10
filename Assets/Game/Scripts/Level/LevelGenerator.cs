using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject stickPrefab;
    public StickLayoutConfig layoutConfig;
    public Transform stickContainer;

    [Header("Level")]
    [Range(1, 10)]
    public int stickCount = 5;

    [Header("Spacing")]
    public float spacingX = 2.5f;
    public float spacingY = 2.5f;

    private void Awake()
    {
        if (stickContainer == null)
        {
            var containerGo = new GameObject("StickContainer");
            containerGo.transform.SetParent(transform, false);
            stickContainer = containerGo.transform;
        }
    }

    private void Start()
    {
        SpawnSticks();
    }

    private void SpawnSticks()
    {
        Vector2Int grid = layoutConfig.GetGrid(stickCount);

        List<Vector3> localPositions = GeneratePositions(stickCount, grid, spacingX, spacingY);

        foreach (Vector3 localPos in localPositions)
        {
            GameObject stick = Instantiate(stickPrefab, stickContainer);
           stick.transform.localPosition = localPos;
            stick.transform.localRotation = Quaternion.identity;
        }
    }

    private List<Vector3> GeneratePositions(int stickCount, Vector2Int grid, float spacingX, float spacingY)
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
