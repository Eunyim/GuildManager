using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestBoardPopup : MonoBehaviour
{
    [Header("퀘스트 생성기 (연결 필수)")]
    public QuestGenerator questGenerator;
    public int questSlotCount = 5; // 한 번에 표시할 의뢰 수

    [Header("(레거시) 수동 퀘스트 목록 — Generator 미연결 시 사용")]
    public List<QuestData> availableQuests;

    [Header("UI 연결")]
    public GameObject questSlotPrefab;
    public Transform contentArea;

    // 현재 활성 퀘스트 목록 (generator로 만든 임시 인스턴스 포함)
    private List<QuestData> _activeQuests = new List<QuestData>();

    // 팝업이 열릴 때마다 퀘스트를 새로 생성
    void OnEnable()
    {
        GenerateAndRefresh();
    }

    // 팝업이 닫힐 때 생성된 퀘스트 인스턴스 정리 (파견 선택된 것만 보존)
    void OnDisable()
    {
        CleanupQuests();
    }

    // [새로고침] 버튼용 (UI에서 연결 가능)
    public void OnClickRefresh()
    {
        GenerateAndRefresh();
    }

    // ─── 생성 + 목록 갱신 ────────────────────────────────────────────

    void GenerateAndRefresh()
    {
        CleanupQuests();

        // 1. 구출 퀘스트: 낙오자 발생 시 퀘스트 생성 + 항상 맨 위에 표시
        if (GameManager.Instance != null)
        {
            // 구출 퀘스트가 없는 낙오자에게 새로 생성
            if (questGenerator != null)
            {
                foreach (Adventurer adv in GameManager.Instance.adventurers)
                {
                    if (!adv.isStranded) continue;
                    bool alreadyExists = GameManager.Instance.rescueQuests.Exists(q => q.rescueTargetName == adv.name);
                    if (alreadyExists) continue;

                    QuestData rq = questGenerator.GenerateRescueQuest(adv);
                    if (rq != null)
                    {
                        GameManager.Instance.rescueQuests.Add(rq);
                        Debug.Log($"🆘 [구출 퀘스트 생성] {adv.name} 구출 작전 의뢰가 게시됐습니다.");
                    }
                }
            }

            // 구출 퀘스트를 목록 최상단에 추가 (GameManager가 수명 관리)
            foreach (QuestData rq in GameManager.Instance.rescueQuests)
                if (rq != null) _activeQuests.Add(rq);
        }

        // 2. 일반 랜덤 퀘스트 생성
        if (questGenerator != null)
        {
            int rep = GameManager.Instance != null ? GameManager.Instance.reputation : 0;
            for (int i = 0; i < questSlotCount; i++)
            {
                RankType rank   = RollRank(rep);
                QuestData quest = questGenerator.Generate(rank);
                if (quest != null) _activeQuests.Add(quest);
            }
        }
        else
        {
            _activeQuests.AddRange(availableQuests);
        }

        RefreshQuestList();
    }

    void CleanupQuests()
    {
        QuestData selected = GameManager.Instance != null ? GameManager.Instance.currentQuest : null;

        foreach (QuestData q in _activeQuests)
        {
            if (q == null || q == selected) continue;

            // 구출 퀘스트는 GameManager가 수명 관리 → 여기서 파괴하지 않음
            if (q.isRescueQuest) continue;

            // 수동 Asset도 파괴하지 않음
            if (questGenerator == null) continue;

            if (q.stages != null)
                foreach (StageData s in q.stages)
                    if (s != null) Destroy(s);
            Destroy(q);
        }
        _activeQuests.Clear();
    }

    void RefreshQuestList()
    {
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        foreach (QuestData quest in _activeQuests)
        {
            if (quest == null) continue;

            GameObject slot = Instantiate(questSlotPrefab, contentArea);
            TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();

            if (quest.isRescueQuest)
            {
                // 구출 퀘스트: 남은 주 수를 강조 표시
                Adventurer target = GameManager.Instance?.adventurers.Find(a => a.name == quest.rescueTargetName);
                int weeksLeft     = target != null ? target.strandedWeeksRemaining : 0;
                string urgency    = weeksLeft <= 1 ? "<color=red>" : "<color=yellow>";

                if (texts.Length > 0) texts[0].text = quest.questName;
                if (texts.Length > 1) texts[1].text = $"{quest.rewardGold:N0} G";
                if (texts.Length > 2) texts[2].text = $"{urgency}⚠ {weeksLeft}주 남음</color>";
                if (texts.Length > 3) texts[3].text = $"[{quest.rank}] 권장 Lv.{quest.recommendedLevel}";
            }
            else
            {
                if (texts.Length > 0) texts[0].text = quest.questName;
                if (texts.Length > 1) texts[1].text = $"{quest.rewardGold:N0} G";
                if (texts.Length > 2) texts[2].text = $"[{quest.rank}]";
                if (texts.Length > 3) texts[3].text = $"[{quest.rank}] 권장 Lv.{quest.recommendedLevel}";
            }

            Button btn = slot.GetComponentInChildren<Button>();
            if (btn != null)
            {
                QuestData captured = quest;
                btn.onClick.AddListener(() => OnClickQuest(captured));
            }
        }
    }

    void OnClickQuest(QuestData quest)
    {
        Debug.Log($"의뢰 선택: {quest.questName}");
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OpenDispatchPartyPopup(quest);
    }

    // ─── 명성 기반 등급 결정 (GDD §5.3) ────────────────────────────

    RankType RollRank(int reputation)
    {
        float roll = Random.value;

        if (reputation >= 100) // 고명성 길드
        {
            if (roll < 0.15f) return RankType.S;
            if (roll < 0.45f) return RankType.A;
            if (roll < 0.70f) return RankType.B;
            if (roll < 0.90f) return RankType.C;
            return RankType.D;
        }
        if (reputation >= 50)
        {
            if (roll < 0.05f) return RankType.S;
            if (roll < 0.25f) return RankType.A;
            if (roll < 0.55f) return RankType.B;
            if (roll < 0.80f) return RankType.C;
            return RankType.D;
        }
        if (reputation >= 20)
        {
            if (roll < 0.30f) return RankType.B;
            if (roll < 0.65f) return RankType.C;
            return RankType.D;
        }

        // 초보 길드: D/C만 등장
        return (roll < 0.40f) ? RankType.C : RankType.D;
    }
}
