using System.Collections.Generic;
using UnityEngine;

public enum PartyState 
{ 
    Idle,       // 대기 중
    OnQuest,    // 파견 나감
    Resting     // 휴식 중 (나중을 위해)
}

[System.Serializable]
public class Party // 1. 클래스 이름이 'Party' 여야 합니다.
{
    public string partyName;
    public List<Adventurer> members = new List<Adventurer>();

    public PartyState state = PartyState.Idle; // 파티 현재 상태 변수

    // 생성자 (이 이름도 반드시 'Party' 여야 합니다)
    public Party(string name) 
    {
        this.partyName = name;
    }

    // 평균 레벨 계산 함수
    public int GetPartyLevel()
    {
        if (members == null || members.Count == 0) return 0;

        int sumLevel = 0;
        foreach (Adventurer member in members)
        {
            sumLevel += member.level;
        }
        return sumLevel / members.Count; 
    }
}