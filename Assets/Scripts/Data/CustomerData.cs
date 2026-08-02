using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCustomer", menuName = "CubeShop/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Основное")]
    public string customerName;
    public Sprite avatar;

    [Header("Поведение")]
    public float walkSpeed = 2f;
    public float patienceDuration = 30f;
    public float patienceLossMultiplier = 1f;

    [Header("Экономика")]
    public int moneyRewardMin = 10;
    public int moneyRewardMax = 25;
    public int experienceReward = 5;

    [Header("Специальные свойства")]
    public float stealChance = 0.05f; // Шанс украсть
    public float trashSpawnChance = 0.3f; // Шанс оставить мусор

    [Header("Заказы")]
    public List<ItemData> possibleOrders; // Что может заказать
}