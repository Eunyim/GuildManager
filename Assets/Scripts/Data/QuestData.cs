using UnityEngine;
using System.Collections.Generic;

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

    [Header("던전 스테이지 구성 (랜덤 생성 시 자동 설정)")]
    public List<StageData> stages = new List<StageData>();

    [Header("구출 퀘스트 전용")]
    public bool isRescueQuest = false;
    public string rescueTargetName = ""; // 구출 대상 모험가 이름

    [Header("유해 수습 퀘스트 전용")]
    public bool isCorpseQuest = false;
    public string corpseTargetName = ""; // 유해 주인 모험가 이름
}