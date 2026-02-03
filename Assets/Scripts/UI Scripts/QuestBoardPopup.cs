using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestBoardPopup : MonoBehaviour
{
    [Header("데이터 연결")]
    public List<QuestData> availableQuests; // 인스펙터에서 퀘스트 데이터 드래그해서 넣기
    
    [Header("UI 연결")]
    public GameObject questSlotPrefab; // Slot_Quest 프리팹
    public Transform contentArea;      // Scroll View의 Content

    void Start()
    {
        // 켜지자마자 목록 갱신
        RefreshQuestList();
    }

    // 목록 새로고침 (모험가 목록이랑 로직 똑같음)
    public void RefreshQuestList()
    {
        // 1. 기존 슬롯 청소
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        // 2. 데이터만큼 슬롯 생성
        foreach (QuestData quest in availableQuests)
        {
            GameObject slot = Instantiate(questSlotPrefab, contentArea);

            // 3. 텍스트 채우기 (Slot 안의 TMP들을 찾아서 넣음)
            TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();
            
            // 순서대로: 0:이름, 1:보상정보, 2:난이도 (프리팹 구조에 따라 다름)
            if(texts.Length > 0) texts[0].text = quest.questName;
            if(texts.Length > 1) texts[1].text = $"{quest.rewardGold} G / {quest.duration}초";
            if(texts.Length > 2) texts[2].text = $"[{quest.rank}]";
            if(texts.Length > 3) 
                texts[3].text = $"[{quest.rank}] 권장 Lv.{quest.recommendedLevel}";

            // 4. 버튼 기능 연결 (수락 버튼)
            Button btn = slot.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => OnClickQuest(quest));
        }
    }

    // 퀘스트 클릭 시 실행 (나중에 파견 창 띄울 곳)
    void OnClickQuest(QuestData quest)
    {
       Debug.Log($"의뢰 선택됨: {quest.questName} -> 파티 선택창으로 이동");
        
        // 로비 매니저에게 "이 퀘스트 할 건데 파티 좀 골라줘" 라고 요청
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OpenDispatchPartyPopup(quest);
        }
    }
}