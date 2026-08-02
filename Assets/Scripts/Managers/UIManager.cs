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
    [SerializeField] private GameObject buildPanel; // Панель строительства

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

        // Инициализация UI
        UpdateMoneyUI(GameManager.Instance.GetMoney());
        UpdateLevelUI(GameManager.Instance.GetLevel());
        UpdateExpUI(0f);
        ClearInventorySlot();
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
        bool isActive = buildPanel != null ? !buildPanel.activeSelf : false;
        if (buildPanel != null)
            buildPanel.SetActive(isActive);

        // Здесь будем вызывать PlacementManager
        Debug.Log($"Build Mode: {(isActive ? "ON" : "OFF")}");
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