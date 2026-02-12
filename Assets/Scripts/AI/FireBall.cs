using UnityEngine;

public class Fireball : MonoBehaviour
{
    private Vector3 targetPos;
    private float speed = 5.0f;
    private float damage;
    private string targetTag; // "Enemy" 또는 "Player"
    private bool isLaunched = false;

    // 1. 발사 설정 (누가, 어디로, 얼마만큼)
    public void Setup(Vector3 _targetPos, float _damage, string _targetTag)
    {
        targetPos = _targetPos;
        damage = _damage;
        targetTag = _targetTag;
        isLaunched = true;
        
        // 3초 뒤 자동 삭제 (못 맞췄을 경우 대비)
        Destroy(gameObject, 3.0f);
        
        // 발사 방향으로 회전
        Vector3 dir = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (!isLaunched) return;

        // 2. 날아가기
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // 3. 도착 체크 (거의 다 왔으면)
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            // 타겟 위치에 도착했으니 삭제 (혹은 범위 데미지 로직 추가 가능)
            Destroy(gameObject);
        }
    }

    // 4. 충돌 처리 (실제로 맞았을 때)
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 내가 노리는 태그(적)와 부딪혔는지 확인
        if (collision.CompareTag(targetTag))
        {
            BattleUnit unit = collision.GetComponent<BattleUnit>();
            if (unit != null)
            {
                unit.TakeDamage(damage); // 데미지 주기
                Debug.Log($"💥 명중! {damage} 데미지");
            }
            Destroy(gameObject); 
        }
    }
    
    // 3D일 경우를 대비해 3D 충돌도 추가
    void OnTriggerEnter(Collider collision)
    {
         if (collision.CompareTag(targetTag))
        {
            BattleUnit unit = collision.GetComponent<BattleUnit>();
            if (unit != null)
            {
                unit.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}