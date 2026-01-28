using UnityEngine;
using UnityEngine.Events; // 이벤트를 인스펙터에서 연결하기 위해 필수

public class InteractiveObject : MonoBehaviour
{
    [Header("설정")]
    public string objectName; // 예: "접수 데스크", "임무 게시판"
    
    [Header("클릭 시 실행할 행동")]
    public UnityEvent onClickAction; // 인스펙터에서 원하는 함수를 드래그&드롭으로 연결 가능

    // [선택 사항] 하이라이트 효과용 (나중에 스프라이트 분리하면 사용)
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    // 마우스가 들어왔을 때 (Hover On)
    void OnMouseEnter()
    {
        // 1. 마우스 커서 모양 변경 (나중에 추가 가능)
        // 2. 하이라이트 효과 (스프라이트가 있다면 색을 밝게)
        if (sr != null) sr.color = new Color(0.8f, 0.8f, 0.8f); // 살짝 회색조로 변경 (반응 확인용)
        
        Debug.Log($"{objectName}에 마우스 올림");
    }

    // 마우스가 나갔을 때 (Hover Off)
    void OnMouseExit()
    {
        // 색상 원상복구
        if (sr != null) sr.color = originalColor;
    }

    // 클릭했을 때 (Click)
    void OnMouseDown()
    {
        // UI가 떠있는 상태가 아닐 때만 클릭되게 하는 조건은 나중에 추가
        Debug.Log($"{objectName} 클릭됨!");
        
        // 연결된 함수 실행!
        onClickAction.Invoke();
    }
}