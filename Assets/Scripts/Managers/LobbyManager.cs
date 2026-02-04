using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트 매시 프로 필수
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance; //싱글톤 추가 (어디서든 LobbyManager.Instance로 호출)

    void Awake()
    {
        Instance = this;
    }
    [Header("뽑기 가격")]
    public int costBasic = 100;
    public int costNormal = 500;
    public int costPremium = 1000;

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
    public GameObject slotPrefab;     // Slot 프리팹

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

    [Header("색상 설정")]
    //명단 배경 갈색 (RGB: 60, 25, 12)
    public Color normalColor = new Color(60f / 255f, 25f / 255f, 12f / 255f); 
    public Color selectedColor = Color.yellow; // 선택 시 노란색

    [Header("로비 캐릭터 설정")]
    public GameObject lobbyChibiPrefab; // 프리팹
    public RectTransform lobbyFloor;    // 활동 구역(Lobby_Floor)

    private Party currentViewingParty; // 지금 상세창 파티
    
    // 현재 보고 있는 모험가를 기억하는 변수 (해고할 때 필요)
    private Adventurer currentTarget;

    public GameObject questPopup;

// 파견을 위한 임시 저장 변수
    private QuestData pendingQuest; // 지금 수락하려는 퀘스트
    private bool isDispatchMode = false; // 지금 파티 상태 구분

    

    // 시작할 때 UI 갱신
    void Start()
    {
        RefreshUI(); //UI 새로고침
        
        UpdateLobbyCharacters(); //게임 시작하자마자 로비에 캐릭터 소환
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

        recruitPopup.transform.SetAsLastSibling(); // 현재 창을 형제들 중 '막내'로 만들어서 화면 맨 앞에 그림 (다중 창)
    }
    // 팝업 안의 [닫기] 버튼을 누르면 -> 팝업을 끈다
    public void OnClickCloseRecruit()
    {
        recruitPopup.SetActive(false);
    }

    public void OnClickManage()
    {
        managePopup.SetActive(true); // 팝업 열기
        managePopup.transform.SetAsLastSibling(); // 맨 앞으로
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
        partyListPopup.transform.SetAsLastSibling(); //맨 앞으로
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

        inputNamePopup.transform.SetAsLastSibling();
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
        // A. 파견 모드일 때 (퀘스트 수락하러 온 경우)
        if (isDispatchMode)
        {
            // 1. 파티가 텅 비었으면?
            if (party.members.Count == 0)
            {
                Debug.Log("⚠️ 멤버가 없는 파티는 보낼 수 없습니다!");
                return;
            }

            // 2. 이미 파견 나간 파티라면?
            if (party.state != PartyState.Idle)
            {
                Debug.Log("⚠️ 이미 파견 중인 파티입니다!");
                return;
            }

            // 3. 출동! (GameManager에게 명령)
            GameManager.Instance.StartQuest(party, pendingQuest);
            
            // 4. 정리 (모드 끄기)
            isDispatchMode = false;
            pendingQuest = null;
        }
        // B. 일반 관리 모드일 때 (그냥 상세정보 보러 온 경우)
        else
        {
            currentViewingParty = party; 
            partyDetailPopup.SetActive(true);
            partyDetailPopup.transform.SetAsLastSibling(); 
            RefreshPartyDetail();
        }
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
        if (currentViewingParty == null) return;

        memberSelectPopup.SetActive(true); // 멤버 선택 창을 맨 앞으로 가져오기
        memberSelectPopup.transform.SetAsLastSibling(); 
        
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
            // 다른 파티 소속이면 패스
            if (member.assignedPartyIndex != -1 && !currentViewingParty.members.Contains(member)) 
                continue;

            GameObject slot = Instantiate(memberSlotPrefab, selectListContent);
            SetupSlotBasic(slot, member);

            // 이미지 & 버튼 찾기
            Image bgImage = slot.GetComponent<Image>();
            if (bgImage == null) bgImage = slot.GetComponentInChildren<Image>();

            Button btn = slot.GetComponent<Button>();
            if (btn == null) btn = slot.GetComponentInChildren<Button>();

            // 텍스트 색상 변경을 위해 찾기
            TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();

            // ★ [핵심] 색상 적용 로직
            if (bgImage != null)
            {
                if (currentViewingParty.members.Contains(member)) 
                {
                    // A. 선택된 상태 (노란 배경 + 검은 글씨)
                    bgImage.color = selectedColor; 
                    foreach(var txt in texts) txt.color = Color.black; 
                }
                else
                {
                    // B. 선택 안 된 상태 (지정하신 갈색 + 흰 글씨)
                    bgImage.color = normalColor; 
                    foreach(var txt in texts) txt.color = Color.white; 
                }
            }

            // 버튼 기능 연결
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnToggleMember(member, bgImage, texts));
            }
        }
    }

    // 8-1. 클릭 시 넣었다 뺐다 (토글)
   void OnToggleMember(Adventurer member, Image slotImage, TextMeshProUGUI[] texts)
    {
        // A. 이미 멤버라면 -> 뺀다 (원상복구)
        if (currentViewingParty.members.Contains(member))
        {
            currentViewingParty.members.Remove(member);
            member.assignedPartyIndex = -1; // -1은 '소속 없음'
            
            // 색상 원상복구
            slotImage.color = normalColor;
            foreach(var txt in texts) txt.color = Color.white;
        }
        // B. 멤버가 아니라면 -> 넣는다 (하이라이트)
        else
        {
            if (currentViewingParty.members.Count >= 4)
            {
                Debug.Log("꽉 찼습니다!");
                return;
            }

            currentViewingParty.members.Add(member);

            // ★ [수정된 부분] 무조건 1이 아니라, 실제 파티 번호를 찾아서 넣기
            // GameManager의 파티 리스트에서 '지금 보고 있는 파티(currentViewingParty)'가 몇 번째인지 찾습니다.
            int realPartyIndex = GameManager.Instance.partyList.IndexOf(currentViewingParty);
            member.assignedPartyIndex = realPartyIndex; 
            
            // 하이라이트
            slotImage.color = selectedColor;
            foreach(var txt in texts) txt.color = Color.black;
        }

        // 인원수 텍스트 갱신
        int currentCount = currentViewingParty.members.Count;
        if(selectCountText != null) 
        {
            selectCountText.text = $"현재 인원: {currentCount} / 4";
            selectCountText.color = (currentCount >= 4) ? Color.red : Color.white;
        }

        // 상세 창도 실시간으로 갱신
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
    public void ClosePartyListPopup()
    {
        partyListPopup.SetActive(false);
        isDispatchMode = false;
        pendingQuest = null;
    }


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

        detailPopup.transform.SetAsLastSibling(); // 맨 앞으로 가져오기
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

    public void OnClickquestPopup()
    {
        questPopup.SetActive(true);
    }

    public void OnClickExplore()
    {
        Debug.Log("탐험 버튼 클릭");
        // 나중에 던전 씬 로드
    }

    /*public void OnClickCard()
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

        UpdateLobbyCharacters(); //로비에 모험가 갱신
    }*/

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

     // - 로비 캐릭터 관리 -

    // 로비 내의 캐릭터들을 기록하는 출석부
    private Dictionary<Adventurer, GameObject> spawnedChibiMap = new Dictionary<Adventurer, GameObject>();

    void UpdateLobbyCharacters()//로비에 캐릭터 업데이트
    {
        // 1. [제거 로직] 데이터에는 없는데 로비에만 있는 유령 캐릭터 삭제 (해고/사망 시 처리)
        // (Dictionary를 돌면서 삭제하기 위해 리스트로 키만 먼저 복사함)
        List<Adventurer> toRemove = new List<Adventurer>();

        foreach (var pair in spawnedChibiMap)
        {
            // 게임매니저 명단에 이 모험가가 없으면?
            if (!GameManager.Instance.adventurers.Contains(pair.Key))
            {
                Destroy(pair.Value); // 로비에서 삭제
                toRemove.Add(pair.Key); // 출석부 지울 명단에 추가
            }
        }
        // 출석부에서 진짜 지우기
        foreach (var key in toRemove) spawnedChibiMap.Remove(key);


        // 2. [생성 로직] 데이터에는 있는데 로비엔 없는 신입 캐릭터 소환
        foreach (Adventurer adv in GameManager.Instance.adventurers)
        {
            // 이미 출석부에 있으면? -> 패스! (하던 산책 계속 하세요)
            if (spawnedChibiMap.ContainsKey(adv)) 
                continue;

            // 없다면? -> 새로 소환!
            GameObject chibi = Instantiate(lobbyChibiPrefab, lobbyFloor);
            
            // 위치 랜덤 조정 (안 그러면 다 정중앙에서 겹쳐서 태어남)
            RectTransform rect = chibi.GetComponent<RectTransform>();
            float rangeX = lobbyFloor.rect.width / 2f - 50f;
            float rangeY = lobbyFloor.rect.height / 2f - 50f;
            rect.anchoredPosition = new Vector2(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY));

            // 출석부에 등록
            spawnedChibiMap.Add(adv, chibi);
        }
    }

    // 퀘스트 보드에서 호출할 함수 (파티 선택)
    public void OpenDispatchPartyPopup(QuestData quest)
    {
        pendingQuest = quest; // 퀘스트 기억하기
        isDispatchMode = true; // 파티 선택 중

        //파티 목록 팝업 열기
        partyListPopup.SetActive(true);
        partyListPopup.transform.SetAsLastSibling();

        RefreshPartyList(); //목록 갱신
    }

    public void OnclickSummonBasic()
    {
        TrySummon(SummonTier.Basic, costBasic);
    }
    public void OnclickSummonNormal()
    {
        TrySummon(SummonTier.Normal, costNormal);
    }
    public void OnclickSummonPremium()
    {
        TrySummon(SummonTier.Premium, costPremium);
    }

    private void TrySummon(SummonTier tier, int cost) // 소환 로직
    {
        if (GameManager.Instance.gold < cost) // 돈 체크
        {
            Debug.Log(" 골드 부족!");
            return;
        }

        GameManager.Instance.gold -= cost; // 돈 차감

        Adventurer newMember = AdventurerGenerator.Generate(tier); // 생성기에 정보 넘겨주기

        GameManager.Instance.adventurers.Add(newMember); // 명단에 추가

        Debug.Log($"[{tier} 소환 성공] {newMember.name} ({newMember.job}) - 등급: {newMember.rank} / HP: {newMember.hp} / ATK: {newMember.atk}");

        RefreshUI();
        UpdateLobbyCharacters();
    }
}