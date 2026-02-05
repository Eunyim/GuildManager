using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummonResultPopup : MonoBehaviour
{
    [Header("UI 연결")]
    public Image portraitImage;      // 초상화 (Portrait)
    public TextMeshProUGUI rankText; // 등급 (Txt_Rank)
    public TextMeshProUGUI nameText; // 이름 (Txt_Name)
    public TextMeshProUGUI jobText;  // 직업 (Txt_Job)
    public TextMeshProUGUI statsText;// 스탯 (Txt_Stats)

    // 소환 성공 시 호출될 함수
    public void ShowResult(Adventurer adv)
    {
        // 1. 팝업 켜기
        gameObject.SetActive(true);
        transform.SetAsLastSibling(); // 맨 앞으로

        // 2. 텍스트 데이터 채우기
        nameText.text = adv.name;
        jobText.text = $"직업: {adv.job}";
        statsText.text = $"HP : {adv.hp}\nATK : {adv.atk}\n성격 : {adv.GetTraitName()}";
        
        // 3. 등급 표시 (색상 효과)
        rankText.text = $"{adv.rank} Rank";
        SetRankColor(adv.rank);

        // 4. (나중에) 직업에 맞는 초상화 이미지 변경
        // portraitImage.sprite = ... 
    }

    // 등급에 따라 글자 색깔 바꿔주는 연출 함수
    void SetRankColor(RankType rank)
    {
        switch (rank)
        {
            case RankType.S: rankText.color = Color.yellow; break; // 금색
            case RankType.A: rankText.color = Color.red; break;    // 빨강
            case RankType.B: rankText.color = Color.cyan; break;   // 하늘색
            default:         rankText.color = Color.white; break;  // 흰색
        }
    }

    // [확인] 버튼에 연결할 함수
    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}