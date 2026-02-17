using UnityEngine;
using System.Collections.Generic;

[System.Serializable] // 인스펙터에 보이게 하는 마법의 단어
public struct DropItem
{
    public MonsterDropData item;      // 떨굴 아이템
    [Range(0, 100)] 
    public float dropRate;     // 확률 (0~100%)
}
public class EnemySlime : BattleUnit
{
    [Header("드랍 아이템 설정")]
    public List<DropItem> dropTable; // 전리품 목록
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

    void CheckDrop()
    {
        if (dropTable == null) return;

        foreach (DropItem loot in dropTable)
        {
            // 0~100 사이 랜덤 숫자 뽑기
            float randomValue = Random.Range(0f, 100f);

            // 확률 당첨!
            if (randomValue <= loot.dropRate)
            {
                Debug.Log($"💎 득템! [{name}]에게서 [{loot.item.itemName}] 획득!");
                
                // ★ 나중에 여기에 [GameManager.Instance.AddItem(loot.item)] 추가 예정
                // 지금은 로그만 띄웁니다.
                
                // (선택) 바닥에 아이템 떨어지는 연출을 원하면 여기서 프리팹 생성
            }
        }
    }
}