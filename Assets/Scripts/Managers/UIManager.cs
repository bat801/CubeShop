using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Верхняя панель")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expSlider;

    [Header("Нижняя панель (Инвентарь)")]
    [SerializeField] private Image inventorySlot;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Header("Панель действий")]
    [SerializeField] private Button buildButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button statsButton;
    [SerializeField] private GameObject buildPanel;

    [Header("Ссылки на предметы для строительства")]
    [SerializeField] private Button btnTable;
    [SerializeField] private Button btnCashRegister;
    [SerializeField] private Button btnCoffeeMachine;

    private bool isBuildModeActive = false;
    private ItemData selectedBuildItem;

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
        // Подписываемся на события GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged.AddListener(UpdateMoneyUI);
            GameManager.Instance.OnLevelChanged.AddListener(UpdateLevelUI);
            GameManager.Instance.OnExperienceChanged.AddListener(UpdateExpUI);
        }

        // Настраиваем кнопки
        if (buildButton != null)
            buildButton.onClick.AddListener(ToggleBuildMode);

        // Настраиваем кнопки выбора предметов
        if (btnTable != null)
            btnTable.onClick.AddListener(() => SelectBuildItem("Item_Table"));

        if (btnCashRegister != null)
            btnCashRegister.onClick.AddListener(() => SelectBuildItem("Item_CashRegister"));

        if (btnCoffeeMachine != null)
            btnCoffeeMachine.onClick.AddListener(() => SelectBuildItem("Item_CoffeeMachine"));

        // Инициализация UI
        UpdateMoneyUI(GameManager.Instance.GetMoney());
        UpdateLevelUI(GameManager.Instance.GetLevel());
        UpdateExpUI(0f);
        ClearInventorySlot();

        // Скрываем панель строительства
        if (buildPanel != null)
            buildPanel.SetActive(false);
    }

    private void SelectBuildItem(string itemName)
    {
        // Загружаем ItemData из папки ScriptableObjects
        ItemData item = Resources.Load<ItemData>($"ScriptableObjects/Items/{itemName}");
        if (item != null)
        {
            if (PlacementManager.Instance != null)
            {
                // Проверяем, что префаб назначен
                if (item.prefab == null)
                {
                    Debug.LogWarning($"У {item.displayName} не назначен префаб! Будет использован куб-заглушка.");
                }
                PlacementManager.Instance.EnterBuildMode(item);
                isBuildModeActive = true;
                if (buildPanel != null)
                    buildPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError($"Не найден ItemData: {itemName}");
        }
    }

    public void UpdateMoneyUI(int money)
    {
        if (moneyText != null)
            moneyText.text = $"${money}";
    }

    public void UpdateLevelUI(int level)
    {
        if (levelText != null)
            levelText.text = $"Level {level}";
    }

    public void UpdateExpUI(float progress)
    {
        if (expSlider != null)
            expSlider.value = progress;
    }

    public void UpdateInventoryUI(Sprite icon, string itemName)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.color = icon != null ? Color.white : new Color(1, 1, 1, 0);
        }

        if (itemNameText != null)
        {
            itemNameText.text = string.IsNullOrEmpty(itemName) ? "Пусто" : itemName;
        }
    }

    public void ClearInventorySlot()
    {
        UpdateInventoryUI(null, "Пусто");
    }

    public void ToggleBuildMode()
    {
        if (!isBuildModeActive)
        {
            // Показываем панель выбора предметов
            if (buildPanel != null)
                buildPanel.SetActive(true);
        }
        else
        {
            // Выходим из режима строительства
            if (PlacementManager.Instance != null)
                PlacementManager.Instance.ExitBuildMode();

            isBuildModeActive = false;
            if (buildPanel != null)
                buildPanel.SetActive(false);
        }
        Debug.Log($"Build Mode: {(isBuildModeActive ? "ON" : "OFF")}");
    }

    public void ShowNotification(string message)
    {
        Debug.Log($"[NOTIFICATION] {message}");
        // TODO: Создать всплывающее уведомление
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged.RemoveListener(UpdateMoneyUI);
            GameManager.Instance.OnLevelChanged.RemoveListener(UpdateLevelUI);
            GameManager.Instance.OnExperienceChanged.RemoveListener(UpdateExpUI);
        }
    }
}