using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("설정")]
    public GameObject unitPrefab;   
    public Transform spawnPointTeam; 

    void Start()
    {
        Debug.Log("1. BattleManager Start 시작됨"); // [체크 1]

        Party targetParty = null;

        // 1. GameManager가 있는지 확인
        if (GameManager.Instance != null && GameManager.Instance.currentDispatchParty != null)
        {
            Debug.Log("2. GameManager에서 파티 정보 가져옴"); // [체크 2-A]
            targetParty = GameManager.Instance.currentDispatchParty;
        }
        else
        {
            Debug.Log("2. 테스트 모드 진입 (임시 파티 생성)"); // [체크 2-B]
            targetParty = new Party("테스트 팀");
            
            // 모험가 생성 (빈 생성자 사용)
            Adventurer testMember1 = new Adventurer(); 
            testMember1.name = "임시 전사";
            targetParty.members.Add(testMember1);

            Adventurer testMember2 = new Adventurer();
            testMember2.name = "임시 궁수";
            targetParty.members.Add(testMember2);
        }

        SpawnMyParty(targetParty);
    }

    void SpawnMyParty(Party party)
    {
        Debug.Log($"3. 소환 함수 진입! 멤버 수: {party.members.Count}명"); // [체크 3]

        // 안전장치: 프리팹 연결 확인
        if (unitPrefab == null) 
        {
            Debug.LogError("🚨 비상! Unit Prefab이 연결되지 않았습니다!");
            return;
        }
        // 안전장치: 스폰 위치 연결 확인
        if (spawnPointTeam == null)
        {
            Debug.LogError("🚨 비상! Spawn Point Team이 연결되지 않았습니다!");
            return;
        }

        for (int i = 0; i < party.members.Count; i++)
        {
            Adventurer member = party.members[i];

            GameObject unitObj = Instantiate(unitPrefab);
            
            // 위치 잡기
            Vector3 offset = new Vector3(-1 * (i % 2), i * 1.5f - 2f, 0); 
            unitObj.transform.position = spawnPointTeam.position + offset;

            unitObj.name = $"Unit_{member.name}";
            unitObj.GetComponent<SpriteRenderer>().color = Color.blue; 

            Debug.Log($"4. {member.name} 생성 완료! 위치: {unitObj.transform.position}"); // [체크 4]
        }
    }
}