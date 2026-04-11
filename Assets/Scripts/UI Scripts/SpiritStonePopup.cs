using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 신전 팝업: 영혼석으로 전사한 모험가를 부활시킨다.
// 사용 방법:
//   1. Hierarchy에 신전 팝업 Panel 생성 후 이 스크립트를 추가
//   2. 인스펙터에서 아래 필드를 연결
//   3. LobbyManager.spiritStonePopup에 이 컴포넌트 연결
//   4. 신전 버튼 onClick → LobbyManager.OnClickOpenSpiritStonePopup()
public class SpiritStonePopup : MonoBehaviour
{
    [Header("상단 정보")]
    public TextMeshProUGUI stoneCountText;   // "보유 영혼석: N개"
    public TextMeshProUGUI goldText;         // "보유 골드: N G"
    public TextMeshProUGUI emptyText;        // "부활 가능한 모험가가 없습니다" 문구

    [Header("목록")]
    public Transform listContent;           // Scroll View Content
    public GameObject slotPrefab;           // 슬롯 프리팹 (TMP 3개 이상 + Button 1개)

    // -----------------------------------------------------------
    // 외부 호출: 팝업을 열 때마다 LobbyManager에서 호출
    // -----------------------------------------------------------
    public void Refresh()
    {
        UpdateHeader();
        BuildList();
    }

    void UpdateHeader()
    {
        if (GameManager.Instance == null) return;

        int stones = GameManager.Instance.GetSpiritStoneCount();
        if (stoneCountText != null) stoneCountText.text = $"보유 영혼석: {stones}개";
        if (goldText       != null) goldText.text       = $"보유 골드: {GameManager.Instance.gold:N0} G";
    }

    void BuildList()
    {
        if (listContent == null || slotPrefab == null) return;

        // 기존 슬롯 제거
        foreach (Transform child in listContent) Destroy(child.gameObject);

        List<Adventurer> dead = GameManager.Instance != null
            ? GameManager.Instance.deadAdventurers
            : new List<Adventurer>();

        if (emptyText != null) emptyText.gameObject.SetActive(dead.Count == 0);

        foreach (Adventurer deceased in dead)
        {
            GameObject slot = Instantiate(slotPrefab, listContent);
            SetupSlot(slot, deceased);
        }
    }

    void SetupSlot(GameObject slot, Adventurer deceased)
    {
        TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();
        int goldCost      = GameManager.Instance.GetReviveCost(deceased.rank);
        bool hasStone     = GameManager.Instance.HasSpiritStone(deceased);
        bool canAfford    = hasStone && GameManager.Instance.gold >= goldCost;
        string stoneKey   = $"{deceased.name}의 영혼석";
        string deathPlace = string.IsNullOrEmpty(deceased.diedAtQuestName)
            ? "알 수 없는 던전" : deceased.diedAtQuestName;

        // 슬롯 텍스트 배치 (프리팹 순서: 이름, 직업/등급/사망지, 영혼석 상태, 부활 비용)
        if (texts.Length > 0) texts[0].text = deceased.name;
        if (texts.Length > 1) texts[1].text = $"{deceased.job} | Rank {deceased.rank} | 전사: {deathPlace}";
        if (texts.Length > 2)
        {
            if (hasStone)
            {
                texts[2].text  = $"[{stoneKey} 보유]";
                texts[2].color = Color.cyan;
            }
            else
            {
                texts[2].text  = $"[{stoneKey} 없음 — 유해 수습 필요]";
                texts[2].color = Color.grey;
            }
        }
        if (texts.Length > 3)
        {
            texts[3].text  = $"부활 비용: {goldCost:N0} G";
            texts[3].color = canAfford ? Color.white : Color.red;
        }

        // 부활 버튼
        Button btn = slot.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.interactable = canAfford;
            Adventurer captured = deceased;
            btn.onClick.AddListener(() => OnClickRevive(captured));
        }
    }

    void OnClickRevive(Adventurer deceased)
    {
        if (GameManager.Instance == null) return;

        bool success = GameManager.Instance.ReviveWithSpiritStone(deceased);
        if (success)
        {
            // LobbyManager UI 전체 갱신
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.RefreshUI();
            }
            // 팝업 내부 갱신
            Refresh();
        }
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
