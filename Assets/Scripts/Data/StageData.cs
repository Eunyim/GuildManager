using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Stage", menuName = "Dungeon/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public string stageName = "1층 복도";
    public Sprite background; // (나중에 배경 바꿀 때 사용)

    [Header("출현 몬스터")]
    public List<GameObject> enemyPrefabs; // 이 방에서 나올 적들 리스트
}