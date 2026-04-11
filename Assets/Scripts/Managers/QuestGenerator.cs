using UnityEngine;
using System.Collections.Generic;

// 퀘스트 랜덤 생성기
// LobbyScene의 QuestBoardPopup 등에 컴포넌트로 추가 후 각 등급 풀에 적 프리팹을 연결합니다.
public class QuestGenerator : MonoBehaviour
{
    [Header("등급별 적 프리팹 풀")]
    public List<GameObject> tierD_Prefabs = new List<GameObject>(); // 슬라임, 고블린
    public List<GameObject> tierC_Prefabs = new List<GameObject>(); // 고블린, 스켈레톤
    public List<GameObject> tierB_Prefabs = new List<GameObject>(); // 스켈레톤, 오크, 도적
    public List<GameObject> tierA_Prefabs = new List<GameObject>(); // 오크, 다크메이지
    public List<GameObject> tierS_Prefabs = new List<GameObject>(); // 최상위 적

    static readonly string[] Verbs     = { "토벌", "소탕", "섬멸", "격퇴", "정화" };
    static readonly string[] Locations = { "소굴", "영역", "출몰지", "군락", "집단" };

    // 퀘스트 1개 생성. 내부적으로 ScriptableObject.CreateInstance 사용 (사용 후 반드시 Destroy 필요)
    public QuestData Generate(RankType rank)
    {
        List<GameObject> pool = GetPool(rank);
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"[QuestGenerator] {rank} 등급 적 풀이 비어있습니다. 인스펙터에서 프리팹을 연결해 주세요.");
            return null;
        }

        GameObject mainEnemy = pool[Random.Range(0, pool.Count)];
        string enemyName     = ToKorean(mainEnemy.name);
        string verb          = Verbs[Random.Range(0, Verbs.Length)];
        string location      = Locations[Random.Range(0, Locations.Length)];

        QuestData quest           = ScriptableObject.CreateInstance<QuestData>();
        quest.rank                = rank.ToString();
        quest.questName           = $"[{rank}] {enemyName} {location} {verb}";
        quest.description         = $"{enemyName}이(가) 출몰하여 피해를 주고 있습니다. 처치해 주세요.";
        quest.rewardGold          = GetReward(rank);
        quest.recommendedLevel    = GetRecLevel(rank);
        quest.enemyPrefab         = mainEnemy;
        quest.stages              = BuildStages(pool, rank);

        return quest;
    }

    // ─── 스테이지 3개 생성 ───────────────────────────────────────────

    List<StageData> BuildStages(List<GameObject> pool, RankType rank)
    {
        var stages = new List<StageData>();
        for (int i = 0; i < 3; i++)
        {
            StageData stage       = ScriptableObject.CreateInstance<StageData>();
            stage.stageName       = $"{i + 1}번 방";
            stage.enemyPrefabs    = new List<GameObject>();

            int count = GetEnemyCount(rank, isBossRoom: i == 2);
            for (int j = 0; j < count; j++)
                stage.enemyPrefabs.Add(pool[Random.Range(0, pool.Count)]);

            stages.Add(stage);
        }
        return stages;
    }

    int GetEnemyCount(RankType rank, bool isBossRoom)
    {
        int bonus = isBossRoom ? 1 : 0;
        switch (rank)
        {
            case RankType.D: return Random.Range(1, 3) + bonus;
            case RankType.C: return Random.Range(2, 4) + bonus;
            case RankType.B: return Random.Range(2, 4) + bonus;
            case RankType.A: return Random.Range(3, 5) + bonus;
            case RankType.S: return Random.Range(3, 6) + bonus;
            default:         return 2 + bonus;
        }
    }

    // ─── 구출 퀘스트 생성 ────────────────────────────────────────────

    // 낙오한 모험가를 위한 긴급 구출 퀘스트를 생성합니다.
    public QuestData GenerateRescueQuest(Adventurer stranded)
    {
        // 모험가 등급에 맞는 풀 사용 (같은 난이도의 적이 있는 던전으로 설정)
        List<GameObject> pool = GetPool(stranded.rank);

        // 등급 풀이 비어있으면 D등급 풀로 대체
        if (pool == null || pool.Count == 0) pool = tierD_Prefabs;
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"[QuestGenerator] 구출 퀘스트 생성 실패: 적 풀이 비어있습니다.");
            return null;
        }

        GameObject mainEnemy  = pool[Random.Range(0, pool.Count)];

        QuestData quest           = ScriptableObject.CreateInstance<QuestData>();
        quest.isRescueQuest       = true;
        quest.rescueTargetName    = stranded.name;
        quest.rank                = stranded.rank.ToString();
        quest.questName           = $"[긴급 구출] {stranded.name} 구출 작전";
        quest.description         = $"{stranded.name}이(가) 던전에 낙오했습니다. 4주 안에 구출하지 못하면 돌아오지 못합니다.";
        quest.rewardGold          = Mathf.RoundToInt(GetReward(stranded.rank) * 1.5f); // 구출 보너스
        quest.recommendedLevel    = GetRecLevel(stranded.rank);
        quest.enemyPrefab         = mainEnemy;
        quest.stages              = BuildStages(pool, stranded.rank);

        return quest;
    }

    // ─── 유해 수습 퀘스트 생성 ───────────────────────────────────────

    // 전사한 모험가의 유해를 수습하는 퀘스트를 생성합니다.
    public QuestData GenerateCorpseQuest(Adventurer deceased)
    {
        List<GameObject> pool = GetPool(deceased.rank);
        if (pool == null || pool.Count == 0) pool = tierD_Prefabs;
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"[QuestGenerator] 유해 수습 퀘스트 생성 실패: 적 풀이 비어있습니다.");
            return null;
        }

        GameObject mainEnemy = pool[Random.Range(0, pool.Count)];

        QuestData quest           = ScriptableObject.CreateInstance<QuestData>();
        quest.isCorpseQuest       = true;
        quest.corpseTargetName    = deceased.name;
        quest.rank                = deceased.rank.ToString();
        string location = string.IsNullOrEmpty(deceased.diedAtQuestName) ? "알 수 없는 던전" : deceased.diedAtQuestName;
        quest.questName           = $"[유해 수습] {deceased.name}의 유해 수습";
        quest.description         = $"{deceased.name}이(가) '{location}'에서 전사했습니다. 유해를 수습하면 유품이나 영혼석을 얻을 수 있습니다.";
        quest.rewardGold          = GetReward(deceased.rank) / 2;
        quest.recommendedLevel    = GetRecLevel(deceased.rank);
        quest.enemyPrefab         = mainEnemy;
        quest.stages              = BuildStages(pool, deceased.rank);

        return quest;
    }

    // ─── 보조 테이블 ─────────────────────────────────────────────────

    List<GameObject> GetPool(RankType rank)
    {
        switch (rank)
        {
            case RankType.D: return tierD_Prefabs;
            case RankType.C: return tierC_Prefabs;
            case RankType.B: return tierB_Prefabs;
            case RankType.A: return tierA_Prefabs;
            case RankType.S: return tierS_Prefabs;
            default:         return tierD_Prefabs;
        }
    }

    int GetReward(RankType rank)
    {
        switch (rank)
        {
            case RankType.D: return Random.Range(100,  201);
            case RankType.C: return Random.Range(200,  401);
            case RankType.B: return Random.Range(400,  701);
            case RankType.A: return Random.Range(700,  1201);
            case RankType.S: return Random.Range(1200, 2001);
            default:         return 100;
        }
    }

    int GetRecLevel(RankType rank)
    {
        switch (rank)
        {
            case RankType.D: return 1;
            case RankType.C: return 2;
            case RankType.B: return 4;
            case RankType.A: return 6;
            case RankType.S: return 8;
            default:         return 1;
        }
    }

    static string ToKorean(string prefabName)
    {
        if (prefabName.Contains("Slime"))    return "슬라임";
        if (prefabName.Contains("Goblin"))   return "고블린";
        if (prefabName.Contains("Skeleton")) return "스켈레톤";
        if (prefabName.Contains("Bandit"))   return "도적단";
        if (prefabName.Contains("Orc"))      return "오크";
        if (prefabName.Contains("DarkMage")) return "다크메이지";
        return prefabName;
    }
}
