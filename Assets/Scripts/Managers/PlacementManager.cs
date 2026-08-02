using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private LayerMask furnitureLayer;
    [SerializeField] private GameObject ghostPrefab;

    private GameObject currentGhost;
    private ItemData selectedItem;
    private bool isBuildMode = false;
    private Material ghostMaterial;

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
    }

    private void Update()
    {
        if (!isBuildMode || currentGhost == null) return;

        // Получаем позицию курсора на полу
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, floorLayer))
        {
            Vector3 snappedPos = GridManager.Instance.SnapToGrid(hit.point);
            currentGhost.transform.position = snappedPos;

            // Проверяем возможность установки
            bool canPlace = CanPlaceItem(snappedPos);
            UpdateGhostColor(canPlace);

            // Установка по клику
            if (canPlace && Input.GetMouseButtonDown(0))
            {
                PlaceItem(snappedPos);
            }
        }

        // Отмена режима строительства по правой кнопке или Escape
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitBuildMode();
        }
    }

    public void EnterBuildMode(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("ItemData is null!");
            return;
        }

        if (!GameManager.Instance.CanAfford(item.buyPrice))
        {
            UIManager.Instance.ShowNotification("Недостаточно денег!");
            return;
        }

        selectedItem = item;
        isBuildMode = true;

        // Создаем призрака
        if (currentGhost != null) Destroy(currentGhost);

        if (ghostPrefab == null)
        {
            Debug.LogError("Ghost Prefab не назначен в PlacementManager!");
            return;
        }

        currentGhost = Instantiate(ghostPrefab);

        // Меняем размер и цвет призрака в зависимости от предмета
        Vector3 scale = new Vector3(item.gridSize.x, item.height, item.gridSize.y);
        currentGhost.transform.localScale = scale;

        // Устанавливаем цвет (полупрозрачный)
        Renderer renderer = currentGhost.GetComponent<Renderer>();
        if (renderer != null)
        {
            ghostMaterial = new Material(renderer.material);
            ghostMaterial.color = new Color(item.prototypeColor.r, item.prototypeColor.g, item.prototypeColor.b, 0.5f);
            renderer.material = ghostMaterial;
        }

        // Показываем сетку
        GridManager.Instance.ShowGrid(true);
        UIManager.Instance.ShowNotification($"Режим строительства: {item.displayName}");
    }

    public void ExitBuildMode()
    {
        isBuildMode = false;
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }
        GridManager.Instance?.ShowGrid(false);
        selectedItem = null;
        UIManager.Instance?.ClearInventorySlot();

        // Скрываем панель строительства
        if (UIManager.Instance != null)
        {
            // Вызываем через рефлексию или просто через публичный метод
            // Пока оставим так
        }
    }

    private bool CanPlaceItem(Vector3 position)
    {
        if (selectedItem == null) return false;

        // Проверка внутри магазина
        if (!GridManager.Instance.IsInsideShop(position))
            return false;

        // Проверка на занятость
        if (!GridManager.Instance.IsPositionFree(position, selectedItem.gridSize))
            return false;

        return true;
    }

    private void UpdateGhostColor(bool canPlace)
    {
        if (ghostMaterial != null)
        {
            ghostMaterial.color = canPlace ?
                new Color(0, 1, 0, 0.5f) : // Зеленый
                new Color(1, 0, 0, 0.5f);   // Красный
        }
    }

    private void PlaceItem(Vector3 position)
    {
        if (selectedItem == null) return;

        if (!GameManager.Instance.SpendMoney(selectedItem.buyPrice))
        {
            UIManager.Instance?.ShowNotification("Недостаточно денег!");
            return;
        }

        // Создаем реальный объект
        GameObject newObject = Instantiate(selectedItem.prefab, position, Quaternion.identity);
        newObject.transform.localScale = new Vector3(selectedItem.gridSize.x, selectedItem.height, selectedItem.gridSize.y);

        // Добавляем компонент InteractableObject (позже)
        // newObject.AddComponent<InteractableObject>();

        // Занимаем клетки в сетке
        GridManager.Instance.OccupyCells(position, selectedItem.gridSize);

        // Выходим из режима строительства
        UIManager.Instance?.ShowNotification($"{selectedItem.displayName} установлен!");
        ExitBuildMode();
    }

    public bool IsInBuildMode() => isBuildMode;
}