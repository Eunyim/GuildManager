using UnityEngine;

[System.Serializable]
public struct DropItem
{
    public MonsterDropData item;      // 떨굴 아이템
    [Range(0, 100)] 
    public float dropRate;     // 확률 (0~100%)
}
