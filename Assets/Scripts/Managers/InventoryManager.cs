using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Настройки инвентаря")]
    [SerializeField] private int maxItemsInHand = 1; // Максимум предметов в руках (пока 1)
    [SerializeField] private float dropDistance = 2f; // Расстояние для выброса предмета

    private ItemData currentItem;
    private GameObject currentItemObject; // Визуальное отображение предмета в руках
    private List<ItemData> craftedItems = new List<ItemData>();

    // События для UI
    public System.Action<ItemData> OnItemChanged;

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

    private void Start()
    {
        ClearHands();
    }

    private void Update()
    {
        // Клик правой кнопкой для выброса предмета
        if (Input.GetMouseButtonDown(1) && currentItem != null)
        {
            DropItem();
        }

        // Клавиша Q для выброса
        if (Input.GetKeyDown(KeyCode.Q) && currentItem != null)
        {
            DropItem();
        }

        // Обновляем позицию предмета в руках (следует за камерой)
        if (currentItemObject != null)
        {
            currentItemObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            currentItemObject.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void TakeItem(ItemData newItem)
    {
        if (newItem == null)
        {
            Debug.LogWarning("Попытка взять null предмет");
            return;
        }

        // Если в руках уже есть предмет - пытаемся скрафтить
        if (currentItem != null)
        {
            ItemData result = TryCraft(currentItem, newItem);
            if (result != null)
            {
                // Успешный крафт
                currentItem = result;
                craftedItems.Add(result);
                UpdateVisuals();
                UIManager.Instance?.ShowNotification($"Создано: {result.displayName}!");
                Debug.Log($"Скрафтили {result.displayName} из {currentItem.displayName} и {newItem.displayName}");
                return;
            }
            else
            {
                // Крафт не удался - заменяем предмет
                Debug.Log($"Заменили {currentItem.displayName} на {newItem.displayName}");
                ClearHands();
            }
        }

        // Берем новый предмет
        currentItem = newItem;
        UpdateVisuals();
        UIManager.Instance?.ShowNotification($"Взяли {newItem.displayName}");
        Debug.Log($"Взяли {newItem.displayName}");
    }

    public void ClearHands()
    {
        currentItem = null;
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
            currentItemObject = null;
        }
        UIManager.Instance?.ClearInventorySlot();
        OnItemChanged?.Invoke(null);
    }

    private void DropItem()
    {
        if (currentItem == null) return;

        // Создаем объект на земле
        if (currentItem.prefab != null)
        {
            Vector3 dropPos = Camera.main.transform.position + Camera.main.transform.forward * dropDistance;
            dropPos.y = 0.5f;

            GameObject dropped = Instantiate(currentItem.prefab, dropPos, Quaternion.identity);
            dropped.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Добавляем компонент для подбора (можно реализовать позже)
            UIManager.Instance?.ShowNotification($"Выбросили {currentItem.displayName}");
        }

        ClearHands();
    }

    private ItemData TryCraft(ItemData itemA, ItemData itemB)
    {
        // Проверяем рецепт в itemA
        if (itemA.recipeIngredients != null && itemA.recipeIngredients.Contains(itemB))
        {
            if (itemA.craftResult != null)
            {
                return itemA.craftResult;
            }
        }

        // Проверяем рецепт в itemB
        if (itemB.recipeIngredients != null && itemB.recipeIngredients.Contains(itemA))
        {
            if (itemB.craftResult != null)
            {
                return itemB.craftResult;
            }
        }

        return null;
    }

    private void UpdateVisuals()
    {
        if (currentItem == null)
        {
            ClearHands();
            return;
        }

        // Удаляем старый визуальный объект
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
            currentItemObject = null;
        }

        // Создаем новый визуальный объект
        if (currentItem.prefab != null)
        {
            currentItemObject = Instantiate(currentItem.prefab);
            currentItemObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        else
        {
            // Заглушка - куб
            currentItemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentItemObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Renderer renderer = currentItemObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = currentItem.prototypeColor;
            }
        }

        // Обновляем UI
        UIManager.Instance?.UpdateInventoryUI(currentItem.icon, currentItem.displayName);
        OnItemChanged?.Invoke(currentItem);
    }

    public ItemData GetCurrentItem() => currentItem;
    public bool HasItem() => currentItem != null;
    public List<ItemData> GetCraftedItems() => craftedItems;
}