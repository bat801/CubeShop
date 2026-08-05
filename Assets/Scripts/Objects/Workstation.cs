using UnityEngine;

public class Workstation : InteractableObject
{
    [Header("Настройки станка")]
    [SerializeField] private ItemData inputItem; // Что нужно подать
    [SerializeField] private ItemData outputItem; // Что производится
    [SerializeField] private bool autoStart = false; // Автоматически начинать производство
    [SerializeField] private int maxStock = 1; // Максимум готовых предметов
    [SerializeField] private int currentStock = 0;

    public override void OnInteract(InventoryManager inventory)
    {
        // Если станок произвел продукт
        if (isReady && currentStock < maxStock)
        {
            inventory.TakeItem(outputItem);
            currentStock++;
            Debug.Log($"Взяли {outputItem.displayName}. В наличии: {currentStock}/{maxStock}");

            if (currentStock >= maxStock)
            {
                SetReadyState(false);
            }

            // Если autoStart и есть ресурсы - начинаем новое производство
            if (autoStart && HasResources())
            {
                StartCrafting();
            }
            return;
        }

        // Если в руках есть ингредиент для загрузки станка
        if (inventory.CurrentItem != null && inventory.CurrentItem == inputItem)
        {
            // Забираем ингредиент
            inventory.ClearHands();
            StartCrafting();
            Debug.Log($"Загрузили {inputItem.displayName} в {data.displayName}");
        }
        else if (inventory.CurrentItem != null)
        {
            UIManager.Instance?.ShowNotification($"Нужен {inputItem.displayName}, а у вас {inventory.CurrentItem.displayName}");
        }
        else
        {
            UIManager.Instance?.ShowNotification($"Положите {inputItem.displayName} в {data.displayName}");
        }
    }

    protected override void Start()
    {
        base.Start();
        SetReadyState(false);

        // Если autoStart и есть ресурсы - начинаем
        if (autoStart && HasResources())
        {
            StartCrafting();
        }
    }

    public void StartCrafting()
    {
        if (isBusy || isReady) return;
        if (!HasResources())
        {
            UIManager.Instance?.ShowNotification($"Нет ресурсов для производства {outputItem.displayName}");
            return;
        }

        StartCoroutine(CraftCoroutine(data.craftTime));
        Debug.Log($"Начали производство {outputItem.displayName}");
    }

    private bool HasResources()
    {
        // Проверяем наличие ресурсов (можно расширить)
        return true;
    }

    protected override System.Collections.IEnumerator CraftCoroutine(float craftTime)
    {
        isBusy = true;
        isReady = false;
        currentTimer = craftTime;

        while (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            // Обновляем индикатор
            yield return null;
        }

        isReady = true;
        isBusy = false;
        SetReadyState(true);
        currentStock = 0;

        UIManager.Instance?.ShowNotification($"{outputItem.displayName} готов!");
        Debug.Log($"{outputItem.displayName} произведен!");
    }

    public void RefillResources()
    {
        // Метод для пополнения ресурсов (будет использоваться позже)
        Debug.Log($"Пополнили ресурсы для {data.displayName}");
    }

    public bool NeedRefill()
    {
        return !HasResources() && !isBusy;
    }
}