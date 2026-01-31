using UnityEngine;
using UnityEngine.UI; // UI 건드릴 땐 필수!

public class HPBar : MonoBehaviour
{
    public Image fillImage; // 빨간색 이미지

    // 체력 갱신 함수 (현재체력 / 최대체력)
    public void UpdateHP(float currentHp, float maxHp)
    {
        if (fillImage != null)
        {
            // 0 ~ 1 사이 값으로 변환
            fillImage.fillAmount = currentHp / maxHp; 
        }
    }
}