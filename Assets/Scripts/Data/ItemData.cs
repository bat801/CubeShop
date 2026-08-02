using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "CubeShop/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Основная информация")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    public string description;

    [Header("Экономика")]
    public int buyPrice = 50;
    public int sellPrice = 10;

    [Header("Размер")]
    public Vector2Int gridSize = Vector2Int.one; // 1x1, 2x1 и т.д.
    public float height = 0.5f;

    [Header("Тип предмета")]
    public ItemType itemType;
    public bool isFurniture = true;
    public bool isStackable = false;

    [Header("Производство (для станков)")]
    public float craftTime = 0f;
    public List<ItemData> recipeIngredients;
    public ItemData craftResult;

    [Header("Визуализация (для прототипа)")]
    public Color prototypeColor = Color.blue;
}

public enum ItemType
{
    Furniture,      // Мебель (столы, стулья)
    Equipment,      // Оборудование (кофемашина, холодильник)
    Ingredient,     // Ингредиент (молоко, сахар)
    Product,        // Готовый товар (чай, кофе)
    Decoration,     // Декор (для будущих версий)
    Tool            // Инструмент (касса)
}