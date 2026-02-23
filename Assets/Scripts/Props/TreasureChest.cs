using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    public int goldReward = 50; // 기본 골드 보상
    public MonsterDropData itemReward; // (선택) 확정으로 주는 아이템
    
    private bool isOpened = false;

    // 상자를 열 때 호출할 함수
    public void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;

        Debug.Log("🎁 보물상자를 열었습니다!");

        // 1. 골드 획득 (GameManager에 골드 추가 함수가 있다고 가정)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldReward);
            Debug.Log($"💰 {goldReward} 골드 획득!");
        }

        // 2. 아이템 획득 (아이템이 설정되어 있다면)
        if (itemReward != null && BattleManager.Instance != null)
        {
            BattleManager.Instance.AddItemToLootBag(itemReward);
        }

        // 3. 상자 열리는 연출 (일단은 그냥 파괴)
        Destroy(gameObject, 1.0f); // 1초 뒤 사라짐
        
        // ★ 4. 상자를 열었으니 다음 스테이지로 넘어가라고 매니저에게 알림
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.GoToNextStage();
        }
    }

    // 마우스로 상자를 클릭했을 때 열리게 하기 (임시)
    void OnMouseDown()
    {
        OpenChest();
    }
}