using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Настройки сетки")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Vector2Int shopSize = new Vector2Int(15, 20);
    [SerializeField] private Vector3 gridOffset = Vector3.zero;

    [Header("Визуализация сетки")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.3f);

    private bool[,] occupancyGrid;
    private List<Vector2Int> occupiedCells = new List<Vector2Int>();
    private GameObject gridVisualization;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        occupancyGrid = new bool[shopSize.x, shopSize.y];

        // Создаем визуализацию сетки
        if (showGrid)
        {
            CreateGridVisualization();
            ShowGrid(false); // По умолчанию скрыта
        }
    }

    private void CreateGridVisualization()
    {
        gridVisualization = new GameObject("GridVisualization");
        gridVisualization.transform.parent = transform;

        // Создаем линии сетки
        float width = shopSize.x;
        float height = shopSize.y;

        // Горизонтальные линии
        for (int y = 0; y <= shopSize.y; y++)
        {
            Vector3 start = new Vector3(-width / 2, 0.01f, -height / 2 + y);
            Vector3 end = new Vector3(width / 2, 0.01f, -height / 2 + y);
            CreateGridLine(start, end);
        }

        // Вертикальные линии
        for (int x = 0; x <= shopSize.x; x++)
        {
            Vector3 start = new Vector3(-width / 2 + x, 0.01f, -height / 2);
            Vector3 end = new Vector3(-width / 2 + x, 0.01f, height / 2);
            CreateGridLine(start, end);
        }

        // Добавляем лог для отладки
        Debug.Log($"Сетка создана: {shopSize.x}x{shopSize.y} клеток");
    }

    private void CreateGridLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = gridVisualization.transform;

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = gridColor;
        line.endColor = gridColor;
    }

    public void ShowGrid(bool show)
    {
        if (gridVisualization != null)
        {
            gridVisualization.SetActive(show);
            Debug.Log($"Сетка {(show ? "показана" : "скрыта")}");
        }
    }

    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        float x = Mathf.Round(worldPosition.x / gridSize) * gridSize;
        float z = Mathf.Round(worldPosition.z / gridSize) * gridSize;
        return new Vector3(x, 0, z) + gridOffset;
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - gridOffset;
        int x = Mathf.RoundToInt(localPos.x / gridSize + shopSize.x / 2);
        int z = Mathf.RoundToInt(localPos.z / gridSize + shopSize.y / 2);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x - shopSize.x / 2f) * gridSize;
        float z = (gridPos.y - shopSize.y / 2f) * gridSize;
        return new Vector3(x, 0, z) + gridOffset;
    }

    public bool IsPositionFree(Vector3 worldPosition, Vector2Int size)
    {
        Vector2Int start = WorldToGrid(worldPosition);

        for (int x = start.x; x < start.x + size.x; x++)
        {
            for (int y = start.y; y < start.y + size.y; y++)
            {
                if (x < 0 || x >= shopSize.x || y < 0 || y >= shopSize.y)
                    return false;

                if (occupancyGrid[x, y])
                    return false;
            }
        }
        return true;
    }

    public bool IsInsideShop(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGrid(worldPosition);
        return gridPos.x >= 0 && gridPos.x < shopSize.x &&
               gridPos.y >= 0 && gridPos.y < shopSize.y;
    }

    public void OccupyCells(Vector3 worldPosition, Vector2Int size)
    {
        Vector2Int start = WorldToGrid(worldPosition);

        for (int x = start.x; x < start.x + size.x; x++)
        {
            for (int y = start.y; y < start.y + size.y; y++)
            {
                if (x >= 0 && x < shopSize.x && y >= 0 && y < shopSize.y)
                {
                    occupancyGrid[x, y] = true;
                    occupiedCells.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    public void FreeCells(Vector3 worldPosition, Vector2Int size)
    {
        Vector2Int start = WorldToGrid(worldPosition);

        for (int x = start.x; x < start.x + size.x; x++)
        {
            for (int y = start.y; y < start.y + size.y; y++)
            {
                if (x >= 0 && x < shopSize.x && y >= 0 && y < shopSize.y)
                {
                    occupancyGrid[x, y] = false;
                    occupiedCells.Remove(new Vector2Int(x, y));
                }
            }
        }
    }

    public Vector2Int GetShopSize() => shopSize;
    public float GetGridSize() => gridSize;
}