using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Slider slider;       // 슬라이더 연결
    public Vector3 offset = new Vector3(0, 1.5f, 0); // 머리 위 높이

    private Transform target;   // 따라다닐 주인
    private BattleUnit unit;    // 주인의 스탯 정보

    // ★ [핵심] 이 함수가 없어서 에러가 났던 겁니다! public 필수!
    public void Setup(BattleUnit owner)
    {
        target = owner.transform;
        unit = owner;

        // 슬라이더 최댓값 설정
        if (slider != null)
        {
            slider.maxValue = unit.maxHp;
            slider.value = unit.currentHp;
        }
    }

    void LateUpdate()
    {
        // 주인이 없거나 죽었으면 나도 사라짐
        if (target == null)
        {
            Destroy(gameObject); 
            return;
        }

        // 1. 위치 따라가기 (주인 머리 위)
        transform.position = target.position + offset;

        // 2. 체력 갱신
        if (slider != null && unit != null)
        {
            slider.value = unit.currentHp;
        }
    }
}