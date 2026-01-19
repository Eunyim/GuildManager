using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트 매시 프로 필수

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance; //싱글톤 추가 (어디서든 LobbyManager.Instance로 호출)

    void Awake()
    {
        Instance = this;
    }

    [Header("툴팁 UI")]
    public GameObject tooltipPanel;         
    public TextMeshProUGUI tooltipText;

    [Header("상단 정보 UI")]
    public TextMeshProUGUI guildNameText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI dayText;

    [Header("팝업창")]
    public GameObject recruitPopup;

    [Header("모집 설정")]
    public int recruitCost = 100; // 모집 비용 (100골드)

    [Header("관리(명단) UI")]
    public GameObject managePopup;    // 관리 팝업창
    public Transform listContent;     // Scroll View 안의 Content (슬롯이 붙을 부모)
    public GameObject slotPrefab;     // 아까 만든 Slot 프리팹

    [Header("상세 정보 팝업 연결")]
    public GameObject detailPopup;        // 팝업 전체 오브젝트
    public TextMeshProUGUI detailName;    // 이름 텍스트
    public TextMeshProUGUI detailInfo;    // 직업+등급
    public TextMeshProUGUI detailTrait;   // 성격
    public TextMeshProUGUI detailParty;   // 소속 파티 상태
    public TextMeshProUGUI detailStats;   // 스탯 정보

    [Header("파티 목록 팝업")]
    public GameObject partyListPopup;
    public Transform partyListContent; // 파티 목록이 뜰 곳
    public GameObject partyListSlotPrefab; // 파티 이름이 적힌 긴 버튼 프리팹

    [Header("이름 입력 팝업")]
    public GameObject inputNamePopup;
    public TMP_InputField nameInputField;

    [Header("파티 상세 팝업")]
    public GameObject partyDetailPopup;
    public TextMeshProUGUI detailTitleText; // "OOO 파티" 제목
    public Transform detailMemberContent;   // 멤버 슬롯이 뜰 곳
    public GameObject memberSlotPrefab;     // 멤버 슬롯 프리팹
    public TextMeshProUGUI emptyMessageText; // "소속된 모험가가 없습니다"

    [Header("멤버 선택(추가) 팝업")]
    public GameObject memberSelectPopup;
    public Transform selectListContent;
    public TextMeshProUGUI selectCountText; // "1/4" 표시
    public GameObject highlightFramePrefab; // 선택 시 씌울 노란 테두리 이미지 (옵션)

    private Party currentViewingParty; // 지금 상세창 파티
    
    // 현재 보고 있는 모험가를 기억하는 변수 (해고할 때 필요)
    private Adventurer currentTarget;

    

    // 시작할 때 UI 갱신
    void Start()
    {
        RefreshUI();
    }

    // UI 새로고침 함수 (돈 쓰거나 날짜 바뀔 때 호출)
    public void RefreshUI()
    {
        // 싱글톤 GameManager가 없으면 에러 방지
        if (GameManager.Instance == null) return;

        guildNameText.text = GameManager.Instance.guildName;
        goldText.text = $"{GameManager.Instance.gold:N0} G"; // 1,000 단위 쉼표
        dayText.text = $"Day {GameManager.Instance.day}";
    }

    // --- 버튼 연결용 함수들 (나중에 내용 채움) ---
    
    public void OnClickRecruit()
    {
        Debug.Log("모집 버튼 클릭");
        recruitPopup.SetActive(true);
    }
    // 팝업 안의 [닫기] 버튼을 누르면 -> 팝업을 끈다
    public void OnClickCloseRecruit()
    {
        recruitPopup.SetActive(false);
    }

    public void OnClickManage()
    {
        managePopup.SetActive(true); // 팝업 열기
        RefreshMemberList();         // 명단 갱신
    }

    // [닫기] 버튼
    public void OnClickCloseManage()
    {
        managePopup.SetActive(false);
    }

    public void OnClickOpenPartyManage() //파티 관리 버튼
    {
        partyListPopup.SetActive(true);
        RefreshPartyList();
    }

    void RefreshPartyList()
    {
        foreach (Transform child in partyListContent) Destroy(child.gameObject);

        // 생성된 파티들을 버튼으로 만들기
        foreach (Party party in GameManager.Instance.partyList)
        {
            GameObject newSlot = Instantiate(partyListSlotPrefab, partyListContent);
            // 버튼 텍스트에 파티 이름 표시
            newSlot.GetComponentInChildren<TextMeshProUGUI>().text = party.partyName;
            
            // 클릭 시 -> 상세 창 열기
            newSlot.GetComponent<Button>().onClick.AddListener(() => OnClickPartyItem(party));
        }
    }

    public void OnClickCreatePartyButton() // [파티 생성] 버튼
    {
        inputNamePopup.SetActive(true);
        nameInputField.text = ""; // 초기화
    }

    public void OnClickConfirmCreateName() // 입력창 [완료] 버튼
    {
        string pName = nameInputField.text;
        if (string.IsNullOrEmpty(pName)) return; // 빈 이름이면 아무것도 안 함

        // 1. 파티 생성 및 저장
        Party newParty = new Party(pName);
        GameManager.Instance.partyList.Add(newParty);

        // 2. UI 정리 (순서 중요!)
        nameInputField.text = "";        // 텍스트 비우기 (다음 번을 위해)
        inputNamePopup.SetActive(false); // ★ 팝업 끄기 (확실하게!)
        
        // 3. 목록 새로고침
        RefreshPartyList();
    }

    void OnClickPartyItem(Party party)
    {
        currentViewingParty = party; // 선택한 파티 기억
        partyDetailPopup.SetActive(true);
        RefreshPartyDetail();
    }

    void RefreshPartyDetail()
    {
        if (currentViewingParty == null) return;

        detailTitleText.text = currentViewingParty.partyName;

        // 멤버 리스트 청소
        foreach (Transform child in detailMemberContent) Destroy(child.gameObject);

        // 멤버가 없으면 문구 띄우기
        if (currentViewingParty.members.Count == 0)
        {
            emptyMessageText.gameObject.SetActive(true);
        }
        else
        {
            emptyMessageText.gameObject.SetActive(false);
            // 멤버 슬롯 생성
            foreach (Adventurer member in currentViewingParty.members)
            {
                GameObject slot = Instantiate(memberSlotPrefab, detailMemberContent);
                SetupSlotBasic(slot, member); // 이름/직업 표시
                
                // 상세창에서의 클릭 기능: (추후 추방 기능을 넣는다면 여기)
            }
        }
    }
    
    // [해체] 버튼
    public void OnClickDisbandParty()
    {
        if (currentViewingParty == null) return;

        // 모험가들의 소속 정보 초기화 (모두 대기 상태로)
        foreach (var member in currentViewingParty.members)
        {
            member.assignedPartyIndex = -1; 
        }

        // 리스트에서 삭제
        GameManager.Instance.partyList.Remove(currentViewingParty);
        currentViewingParty = null;

        // 창 닫고 목록 갱신
        partyDetailPopup.SetActive(false);
        RefreshPartyList();
    }

    public void OnClickAddMemberButton() // [추가] 버튼
    {
        memberSelectPopup.SetActive(true);
        RefreshSelectPopup();
    }

    void RefreshSelectPopup()
    {
        // 안전장치
        if (currentViewingParty == null || selectListContent == null) return;

        // 1. 기존 목록 청소
        foreach (Transform child in selectListContent) Destroy(child.gameObject);

        // 2. 인원수 텍스트 갱신
        if (selectCountText != null)
        {
            int currentCount = currentViewingParty.members.Count;
            selectCountText.text = $"현재 인원: {currentCount} / 4";
            selectCountText.color = (currentCount >= 4) ? Color.red : Color.white;
        }

        // 3. 리스트 생성 루프
        foreach (Adventurer member in GameManager.Instance.adventurers)
        {
            // 다른 파티에 속해있으면 건너뛰기
            if (member.assignedPartyIndex != -1 && !currentViewingParty.members.Contains(member)) 
                continue;

            // 슬롯 생성
            GameObject slot = Instantiate(memberSlotPrefab, selectListContent);
            SetupSlotBasic(slot, member);

            // ★ 이미지 컴포넌트 안전하게 찾기 (최상위에 없으면 자식에서 찾음)
            Image bgImage = slot.GetComponent<Image>();
            if (bgImage == null) bgImage = slot.GetComponentInChildren<Image>();

            // ★ 버튼 컴포넌트 안전하게 찾기
            Button btn = slot.GetComponent<Button>();
            if (btn == null) btn = slot.GetComponentInChildren<Button>();

            // 하이라이트 처리 (bgImage가 있을 때만 실행)
            if (bgImage != null)
            {
                // ▼ 오타 수정! (.members.Members -> .members)
                if (currentViewingParty.members.Contains(member)) 
                    bgImage.color = Color.yellow;
                else
                    bgImage.color = Color.white;
            }

            // 버튼 기능 연결 (btn이 있을 때만 실행)
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                // bgImage가 null일 수도 있으니 전달할 때 주의
                btn.onClick.AddListener(() => OnToggleMember(member, bgImage));
            }
        }
    }

    // 8-1. 클릭 시 넣었다 뺐다 (토글)
    void OnToggleMember(Adventurer member, Image slotImage)
    {
        // A. 이미 멤버라면 -> 뺀다 (하이라이트 해제)
        if (currentViewingParty.members.Contains(member))
        {
            currentViewingParty.members.Remove(member);
            member.assignedPartyIndex = -1; // 무소속
            slotImage.color = Color.white;
        }
        // B. 멤버가 아니라면 -> 넣는다 (하이라이트)
        else
        {
            // 8-2. 4명 초과 체크
            if (currentViewingParty.members.Count >= 4)
            {
                Debug.Log("경고: 파티원은 최대 4명입니다!");
                // (여기에 경고 팝업이나 텍스트 애니메이션 추가 가능)
                return;
            }

            currentViewingParty.members.Add(member);
            member.assignedPartyIndex = 1; // (임시) 파티 소속됨 표시
            slotImage.color = Color.yellow;
        }

        // 인원수 텍스트 갱신
        int currentCount = currentViewingParty.members.Count;
        selectCountText.text = $"현재 인원: {currentCount} / 4";
        
        // 상세 창도 실시간으로 갱신해주면 좋음
        RefreshPartyDetail();
    }

    // 슬롯 텍스트 채우는 도우미 함수
    void SetupSlotBasic(GameObject slot, Adventurer member)
    {
        TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 3)
        {
            texts[0].text = member.name;
            texts[1].text = member.job.ToString();
            texts[2].text = member.rank.ToString();
        }
    }

    // 팝업 닫기들
    public void CloseInputPopup() => inputNamePopup.SetActive(false);
    public void ClosePartyDetailPopup() => partyDetailPopup.SetActive(false);
    public void CloseSelectPopup() => memberSelectPopup.SetActive(false);
    public void ClosePartyListPopup() => partyListPopup.SetActive(false);


    void RefreshMemberList()
    {
        // 1. 기존에 떠있던 슬롯들 싹 지우기 (초기화)
        foreach (Transform child in listContent)
        {
            Destroy(child.gameObject);
        }

        // 2. GameManager에 있는 모험가 수만큼 반복
        foreach (Adventurer member in GameManager.Instance.adventurers)
        {
            // 3. 슬롯 생성 (Instantiate)
            GameObject newSlot = Instantiate(slotPrefab, listContent);

           // 텍스트 컴포넌트들을 가져옴
            TextMeshProUGUI[] texts = newSlot.GetComponentsInChildren<TextMeshProUGUI>();
            
            // 텍스트 개수에 맞춰 데이터 할당
            // (순서: 이름, 직업, 등급, 성격) <- 프리팹 상의 순서(Hierarchy 위에서 아래)에 따라 다릅니다!
            // 안전하게 하려면 나중에 별도 스크립트를 파는 게 좋지만, 지금은 순서대로 넣습니다.
            
            if(texts.Length >= 3) // 텍스트가 3개라면
            {
                 texts[0].text = member.name;            
                 texts[1].text = member.job.ToString();
                 texts[2].text = member.rank.ToString();
                 
                 // ★ 성격 표시 (한글 변환 함수 사용)
                 //texts[3].text = member.GetTraitName(); 
            }
            // 만약 아직 텍스트를 추가 안 해서 3개라면?
            else if(texts.Length >= 3)
            {
                // 기존 이름 옆에 괄호로 붙여서 보여주기 (임시 방편)
                texts[0].text = $"{member.name} ({member.GetTraitName()})";
                texts[1].text = member.job.ToString();
                texts[2].text = member.rank.ToString();
            }

            Button slotButton = newSlot.GetComponent<Button>();
            
            if (slotButton != null)
            {
                // 클릭 이벤트 연결 (람다식 사용)
                // "이 버튼을 누르면 -> OpenDetailPopup(member)를 실행해라"
                slotButton.onClick.AddListener(() => OpenDetailPopup(member));
            }
        }
    }

    // --- 1. 슬롯 클릭 시 실행될 함수 (팝업 열기) ---
    public void OpenDetailPopup(Adventurer adventurer)
    {
        currentTarget = adventurer; // 누구를 클릭했는지 기억

        // 텍스트 데이터 채워넣기
        detailName.text = adventurer.name;
        detailInfo.text = $"{adventurer.job} | Rank {adventurer.rank}";
        detailTrait.text = $"성격: {adventurer.GetTraitName()}";
        
        // 소속 파티 표시 로직
        if (adventurer.assignedPartyIndex == -1)
            detailParty.text = "<color=green>대기 중</color>";
        else
            detailParty.text = $"<color=yellow>파티 {adventurer.assignedPartyIndex + 1} 소속</color>";

        detailStats.text = $"HP: {adventurer.hp} / ATK: {adventurer.atk}";

        // 팝업 켜기
        detailPopup.SetActive(true);
    }

    // --- 2. [해고하기] 버튼 기능 ---
    public void OnClickFire()
    {
        if (currentTarget == null) return;

        // 파티에 속해있으면 해고 불가 (안전장치)
        if (currentTarget.assignedPartyIndex != -1)
        {
            Debug.Log("파티 중인 멤버는 해고할 수 없습니다!");
            return;
        }

        // 명단에서 삭제
        GameManager.Instance.adventurers.Remove(currentTarget);
        Debug.Log($"{currentTarget.name} 해고 완료.");

        // UI 갱신 (리스트 다시 그리기)
        RefreshMemberList();

        // 팝업 닫기
        CloseDetailPopup();
    }

    // --- 3. [닫기] 버튼 기능 ---
    public void CloseDetailPopup()
    {
        detailPopup.SetActive(false);
        currentTarget = null;

        HideTraitTooltip();
    }

    public void OnClickShop()
    {
        Debug.Log("상점 버튼 클릭");
    }

    public void OnClickExplore()
    {
        Debug.Log("탐험 버튼 클릭");
        // 나중에 던전 씬 로드
    }

    public void OnClickCard()
    {
        // 1. 돈이 충분한지 확인
        if (GameManager.Instance.gold < recruitCost)
        {
            Debug.Log("골드가 부족합니다!");
            return;
        }

        // 2. 돈 차감
        GameManager.Instance.gold -= recruitCost;

        // 3. 랜덤 모험가 생성 (Generator 호출)
        Adventurer newMember = AdventurerGenerator.Generate();

        // 4. 길드 명단에 추가
        GameManager.Instance.adventurers.Add(newMember);

        // 5. 결과 로그 출력 (나중에 결과창 UI로 바꿀 예정)
        Debug.Log($"[채용 성공] 이름: {newMember.name} | 직업: {newMember.job} | 등급: {newMember.rank}");

        // 6. UI 갱신 (돈 줄어든 거 반영)
        RefreshUI();

        // 7. 팝업 열어놓기 (채용 끝났으니)
        recruitPopup.SetActive(true);
    }

    public void ShowTraitTooltip()// 툴팁 켜기 함수
    {
        if (currentTarget == null) return; //현재 선택된 모험가가 없다면 무시

        tooltipText.text = currentTarget.GetTraitDescription(); // 텍스트 설정

        tooltipPanel.SetActive(true);
    }

    public void HideTraitTooltip()// 툴팁 끄기 함수
    {
        tooltipPanel.SetActive(false);
    }
}