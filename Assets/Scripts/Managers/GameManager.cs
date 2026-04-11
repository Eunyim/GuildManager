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

    [Header("트라우마 데이터")]
    public List<TraitData> traumaPool = new List<TraitData>();
    public int traumaChancePercentage = 50; // 트라우마 발생 확률 (0-100%)

    [Header("긍정 특성 풀 (레벨업 보상)")]
    public List<TraitData> positiveTraitPool = new List<TraitData>();

    [Header("레벨업 로그 (결과창 전달용)")]
    public List<string> levelUpLog = new List<string>();

    public TraitData GetRandomTrauma()
    {
        if (traumaPool == null || traumaPool.Count == 0) return null;
        return traumaPool[Random.Range(0, traumaPool.Count)];
    }

    [Header("구출 퀘스트 목록")]
    public List<QuestData> rescueQuests = new List<QuestData>(); // 현재 활성 구출 퀘스트

    [Header("전사자 목록 (유해 수습 대상)")]
    public List<Adventurer> deadAdventurers = new List<Adventurer>(); // 유해가 남아있는 전사자
    public List<QuestData> corpseQuests = new List<QuestData>();     // 현재 활성 유해 수습 퀘스트

    [Header("결산 데이터 (로비 전달용)")]
    public bool hasPendingResult = false; // 보여줄 결산 창이 있는가?
    public bool lastBattleWon = false;    // 승리했는가? (도주/패배는 false)
    public Dictionary<string, int> lastEarnedLoot = new Dictionary<string, int>(); // 방금 던전에서 얻은 템들
    
    [Header("시간 및 경제 시스템")]
    public int month = 1;       // 현재 월
    public int week = 1;        // 현재 주차 (1~4주)
    public int weeksPerMonth = 4; // 한 달은 4주(4턴)로 고정




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

    // ★ 전투 후 생환 파티원에게 경험치를 지급하고 레벨업을 처리합니다.
    // BattleManager.OnClick_ReturnToLobby에서 승리 시 호출합니다.
    public void GainExpAfterBattle(List<Adventurer> survivors, string questRank)
    {
        if (survivors == null || survivors.Count == 0) return;

        levelUpLog.Clear();
        int expGain = GetExpByRank(questRank);

        foreach (Adventurer adv in survivors)
        {
            if (adv.isDead || adv.isStranded) continue;

            adv.exp += expGain;
            Debug.Log($"⭐ [{adv.name}] 경험치 +{expGain} (현재: {adv.exp}/{adv.expToNextLevel})");

            // 연속 레벨업 처리 (한 번 전투로 여러 레벨 오를 수 있음)
            while (adv.exp >= adv.expToNextLevel)
            {
                adv.exp -= adv.expToNextLevel;
                LevelUp(adv);
            }
        }
    }

    private void LevelUp(Adventurer adv)
    {
        adv.level++;
        adv.expToNextLevel = adv.level * 100;

        int hpGain  = GetHpGainPerLevel(adv.job, adv.rank);
        int atkGain = GetAtkGainPerLevel(adv.job, adv.rank);
        adv.hp      += hpGain;
        adv.atk     += atkGain;
        adv.currentHp = adv.hp; // 레벨업 시 체력 완전 회복

        string msg = $"{adv.name} Lv.{adv.level - 1} → Lv.{adv.level}! (HP+{hpGain} ATK+{atkGain})";
        levelUpLog.Add(msg);
        Debug.Log($"🎉 [레벨업] {msg}");

        // 3레벨마다 50% 확률로 긍정 특성 획득
        if (adv.level % 3 == 0 && positiveTraitPool.Count > 0)
        {
            if (Random.Range(0, 100) < 50)
            {
                TraitData newTrait = positiveTraitPool[Random.Range(0, positiveTraitPool.Count)];
                if (!adv.traits.Contains(newTrait))
                {
                    adv.traits.Add(newTrait);
                    string traitMsg = $"{adv.name}이(가) 특성 '{newTrait.traitName}' 획득!";
                    levelUpLog.Add(traitMsg);
                    Debug.Log($"✨ [특성 획득] {traitMsg}");
                }
            }
        }
    }

    private int GetExpByRank(string rank)
    {
        switch (rank)
        {
            case "D": return 30;
            case "C": return 60;
            case "B": return 100;
            case "A": return 150;
            case "S": return 200;
            default:  return 30;
        }
    }

    private int GetHpGainPerLevel(JobType job, RankType rank)
    {
        float baseGain;
        switch (job)
        {
            case JobType.Warrior: baseGain = 15f; break;
            case JobType.Healer:  baseGain = 9f;  break;
            case JobType.Archer:  baseGain = 8f;  break;
            case JobType.Mage:    baseGain = 7f;  break;
            case JobType.Rogue:   baseGain = 6f;  break;
            default:              baseGain = 10f; break;
        }
        return Mathf.RoundToInt(baseGain * (1f + (int)rank * 0.2f));
    }

    private int GetAtkGainPerLevel(JobType job, RankType rank)
    {
        float baseGain;
        switch (job)
        {
            case JobType.Mage:    baseGain = 5f; break;
            case JobType.Archer:  baseGain = 4f; break;
            case JobType.Rogue:   baseGain = 4f; break;
            case JobType.Warrior: baseGain = 3f; break;
            case JobType.Healer:  baseGain = 2f; break;
            default:              baseGain = 3f; break;
        }
        return Mathf.RoundToInt(baseGain * (1f + (int)rank * 0.2f));
    }

    // 영혼석으로 전사자를 레벨 1로 부활시킵니다.
    // SpiritStonePopup에서 호출합니다.
    public bool ReviveWithSpiritStone(Adventurer deceased)
    {
        string stoneKey = $"{deceased.name}의 영혼석";
        int goldCost    = GetReviveCost(deceased.rank);

        if (!guildInventory.ContainsKey(stoneKey) || guildInventory[stoneKey] <= 0)
        {
            Debug.LogWarning($"❌ [부활 실패] '{stoneKey}'이(가) 없습니다.");
            return false;
        }
        if (gold < goldCost)
        {
            Debug.LogWarning($"❌ [부활 실패] 골드가 부족합니다. (필요: {goldCost}G, 보유: {gold}G)");
            return false;
        }

        // 자원 소모
        guildInventory[stoneKey]--;
        if (guildInventory[stoneKey] <= 0) guildInventory.Remove(stoneKey);
        gold -= goldCost;

        // 레벨 1로 초기화 후 현역 복귀
        deceased.level            = 1;
        deceased.isDead           = false;
        deceased.isStranded       = false;
        deceased.strandedWeeksRemaining = 0;
        deceased.recoveryWeeks    = 2; // 부활 후 2주 요양
        deceased.moralePenalty    = 0f;
        deceased.assignedPartyIndex = -1;
        deceased.currentHp        = deceased.hp;
        deceased.traits.Clear();   // 트라우마도 초기화

        deadAdventurers.Remove(deceased);
        adventurers.Add(deceased);

        // 해당 유해 수습 퀘스트 제거
        QuestData cq = corpseQuests.Find(q => q.corpseTargetName == deceased.name);
        if (cq != null) DestroyCorpseQuest(cq);

        Debug.Log($"✨ [부활] {deceased.name}이(가) 영혼석의 힘으로 레벨 1로 되살아났습니다! (2주 요양 필요)");
        return true;
    }

    public int GetReviveCost(RankType rank)
    {
        switch (rank)
        {
            case RankType.D: return 200;
            case RankType.C: return 300;
            case RankType.B: return 500;
            case RankType.A: return 800;
            case RankType.S: return 1200;
            default:         return 200;
        }
    }

    // 보유한 모든 영혼석 총 개수 (로비 상단 표시용)
    public int GetSpiritStoneCount()
    {
        int total = 0;
        foreach (var pair in guildInventory)
            if (pair.Key.EndsWith("의 영혼석")) total += pair.Value;
        return total;
    }

    // 특정 모험가의 영혼석 보유 여부
    public bool HasSpiritStone(Adventurer deceased)
    {
        string key = $"{deceased.name}의 영혼석";
        return guildInventory.ContainsKey(key) && guildInventory[key] > 0;
    }

    // 유해 수습 완료: 전사자 유해를 수습하고 장비 or 영혼석을 보상으로 지급합니다.
    // BattleManager에서 승리 직후, CleanupCurrentQuest 전에 호출합니다.
    public void CompleteCorpseQuest(QuestData quest)
    {
        if (quest == null || !quest.isCorpseQuest) return;

        Adventurer deceased = deadAdventurers.Find(a => a.name == quest.corpseTargetName);
        if (deceased != null)
        {
            float roll = Random.value;
            if (roll < 0.2f) // 20% 확률로 해당 모험가의 영혼석 획득
            {
                string stoneKey = $"{deceased.name}의 영혼석";
                if (guildInventory.ContainsKey(stoneKey))
                    guildInventory[stoneKey]++;
                else
                    guildInventory.Add(stoneKey, 1);

                if (lastEarnedLoot.ContainsKey(stoneKey))
                    lastEarnedLoot[stoneKey]++;
                else
                    lastEarnedLoot.Add(stoneKey, 1);

                Debug.Log($"💎 [유해 수습 완료] {deceased.name}의 유해를 수습했습니다. '{stoneKey}' 획득!");
            }
            else // 80% 확률로 유품 (골드 환산)
            {
                int equipGold = 50 + (deceased.level * 20);
                gold += equipGold;

                string equipName = $"{deceased.name}의 유품";
                if (lastEarnedLoot.ContainsKey(equipName))
                    lastEarnedLoot[equipName]++;
                else
                    lastEarnedLoot.Add(equipName, 1);

                Debug.Log($"⚔️ [유해 수습 완료] {deceased.name}의 유해를 수습했습니다. 유품 획득! ({equipGold}G)");
            }

            deadAdventurers.Remove(deceased);
        }

        corpseQuests.Remove(quest);
    }

    // 유해 수습 퀘스트를 목록에서 제거하고 ScriptableObject 인스턴스를 파괴합니다.
    public void DestroyCorpseQuest(QuestData quest)
    {
        if (quest == null) return;
        corpseQuests.Remove(quest);
        if (quest.stages != null)
            foreach (StageData s in quest.stages)
                if (s != null) Destroy(s);
        Destroy(quest);
    }

    // 구출 퀘스트 완료: 낙오 모험가를 구출하고 퀘스트를 목록에서 제거합니다.
    // BattleManager에서 승리 직후, CleanupCurrentQuest 전에 호출합니다.
    public void CompleteRescueQuest(QuestData quest)
    {
        if (quest == null || !quest.isRescueQuest) return;

        Adventurer rescued = adventurers.Find(a => a.name == quest.rescueTargetName && a.isStranded);
        if (rescued != null)
        {
            rescued.isStranded             = false;
            rescued.strandedWeeksRemaining = 0;
            rescued.currentHp              = Mathf.Max(1, rescued.hp / 2); // 생환, 절반 체력
            rescued.recoveryWeeks          = 1;                             // 1주 요양 필요
            rescued.assignedPartyIndex     = -1;
            Debug.Log($"🎉 [구출 완료] {rescued.name}이(가) 무사히 귀환했습니다! (1주 요양 후 파견 가능)");
        }

        rescueQuests.Remove(quest);
        // 인스턴스 파괴는 CleanupCurrentQuest에서 처리
    }

    // 구출 퀘스트를 목록에서 제거하고 ScriptableObject 인스턴스를 파괴합니다.
    private void DestroyRescueQuest(QuestData quest)
    {
        if (quest == null) return;
        rescueQuests.Remove(quest);
        if (quest.stages != null)
            foreach (StageData s in quest.stages)
                if (s != null) Destroy(s);
        Destroy(quest);
    }

    // QuestGenerator로 생성된 런타임 ScriptableObject 인스턴스를 정리합니다.
    // BattleManager에서 퀘스트가 끝날 때 호출하세요.
    public void CleanupCurrentQuest()
    {
        if (currentQuest == null) return;

        if (currentQuest.stages != null)
            foreach (StageData s in currentQuest.stages)
                if (s != null) Destroy(s);

        Destroy(currentQuest);
        currentQuest = null;
    }

    public void AddQuestRewards(int earnedGold, int earnedRep, bool won)
    {
        gold += earnedGold;
        reputation += earnedRep;
        lastBattleWon = won;
        hasPendingResult = true; // 로비에서 결과창을 띄우도록 예약
        
        Debug.Log($"💰 [보상 정산] 골드: +{earnedGold}, 명성: +{earnedRep} (현재 골드: {gold})");
    }

    // ★ [실행 페이즈 -> 정산 페이즈] 1주가 지나는 함수 (로비 결산창에서 확인을 누를 때 실행!)
    public void PassWeek()
    {
        week++;
        Debug.Log($"⏳ 시간이 1주 흘렀습니다. (현재 날짜: {month}월 {week}주차)");

        // 4주차가 넘어가면(즉 5주차가 되면) 다음 달로 넘기고 월급 정산!
        if (week > weeksPerMonth)
        {
            week = 1; 
            month++;  
            
            MonthlySettlement(); // 💸 월급 정산 실행!
        }
        
        // 부상자 회복 처리
        foreach (Adventurer adv in adventurers)
        {
            if (!adv.isDead && adv.recoveryWeeks > 0)
            {
                adv.recoveryWeeks--;
                if (adv.recoveryWeeks <= 0)
                {
                    adv.currentHp     = adv.hp;
                    adv.recoveryWeeks = 0;
                    adv.moralePenalty = 0f;
                    Debug.Log($"💖 [회복 완료] {adv.name}님이 완치되었습니다! (다시 파견 가능)");
                }
            }
        }

        // 낙오 모험가 구출 기한 타이머 처리
        List<Adventurer> expiredList = new List<Adventurer>();
        foreach (Adventurer adv in adventurers)
        {
            if (!adv.isStranded) continue;

            adv.strandedWeeksRemaining--;
            Debug.Log($"⏳ [낙오] {adv.name} - 구출 기한 {adv.strandedWeeksRemaining}주 남음");

            if (adv.strandedWeeksRemaining <= 0)
            {
                Debug.Log($"💀 [사망] {adv.name}이(가) 구출되지 못하고 던전에서 사망했습니다...");
                expiredList.Add(adv);

                // 해당 구출 퀘스트도 제거
                QuestData rq = rescueQuests.Find(q => q.rescueTargetName == adv.name);
                if (rq != null) DestroyRescueQuest(rq);
            }
        }
        foreach (Adventurer dead in expiredList) adventurers.Remove(dead);
    }

    // ★ 월급 정산 (파산의 위험)
    private void MonthlySettlement()
    {
        Debug.Log($"📅 {month}월 1주차가 되었습니다! [길드 유지비 및 월급 정산의 날]");
        int totalSalary = 0;

        foreach (Adventurer adv in adventurers)
        {
            // (기획에 맞춰 랭크/레벨별로 월급을 다르게 줄 수 있습니다. 일단 기본 50G + 레벨당 10G)
            int salary = 50 + (adv.level * 10); 
            totalSalary += salary;
        }

        gold -= totalSalary; // 길드 자금에서 뭉텅이로 차감!
        
        Debug.Log($"💸 모험가 {adventurers.Count}명의 월급으로 총 {totalSalary}G가 지출되었습니다. (남은 골드: {gold}G)");

        if (gold < 0)
        {
            Debug.LogError("🚨 [파산 위기] 길드 자금이 마이너스입니다! 길드가 파산했습니다! (Game Over)");
            // TODO: 나중에 파산 팝업 UI를 띄우고 메인 화면으로 쫓아내는 로직 연결
        }
    }



   

}

