using UnityEngine;

using System.Collections.Generic;

using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour

{

    // [싱글톤 패턴] 전역에서 접근 가능한 유일한 인스턴스

    public static GameManager Instance;

    [Header("현재 파견 정보")]

    public Party currentDispatchParty; // 누가 갔는가

    public QuestData currentQuest; // 어떤 퀘스트인가

    [Header("프리팹 연결 (순서 중요!)")]
    // ★ 이 줄이 없어서 에러가 났던 겁니다. 추가해주세요!
    public GameObject[] unitPrefabs; // 0:전사, 1:궁수, 2:마법사, 3:도적, 4:힐러



    [Header("길드 데이터")]

    public string guildName = "용감한 모험가들"; // 길드 이름

    public int gold = 1000;          // 초기 자금

    public int reputation = 0;       // 명성

    public int day = 1;              // 날짜



    [Header("건물 정보")]

    public int guildLevel = 0; // 0: 텐트/판잣집, 1: 목조 건물, 2: 석조 건물 ...



    public List<Adventurer> adventurers = new List<Adventurer>(); //모험가 명단



   



    [Header("파티 데이터")]

    public List<Party> partyList = new List<Party>();

    [Header("길드 인벤토리")]
    public Dictionary<string, int> guildInventory = new Dictionary<string, int>();

    [Header("결산 데이터 (로비 전달용)")]
    public bool hasPendingResult = false; // 보여줄 결산 창이 있는가?
    public bool lastBattleWon = false;    // 승리했는가? (도주/패배는 false)
    public Dictionary<string, int> lastEarnedLoot = new Dictionary<string, int>(); // 방금 던전에서 얻은 템들





    private void Awake()

    {

        // 싱글톤 보장 로직

        if (Instance == null)

        {

            Instance = this;

            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음

        }

        else

        {

            Destroy(gameObject); // 중복 생성 방지

        }

    }



    public void StartQuest(Party party, QuestData quest) //퀘스트 파견 시작 함수

    {

        // 1. 짐 챙기기 (데이터 저장)

        currentDispatchParty = party;

        currentQuest = quest;



        // 2. 파티 상태 변경

        party.state = PartyState.OnQuest;



        Debug.Log($"🚀 '{party.partyName}' 파티가 '{quest.questName}' 의뢰(적: {quest.enemyPrefab.name})를 수행하러 갑니다!");



        // 3. 던전 씬 로딩

        // (Build Settings에 'Dungeon' 씬이 등록되어 있어야 합니다!)

        SceneManager.LoadScene("DungeonScene");

    }

    // 직업(JobType)을 주면 해당 프리팹을 뱉어내는 함수
    public GameObject GetUnitPrefab(JobType job)
    {
        // unitPrefabs 배열 순서: 0:전사, 1:궁수, 2:마법사, 3:도적, 4:힐러... 라고 가정
        int index = (int)job;
        if (index >= 0 && index < unitPrefabs.Length)
        {
            return unitPrefabs[index];
        }
        return unitPrefabs[0]; // 없으면 기본값(전사) 리턴
    }



    // 테스트용 함수

    public void AddGold(int amount)

    {

        gold += amount;

        Debug.Log($"[재정] 현재 골드: {gold} G");

    }

    // ★ 전투가 끝나고 BattleManager가 전리품을 싸들고 와서 보고하는 함수
    public void AddLootToGuild(List<MonsterDropData> dungeonLoot, bool isWin)
    {
        lastEarnedLoot.Clear(); // 이전 결산 기록 지우기

        foreach (MonsterDropData item in dungeonLoot)
        {
            // MonsterDropData 안에 있는 아이템 이름 가져오기
            string itemName = item.itemName; 

            // 1. 길드 창고에 아이템 쑤셔넣기 (있으면 +1, 없으면 새로 등록)
            if (guildInventory.ContainsKey(itemName))
                guildInventory[itemName]++;
            else
                guildInventory.Add(itemName, 1);

            // 2. 결산 창에 띄워주기 위해 임시 바구니에도 담기
            if (lastEarnedLoot.ContainsKey(itemName))
                lastEarnedLoot[itemName]++;
            else
                lastEarnedLoot.Add(itemName, 1);
        }

        lastBattleWon = isWin;
        hasPendingResult = true; // "로비야, 결산 창 띄울 준비 해!"
        
        Debug.Log($"📦 길드 창고 저장 완료! (총 {dungeonLoot.Count}개의 전리품을 정리했습니다)");
    }



   

}

