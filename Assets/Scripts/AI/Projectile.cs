using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 10f;
    private float damage = 0f;
    private string targetTag = ""; // "Enemy" 또는 "Player"

    // 화살 발사할 때 세팅해주는 함수
    public void Setup(Vector3 dir, float _damage, string _targetTag)
    {
        damage = _damage;
        targetTag = _targetTag;

        // 1. 날아가는 각도 맞추기 (오른쪽이 기준)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. 속도 설정 (앞으로 전진)
        GetComponent<Rigidbody2D>().linearVelocity = dir * speed; 
        
        // 3. 2초 뒤에 못 맞췄으면 자동 삭제 (메모리 관리)
        Destroy(gameObject, 2.0f);
    }

    // 충돌 감지
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 내 타겟 태그와 일치하는 놈만 때림
        if (collision.CompareTag(targetTag))
        {
            BattleUnit targetUnit = collision.GetComponent<BattleUnit>();
            if (targetUnit != null)
            {
                // 데미지 주기
                targetUnit.TakeDamage(damage);
                
                // 화살 삭제 (관통 화살이 아니라면)
                Destroy(gameObject);
            }
        }
    }
}