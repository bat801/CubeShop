using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private LayerMask furnitureLayer;

    private GameObject currentGhost;
    private ItemData selectedItem;
    private bool isBuildMode = false;

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
            Vector3 snappedPos = GridManager.Instance.SnapToGrid(hit.point, selectedItem.height);
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

        // Создаем призрака из префаба предмета
        if (item.prefab == null)
        {
            Debug.LogError($"Prefab для {item.displayName} не назначен!");
            // Создаем заглушку (куб)
            currentGhost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentGhost.transform.localScale = new Vector3(item.gridSize.x, item.height, item.gridSize.y);
        }
        else
        {
            currentGhost = Instantiate(item.prefab);
        }

        // Делаем все материалы полупрозрачными
        MakeGhostTransparent(currentGhost);

        // Устанавливаем начальный цвет (зеленый)
        UpdateGhostColor(true);

        // Показываем сетку
        GridManager.Instance?.ShowGrid(true);
        UIManager.Instance?.ShowNotification($"Режим строительства: {item.displayName}");
    }

    private void MakeGhostTransparent(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = new Material(materials[i]);
                Color color = mat.color;
                color.a = 0.5f;
                mat.color = color;

                // Настройка для прозрачности
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;

                materials[i] = mat;
            }
            renderer.materials = materials;
        }
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

        // Скрываем панель строительства через UIManager
        // (добавим метод позже)
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
        if (currentGhost == null) return;

        Color targetColor = canPlace ? Color.green : Color.red;
        targetColor.a = 0.5f;

        Renderer[] renderers = currentGhost.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material mat in materials)
            {
                Color color = mat.color;
                color.r = targetColor.r;
                color.g = targetColor.g;
                color.b = targetColor.b;
                color.a = 0.5f;
                mat.color = color;
            }
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

        // Создаем реальный объект из префаба
        GameObject newObject;
        if (selectedItem.prefab != null)
        {
            newObject = Instantiate(selectedItem.prefab, position, Quaternion.identity);
        }
        else
        {
            // Заглушка
            newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.transform.localScale = new Vector3(selectedItem.gridSize.x, selectedItem.height, selectedItem.gridSize.y);
            Renderer renderer = newObject.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = selectedItem.prototypeColor;
        }

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