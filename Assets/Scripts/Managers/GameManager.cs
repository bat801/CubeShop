using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Начальные значения")]
    [SerializeField] private int startingMoney = 1000;
    [SerializeField] private int startingLevel = 1;

    // Текущие значения
    private int currentMoney;
    private int currentExperience;
    private int currentLevel;

    // События для UI
    public UnityEvent<int> OnMoneyChanged = new UnityEvent<int>();
    public UnityEvent<int> OnLevelChanged = new UnityEvent<int>();
    public UnityEvent<float> OnExperienceChanged = new UnityEvent<float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Initialize();
    }

    private void Initialize()
    {
        currentMoney = startingMoney;
        currentLevel = startingLevel;
        currentExperience = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        OnLevelChanged?.Invoke(currentLevel);
    }

    public int GetMoney() => currentMoney;
    public int GetLevel() => currentLevel;
    public int GetExperience() => currentExperience;

    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount)) return false;

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        int expForNextLevel = GetExpForLevel(currentLevel + 1);

        if (currentExperience >= expForNextLevel)
        {
            // Повышение уровня
            currentLevel++;
            currentExperience -= expForNextLevel;
            OnLevelChanged?.Invoke(currentLevel);
            OnExperienceChanged?.Invoke(0f);

            // Открываем новые предметы (будет реализовано позже)
            Debug.Log($"Поздравляем! Вы достигли {currentLevel} уровня!");
        }
        else
        {
            float progress = (float)currentExperience / expForNextLevel;
            OnExperienceChanged?.Invoke(progress);
        }
    }

    private int GetExpForLevel(int level)
    {
        // Формула: 50 * уровень + 30 * (уровень^1.5)
        return Mathf.FloorToInt(50 * level + 30 * Mathf.Pow(level, 1.5f));
    }

    // Для отладки
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMoney(100);
            Debug.Log($"Добавлено 100 монет. Текущий баланс: {currentMoney}");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddExperience(10);
            Debug.Log($"Добавлено 10 опыта. Текущий опыт: {currentExperience}");
        }
    }
}