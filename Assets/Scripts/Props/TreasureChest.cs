using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpened) return;

        if (collision.CompareTag("Player"))
        {
            isOpened = true;
            Debug.Log($"🎁 {collision.name}가 보물상자를 열었습니다!");
            
            // 매니저에게 다음 스테이지로 가라고 알려줌
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnChestOpened();
            }

            // 색깔 바꾸지 말고, 터치 즉시 깔끔하게 상자 파괴!
            Destroy(gameObject); 
        }
    }
}