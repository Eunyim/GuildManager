using UnityEngine;

using UnityEngine;

// 1. 발동 조건 목록 (레고 블록 1)
public enum TraitConditionType
{
    Always,                 // 상시 발동 (예: 외팔이, 겁쟁이)
    NoAllyNearby,           // 반경 내 아군 없음 (예: 외톨이)
    SpecificEnemyExists,    // 특정 적 존재 (예: 고블린 공포증)
    HpBelowPercentage       // 체력이 일정 % 이하 (예: 광전사)
}

// 2. 결과 효과 목록 (레고 블록 2)
public enum TraitEffectType
{
    ModifyAttackPower,      // 공격력 증감 (예: +20%, -50%)
    ModifyMoveSpeed,        // 이동속도 증감 (예: +10%, -20%)
    ChangeAI_HideBehindAlly // AI 변경: 아군 뒤로 숨기 (예: 겁쟁이)
}

// 이 속성 덕분에 유니티 우클릭 메뉴에서 특성을 붕어빵처럼 찍어낼 수 있습니다.
[CreateAssetMenu(fileName = "New Trait", menuName = "Guild/Trait Data")]
public class TraitData : ScriptableObject
{
    [Header("📝 특성 기본 정보")]
    public string traitName;        // 특성 이름 (예: "고블린 공포증")
    [TextArea(2, 3)]
    public string description;      // 설명

    [Header("🔍 발동 조건 (Condition)")]
    public TraitConditionType conditionType;
    public string conditionTag;     // 찾을 태그 (예: "Enemy_Goblin" 등)
    public float conditionValue;    // 조건 수치 (예: 반경 3.0f, 체력 0.3 등)

    [Header("💥 적용 효과 (Effect)")]
    public TraitEffectType effectType;
    public float effectValue;       // 효과 수치 (예: 1.2 = 20% 증가, 0.8 = 20% 감소)
}