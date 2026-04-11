using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // 싱글톤

    [Header("던전 구성")]
    public List<StageData> dungeonStages; // 스테이지 데이터 리스트
    private int currentStageIndex = 0;
    
    [Header("스폰 위치")]
    public Transform playerSpawnPoint; // 아군 시작 위치 (기본)
    public Transform enemySpawnPoint;  // 적군 시작 위치 (오른쪽)

    [Header("상태 정보")]
    public bool isBattleActive = false;
    public int allyCount = 0;  // 현재 생존 아군 수
    public int enemyCount = 0; // 현재 생존 적군 수

    [Header("UI 패널 연결")]
    public GameObject victoryPanel; // 승리 패널
    public GameObject defeatPanel;  // 패배 패널

    [Header("아군 프리팹 연결 (직업별)")]
    public GameObject warriorPrefab; 
    public GameObject archerPrefab;  
    public GameObject magePrefab;    
    public GameObject roguePrefab;
    public GameObject healerPrefab;

    [Header("던전 전리품")]
    public List<MonsterDropData> earnedItems = new List<MonsterDropData>(); 

    [Header("던전 오브젝트")]
    public GameObject chestPrefab; // 보물상자 프리팹

    [Header("아군 스폰 위치 설정")]
    public Transform[] frontSpawnPoints; // 전위 스폰 좌표들 (앞줄)
    public Transform[] backSpawnPoints;  // 후위 스폰 좌표들 (뒷줄)

    [Header("던전 진행")]
    private bool isRoomCleared = false; // 방 클리어 여부

    // (에러 방지용)
    public GameObject damageTextPrefab; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. 아군 소환 (직업별 프리팹 사용)
        SetupAllyParty();

        // 2. 퀘스트에 스테이지가 있으면 인스펙터 값보다 우선 사용
        if (GameManager.Instance != null &&
            GameManager.Instance.currentQuest != null &&
            GameManager.Instance.currentQuest.stages != null &&
            GameManager.Instance.currentQuest.stages.Count > 0)
        {
            dungeonStages = GameManager.Instance.currentQuest.stages;
        }

        // 3. 던전 스테이지 시작 (0번부터)
        if (dungeonStages != null && dungeonStages.Count > 0)
        {
            StartCoroutine(StartStageCoroutine(0));
        }
        else
        {
            Debug.LogError("🚨 던전 스테이지 데이터(StageData)가 비어있습니다!");
        }
    }

    // --- 1. 아군 소환 로직 ---
    void SetupAllyParty()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentDispatchParty == null) 
        {
            Debug.LogWarning("게임 매니저나 파티 정보가 없습니다. 테스트 모드일 수 있습니다.");
            return;
        }

        List<Adventurer> members = GameManager.Instance.currentDispatchParty.members;

        int frontIndex = 0;
        int backIndex = 0;
        allyCount = 0;

        foreach (Adventurer member in members)
        {
            GameObject prefabToSpawn = null;
            bool isFrontLine = false;

            switch (member.job)
            {
                case JobType.Warrior: prefabToSpawn = warriorPrefab; isFrontLine = true; break;
                case JobType.Rogue:   prefabToSpawn = roguePrefab; isFrontLine = true; break;
                case JobType.Archer:  prefabToSpawn = archerPrefab; isFrontLine = false; break;
                case JobType.Mage:    prefabToSpawn = magePrefab; isFrontLine = false; break;
                case JobType.Healer:  prefabToSpawn = healerPrefab; isFrontLine = false; break;
                default:              prefabToSpawn = warriorPrefab; isFrontLine = true; break;
            }

            if (prefabToSpawn == null) continue;

            GameObject newUnit = Instantiate(prefabToSpawn);
            newUnit.name = $"Unit_{member.name}";
            newUnit.tag = "Player";

            Transform targetSpawnPoint = null;
            if (isFrontLine && frontSpawnPoints != null && frontIndex < frontSpawnPoints.Length)
            {
                targetSpawnPoint = frontSpawnPoints[frontIndex++];
            }
            else if (!isFrontLine && backSpawnPoints != null && backIndex < backSpawnPoints.Length)
            {
                targetSpawnPoint = backSpawnPoints[backIndex++];
            }
            else
            {
                targetSpawnPoint = playerSpawnPoint; 
            }

            float randomX = Random.Range(-0.2f, 0.2f);
            float randomY = Random.Range(-0.2f, 0.2f);
            if (targetSpawnPoint != null)
                newUnit.transform.position = targetSpawnPoint.position + new Vector3(randomX, randomY, 0);

            BattleUnit unitScript = newUnit.GetComponent<BattleUnit>();
            if (unitScript != null) unitScript.Initialize(member); 

            allyCount++;
        }
        Debug.Log($"⚔️ 아군 {allyCount}명 소환 완료!");
    }

    // --- 2. 스테이지 진행 코루틴 ---
    IEnumerator StartStageCoroutine(int stageIndex)
    {
        currentStageIndex = stageIndex;
        
        if (currentStageIndex >= dungeonStages.Count)
        {
            Debug.Log("🎉 던전 클리어!");
            GameOver(true);
            yield break;
        }

        Debug.Log($"⚔️ 스테이지 {stageIndex + 1} 시작");
        yield return new WaitForSeconds(2.0f);

        ResetAllyPositions();
        SpawnEnemies(dungeonStages[stageIndex]);
        isBattleActive = true;
    }

    void SpawnEnemies(StageData stageData)
    {
        if (stageData == null) return;

        List<GameObject> enemies = stageData.enemyPrefabs;
        float spacing = 1.5f;
        enemyCount = enemies.Count;

        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject prefab = enemies[i];
            if (prefab == null) continue;

            Vector3 pos = enemySpawnPoint.position + new Vector3(i * spacing, 0, 0);
            GameObject newEnemy = Instantiate(prefab, pos, Quaternion.identity);
            newEnemy.tag = "Enemy";
            newEnemy.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    // --- 3. 유닛 사망 처리 ---
    public void OnUnitDead(bool isPlayerSide)
    {
        if (!isBattleActive) return;

        if (isPlayerSide)
        {
            allyCount--;
            if (allyCount <= 0)
            {
                isBattleActive = false;
                GameOver(false);
            }
        }
        else
        {
            enemyCount--;
            if (enemyCount <= 0 && !isRoomCleared)
            {
                isRoomCleared = true;
                isBattleActive = false;
                SpawnTreasureChest();
            }
        }
    }

    void SpawnTreasureChest()
    {
        Vector3 chestSpawnPos = new Vector3(4.0f, 0f, 0f); 
        GameObject chest = Instantiate(chestPrefab, chestSpawnPos, Quaternion.identity);
        OrderToLoot(chest.transform);
    }

    public void AddItemToLootBag(MonsterDropData item)
    {
        if (item != null)
        {
            earnedItems.Add(item);
            Debug.Log($"💎 전리품 획득: {item.itemName}");
        }
    }

    public void GoToNextStage()
    {
        currentStageIndex++;
        if (currentStageIndex < dungeonStages.Count) 
        {
            StartCoroutine(StartStageCoroutine(currentStageIndex)); 
        }
        else
        {
            GameOver(true);
        }
    }

    // --- 4. 게임 종료 및 UI ---
    public void GameOver(bool isWin)
    {
        if (isWin) { if (victoryPanel != null) victoryPanel.SetActive(true); }
        else       { if (defeatPanel != null) defeatPanel.SetActive(true); }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLootToGuild(earnedItems, isWin); 
        }
    }

    // --- 5. 로비로 돌아가기 ---
    public void OnClick_ReturnToLobby()
    {
        if (GameManager.Instance != null)
        {
            // 1. 상태 동기화
            SyncAdventurerStatus();

            // 2. 파티 및 퀘스트 정산
            if (GameManager.Instance.currentDispatchParty != null)
            {
                GameManager.Instance.currentDispatchParty.state = PartyState.Idle;

                int reward = (GameManager.Instance.currentQuest != null) ? GameManager.Instance.currentQuest.rewardGold : 0;
                GameManager.Instance.AddQuestRewards(reward, reward / 10, true);

                // 구출 퀘스트 승리 시 → 낙오 모험가 구출 처리
                if (GameManager.Instance.currentQuest != null && GameManager.Instance.currentQuest.isRescueQuest)
                    GameManager.Instance.CompleteRescueQuest(GameManager.Instance.currentQuest);

                // 유해 수습 퀘스트 승리 시 → 유해 수습 보상 지급
                if (GameManager.Instance.currentQuest != null && GameManager.Instance.currentQuest.isCorpseQuest)
                    GameManager.Instance.CompleteCorpseQuest(GameManager.Instance.currentQuest);

                // 승리 시 생환 파티원 경험치 지급
                string rank = GameManager.Instance.currentQuest != null
                    ? GameManager.Instance.currentQuest.rank : "D";
                GameManager.Instance.GainExpAfterBattle(
                    GameManager.Instance.currentDispatchParty.members, rank);

                GameManager.Instance.currentDispatchParty = null;
                GameManager.Instance.CleanupCurrentQuest();
            }
        }
        SceneManager.LoadScene("LobbyScene");
    }

    private void CheckRoomClear()
    {
        if (isRoomCleared) return;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            isRoomCleared = true;
            SpawnTreasureChest();
        }
    }

    private void OrderToLoot(Transform chestTransform)
    {
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Player");
        if (allies.Length > 0)
        {
            BattleUnit looter = allies[0].GetComponent<BattleUnit>();
            if (looter != null) looter.CommandLoot(chestTransform);
        }
    }

    public void ShowDamageText(Vector3 pos, float damage, bool crit) { }
    public void ShowHealText(Vector3 pos, float amount) { }

    public void OnClickFleeButton()
    {
        if (!isBattleActive) return;

        isBattleActive = false;

        GameObject[] allies = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in allies)
        {
            BattleUnit u = obj.GetComponent<BattleUnit>();
            if (u != null) u.enabled = false;
        }

        StartCoroutine(EscapeToLobby());
    }

    private IEnumerator EscapeToLobby()
    {
        yield return new WaitForSeconds(1.0f);

        if (GameManager.Instance != null && GameManager.Instance.currentDispatchParty != null)
        {
            ApplyFleeConsequences();
            GameManager.Instance.AddLootToGuild(earnedItems, false);
        }

        SceneManager.LoadScene("LobbyScene");
    }

    // 도주 시 낙오 + 사기 하락 처리
    private void ApplyFleeConsequences()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentDispatchParty == null) return;

        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag("Player");
        List<Adventurer> strandedList  = new List<Adventurer>();
        List<Adventurer> returnedList  = new List<Adventurer>();
        List<Adventurer> deadList      = new List<Adventurer>();

        foreach (Adventurer member in GameManager.Instance.currentDispatchParty.members)
        {
            string unitName = $"Unit_{member.name}";
            BattleUnit unitScript = null;
            foreach (GameObject obj in allyObjects)
            {
                if (obj.name == unitName) { unitScript = obj.GetComponent<BattleUnit>(); break; }
            }

            // 전사 판정
            bool isDead    = (unitScript == null || unitScript.isDead);
            // 부상 판정: 살아있지만 HP가 최대 미만
            bool isInjured = !isDead && (unitScript.currentHp < unitScript.maxHp);

            if (isDead)
            {
                member.isDead     = true;
                member.currentHp  = 0;
                deadList.Add(member);
            }
            else if (isInjured)
            {
                // 부상자 → 낙오
                member.currentHp              = (int)unitScript.currentHp;
                member.isStranded             = true;
                member.strandedWeeksRemaining = 4; // 4주 안에 구출하지 못하면 사망
                member.assignedPartyIndex     = -1;
                strandedList.Add(member);
                Debug.Log($"😨 [낙오] {member.name}이(가) 부상 상태로 던전에 남겨졌습니다. (구출 기한: 4주)");
            }
            else
            {
                // 건강하게 복귀
                member.currentHp = (int)unitScript.currentHp;
                returnedList.Add(member);
            }
        }

        // 사기 하락: 복귀 파티원 전체 능력치 10~20% 랜덤 하락
        if (returnedList.Count > 0)
        {
            float penalty = Random.Range(0.10f, 0.20f);
            foreach (Adventurer adv in returnedList)
            {
                adv.moralePenalty = penalty;
                Debug.Log($"😞 [사기 하락] {adv.name} - 능력치 {penalty * 100f:F0}% 감소 (다음 파견 적용)");
            }
        }

        // 전사자: 파티 & 모험가 명단에서 제거, 유해 수습 목록에 보존
        string questName = GameManager.Instance.currentQuest != null
            ? GameManager.Instance.currentQuest.questName : "알 수 없는 던전";
        foreach (Adventurer dead in deadList)
        {
            dead.diedAtQuestName = questName;
            GameManager.Instance.currentDispatchParty.members.Remove(dead);
            GameManager.Instance.adventurers.Remove(dead);
            if (!GameManager.Instance.deadAdventurers.Contains(dead))
                GameManager.Instance.deadAdventurers.Add(dead);
            Debug.Log($"💀 [전사] {dead.name}이(가) '{questName}'에서 사망했습니다. (유해 수습 가능)");
        }

        // 낙오자: 파티에서만 제거 (adventurers 명단은 유지 → 나중에 구출 가능)
        foreach (Adventurer stranded in strandedList)
        {
            GameManager.Instance.currentDispatchParty.members.Remove(stranded);
        }

        GameManager.Instance.currentDispatchParty.state = PartyState.Idle;
        GameManager.Instance.currentDispatchParty = null;
        GameManager.Instance.CleanupCurrentQuest();
    }

    public void OnChestOpened()
    {
        CancelInvoke("CheckRoomClear"); 
        isRoomCleared = false; 
        currentStageIndex++; 
        StartCoroutine(StartStageCoroutine(currentStageIndex)); 
    }

    private void ResetAllyPositions()
    {
        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag("Player");
        List<BattleUnit> survivingAllies = new List<BattleUnit>();

        foreach (GameObject obj in allyObjects)
        {
            BattleUnit unit = obj.GetComponent<BattleUnit>();
            if (unit != null && !unit.isDead)
            {
                survivingAllies.Add(unit);
                unit.ResetState();
            }
        }

        survivingAllies.Sort((a, b) => b.maxHp.CompareTo(a.maxHp));

        int fIdx = 0; int bIdx = 0;
        foreach (BattleUnit unit in survivingAllies)
        {
            Transform tp = null;
            if (frontSpawnPoints != null && fIdx < frontSpawnPoints.Length) tp = frontSpawnPoints[fIdx++];
            else if (backSpawnPoints != null && bIdx < backSpawnPoints.Length) tp = backSpawnPoints[bIdx++];

            if (tp != null)
                unit.transform.position = tp.position + new Vector3(Random.Range(-0.2f,0.2f), Random.Range(-0.2f,0.2f), 0);
        }
    }

    private void SyncAdventurerStatus()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentDispatchParty == null) return;

        List<Adventurer> deadAdventurers = new List<Adventurer>();
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Player");

        foreach (Adventurer member in GameManager.Instance.currentDispatchParty.members)
        {
            string unitName = $"Unit_{member.name}";
            BattleUnit unitScript = null;
            foreach(GameObject obj in allies) { if(obj.name == unitName) { unitScript = obj.GetComponent<BattleUnit>(); break; } }

            if (unitScript != null)
            {
                member.currentHp = (int)unitScript.currentHp; 
                member.isDead = unitScript.isDead; 
            }
            else { member.currentHp = 0; member.isDead = true; }

            if (member.isDead || member.currentHp <= 0) deadAdventurers.Add(member);
            else if (member.currentHp < member.hp)
            {
                float hpPercent = (float)member.currentHp / member.hp;
                
                // [중상 처리: 체력 30% 이하]
                if (hpPercent <= 0.3f)
                {
                    member.recoveryWeeks = 2;
                    
                    // ★ 트라우마(부정적 특성) 부여 확률 체크
                    if (Random.Range(0, 100) < GameManager.Instance.traumaChancePercentage)
                    {
                        TraitData trauma = GameManager.Instance.GetRandomTrauma();
                        if (trauma != null && !member.traits.Contains(trauma))
                        {
                            member.traits.Add(trauma);
                            Debug.Log($"😱 [트라우마 발생] {member.name}님이 '{trauma.traitName}' 특성을 얻었습니다.");
                        }
                    }
                    else
                    {
                        Debug.Log($"😌 [중상] {member.name}님이 중상을 입었지만 트라우마는 발생하지 않았습니다.");
                    }
                }
                else
                {
                    member.recoveryWeeks = 1;
                }
                
                member.assignedPartyIndex = -1; 
            }
        }
        
        string questNameForDead = GameManager.Instance.currentQuest != null
            ? GameManager.Instance.currentQuest.questName : "알 수 없는 던전";
        foreach (Adventurer deadGuy in deadAdventurers)
        {
            deadGuy.diedAtQuestName = questNameForDead;
            GameManager.Instance.currentDispatchParty.members.Remove(deadGuy);
            if (GameManager.Instance.adventurers.Contains(deadGuy))
                GameManager.Instance.adventurers.Remove(deadGuy);
            if (!GameManager.Instance.deadAdventurers.Contains(deadGuy))
                GameManager.Instance.deadAdventurers.Add(deadGuy);
            Debug.Log($"💀 [전사] {deadGuy.name}이(가) '{questNameForDead}'에서 사망했습니다. (유해 수습 가능)");
        }
    }
}