using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Guild/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("의뢰 기본 정보")]
    public string questName;        
    [TextArea] 
    public string description;      
    public string rank;             

    [Header("조건 및 보상")]
    public int rewardGold; //보수
    public int duration; //소요 시간           
    
    //권장 파티 레벨
    public int recommendedLevel;    

    public GameObject enemyPrefab;
}