using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InteractableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Основные данные")]
    [SerializeField] protected ItemData data;
    [SerializeField] protected bool isBusy = false;
    [SerializeField] protected bool isReady = false;
    [SerializeField] protected float currentTimer = 0f;

    [Header("Визуальные эффекты")]
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color highlightColor = Color.yellow;
    [SerializeField] protected Color readyColor = Color.green;

    protected Renderer[] renderers;
    protected Material[] originalMaterials;

    protected virtual void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }
        }
    }

    // Базовый метод взаимодействия (вызывается при клике)
    public virtual void OnInteract(InventoryManager inventory)
    {
        Debug.Log($"Взаимодействие с {gameObject.name}");
    }

    // Наведение мыши
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (isReady)
        {
            SetHighlightColor(highlightColor);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        ResetColor();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnInteract(InventoryManager.Instance);
        }
    }

    // Обновление цвета при готовности
    protected virtual void SetReadyState(bool ready)
    {
        isReady = ready;
        if (ready)
        {
            SetHighlightColor(readyColor);
        }
        else
        {
            ResetColor();
        }
    }

    protected virtual void SetHighlightColor(Color color)
    {
        if (renderers == null) return;
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    protected virtual void ResetColor()
    {
        if (renderers == null || originalMaterials == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length)
            {
                renderers[i].material = originalMaterials[i];
            }
        }
    }

    // Процесс производства (корутина)
    protected virtual System.Collections.IEnumerator CraftCoroutine(float craftTime)
    {
        isBusy = true;
        isReady = false;
        currentTimer = craftTime;

        while (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            // Обновляем индикатор (будет реализовано позже)
            yield return null;
        }

        isReady = true;
        isBusy = false;
        SetReadyState(true);
        Debug.Log($"{data.displayName} готов!");
    }

    public bool IsReady() => isReady;
    public bool IsBusy() => isBusy;
    public ItemData GetData() => data;
}