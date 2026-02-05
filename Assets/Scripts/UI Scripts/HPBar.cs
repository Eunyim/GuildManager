using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [Header("슬라이더 연결")]
    public Slider hpSlider; // 빨간색/초록색 슬라이더
    public Slider mpSlider; // 파란색 슬라이더

    [Header("위치 조정")]
    public Vector3 offset = new Vector3(0, 2.0f, 0); // 머리보다 약간 위

    private Transform target;   // 따라다닐 주인 (Unit)
    private BattleUnit unit;    // 주인의 데이터

    // 유닛이 생성될 때 이 함수를 호출해서 연결해줌
    public void Setup(BattleUnit owner)
    {
        target = owner.transform;
        unit = owner;

        // HP 바 초기화
        if (hpSlider != null)
        {
            hpSlider.maxValue = unit.maxHp;
            hpSlider.value = unit.currentHp;
        }

        // MP 바 초기화
        if (mpSlider != null)
        {
            mpSlider.maxValue = unit.maxMp;
            mpSlider.value = unit.currentMp;
        }
    }

    void LateUpdate()
    {
        // 주인이 없거나 죽었으면 -> HP바도 삭제
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. 위치 따라가기
        transform.position = target.position + offset;

        // 2. HP 실시간 갱신
        if (hpSlider != null && unit != null)
        {
            hpSlider.value = unit.currentHp;
        }

        // 3. MP 실시간 갱신
        if (mpSlider != null && unit != null)
        {
            mpSlider.value = unit.currentMp;
        }
    }
}