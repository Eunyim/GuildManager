using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Party // 1. 클래스 이름이 'Party' 여야 합니다.
{
    public string partyName;
    public List<Adventurer> members = new List<Adventurer>();

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