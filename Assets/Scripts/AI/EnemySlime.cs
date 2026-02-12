using UnityEngine;

public class EnemySlime : BattleUnit
{
    protected override void Start()
    {
        // 슬라임 스탯
        maxHp = 100f;
        attackRange = 2.3f; // 사거리 넉넉함
        moveSpeed = 1.5f; 
        attackPower = 10f;
        
        // ★ [추가] 공격 속도 설정 (이게 없으면 기본값에 따라 안 때릴 수도 있음)
        attackCooldown = 1.5f; 

        base.Start();
    }

    protected override void Attack()
    {
        // 디버깅용 로그 (범인 색출)
        // Debug.Log($"[슬라임] 공격 시도! 타겟: {(target != null ? target.name : "없음")}");

        if(target != null)
        {
            // 진짜 때리기
            target.TakeDamage(attackPower);
            
            // 때렸다고 로그 찍기
            // Debug.Log($"💥 [슬라임] 퍽! {target.name}에게 {attackPower} 데미지!");
        }
        else
        {
            // 타겟이 없어서 못 때림 -> 태그 문제일 확률 99%
             Debug.LogWarning("⚠️ [슬라임] 때리려는데 타겟이 null입니다! 플레이어 태그(Player)를 확인하세요.");
        }
    }
}