using UnityEngine;

public class StorageUnit : InteractableObject
{
    [Header("Настройки хранилища")]
    [SerializeField] private ItemData storedItem; // Что хранится
    [SerializeField] private int capacity = 10;
    [SerializeField] private int currentAmount = 5;
    [SerializeField] private float refillTime = 10f; // Время обновления

    private bool isRefilling = false;

    public override void OnInteract(InventoryManager inventory)
    {
        // Если в руках ничего нет - берем предмет из хранилища
        if (inventory.CurrentItem == null)
        {
            if (currentAmount > 0)
            {
                inventory.TakeItem(storedItem);
                currentAmount--;
                Debug.Log($"Взяли {storedItem.displayName}. Осталось: {currentAmount}/{capacity}");
                UIManager.Instance?.ShowNotification($"Взяли {storedItem.displayName}");
            }
            else
            {
                UIManager.Instance?.ShowNotification($"{storedItem.displayName} закончился!");
                StartRefill();
            }
        }
        else if (inventory.CurrentItem == storedItem)
        {
            // Если в руках есть такой же предмет - складываем обратно
            if (currentAmount < capacity)
            {
                inventory.ClearHands();
                currentAmount++;
                Debug.Log($"Положили {storedItem.displayName}. Теперь: {currentAmount}/{capacity}");
                UIManager.Instance?.ShowNotification($"Положили {storedItem.displayName}");
            }
            else
            {
                UIManager.Instance?.ShowNotification($"{storedItem.displayName} уже полный!");
            }
        }
        else
        {
            UIManager.Instance?.ShowNotification($"Здесь хранится {storedItem.displayName}, а у вас {inventory.CurrentItem.displayName}");
        }
    }

    public void StartRefill()
    {
        if (isRefilling) return;

        UIManager.Instance?.ShowNotification($"Заказываем {storedItem.displayName}...");
        StartCoroutine(RefillCoroutine());
    }

    private System.Collections.IEnumerator RefillCoroutine()
    {
        isRefilling = true;

        // Ждем refillTime секунд
        yield return new WaitForSeconds(refillTime);

        // Пополняем до максимума
        currentAmount = capacity;
        isRefilling = false;

        UIManager.Instance?.ShowNotification($"{storedItem.displayName} пополнен!");
        Debug.Log($"{storedItem.displayName} пополнен до {capacity}");
    }

    public void SetStoredItem(ItemData item)
    {
        storedItem = item;
    }

    public ItemData GetStoredItem() => storedItem;
    public int GetCurrentAmount() => currentAmount;
    public int GetCapacity() => capacity;
    public bool IsEmpty() => currentAmount <= 0;
    public bool IsFull() => currentAmount >= capacity;
}