using UnityEngine;
using System.Collections.Generic; // 리스트 사용용

public static class AdventurerGenerator
{
    // 랜덤 이름 후보 (나중에 더 추가하세요!)
    private static string[] names = { 
        "아서", "란슬롯", "가웨인", "멀린", "헤라클레스", 
        "지크", "잔느", "쿠훌린", "길가메쉬", "이스칸달",
        "토르", "로키", "오딘", "프레이야", "펜릴"
    };

    // ★ 랜덤 모험가 생성 함수
    public static Adventurer Generate()
    {
        // 1. 이름 랜덤
        string rndName = names[Random.Range(0, names.Length)];

        // 2. 직업 랜덤
        JobType rndJob = (JobType)Random.Range(0, System.Enum.GetValues(typeof(JobType)).Length);

        // 3. 등급 랜덤
        int roll = Random.Range(0, 100);
        RankType rndRank;
        if (roll < 50) rndRank = RankType.C;
        else if (roll < 80) rndRank = RankType.B;
        else if (roll < 95) rndRank = RankType.A;
        else rndRank = RankType.S;

        // ★ 4. 성격 랜덤 추가 (핵심)
        TraitType rndTrait = (TraitType)Random.Range(0, System.Enum.GetValues(typeof(TraitType)).Length);

        // 5. 객체 생성 (수정된 생성자에 맞춰 trait 전달)
        Adventurer newAdventurer = new Adventurer(rndName, rndJob, rndRank, rndTrait);

        // 6. 스탯 보정
        int bonusStat = (int)rndRank * 5;
        newAdventurer.atk += bonusStat + Random.Range(0, 5);
        newAdventurer.hp += (bonusStat * 10) + Random.Range(0, 50);

        return newAdventurer;
    }
}