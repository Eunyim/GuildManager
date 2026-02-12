using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Slider hpSlider;
    public Slider mpSlider;

    // 초기 설정 (생성될 때 한 번 호출)
    public void Setup(BattleUnit unit)
    {
        if (hpSlider != null) 
        {
            hpSlider.maxValue = unit.maxHp;
            hpSlider.value = unit.currentHp;
        }
        
        if (mpSlider != null)
        {
            mpSlider.maxValue = unit.maxMp;
            mpSlider.value = unit.currentMp;
        }
    }

    // ★ [핵심] 매 프레임 체력/마나 갱신하는 함수 (이게 없어서 에러 남!)
    public void UpdateBar(float currentHp, float maxHp, float currentMp, float maxMp)
    {
        if (hpSlider != null) 
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (mpSlider != null) 
        {
            mpSlider.maxValue = maxMp;
            mpSlider.value = currentMp;
        }
    }
}