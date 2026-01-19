using UnityEngine;
using System.Collections.Generic;

// 직업 종류 (Enum)
public enum JobType
{
    전사, // 전사
    마법사,    // 마법사
    궁수,  // 궁수
    성직자,  // 사제
    도적    // 도적
}
// 성격 (enum)
public enum TraitType
{
    Normal,       // 평범함
    Coward,       // 겁쟁이
    HotTempered,  // 다혈질
    GlorySeeker,  // 공명심
    Careful,      // 신중함
    Lazy          // 게으름

}

// 등급 (Enum)
public enum RankType
{
    C, B, A, S
}

[System.Serializable] // 이 줄이 있어야 나중에 인스펙터에서 보입니다!
public class Adventurer
{
    public string name;      // 이름
    public JobType job;      // 직업
    public RankType rank;    // 등급
    public TraitType trait; // 성격
    
    // 능력치
    public int level;
    public int hp;
    public int atk;

    // -1이면 "대기 중(무소속)", 0, 1, 2...이면 해당 파티 소속
    public int assignedPartyIndex = -1;
    
    // 생성자 (Constructor): 모험가 태어날 때 기본 세팅
    public Adventurer(string _name, JobType _job, RankType _rank, TraitType _trait)
    {
        name = _name;
        job = _job;
        rank = _rank;
        trait = _trait;
        level = 1;
        
        // 일단 기본값 (나중에 직업별로 다르게 설정 가능)
        hp = 100;
        atk = 10;
        level = 1;
    }
    // 성격을 한글 이름으로 반환해주는 함수
    public string GetTraitName()
    {
        switch (trait)
        {
            case TraitType.Normal: return "평범함";
            case TraitType.Coward: return "겁쟁이";
            case TraitType.HotTempered: return "다혈질";
            case TraitType.GlorySeeker: return "공명심";
            case TraitType.Careful: return "신중함";
            case TraitType.Lazy: return "게으름";
            default: return "알수없음";
        }
    }

    //성격 설명 함수
    public string GetTraitDescription()
    {
        switch (trait)
        {
            case TraitType.Normal: 
                return "특별한 장점도, 단점도 없는 평범한 성격입니다.";
            case TraitType.Coward: 
                return "체력이 30% 이하로 떨어지면 도망갈 확률이 높습니다. 아군의 뒤에 숨기를 좋아합니다.";
            case TraitType.HotTempered: 
                return "적을 향해 달려들기를 선호합니다. 어그로를 무시합니다.";
            case TraitType.GlorySeeker: 
                return "막타를 칠 수 있는 적을 우선적으로 공격합니다.";
            case TraitType.Careful: 
                return "적에게 달려들기보다 가만히 서서 지켜보기를 좋아합니다. 가만히 있을 때 방어력이 올라갑니다";
            case TraitType.Lazy: 
                return "가끔 공격하지 않고 멍하니 서 있을 때가 있습니다.";
            default: 
                return "설명이 없습니다.";
        }
    }

    
}

[System.Serializable]
public class Party 
    {
        public string partyName; //파티 이름
        public List<Adventurer> members = new List<Adventurer>(); // 멤버 리스트

        public Party(string name)
        {
            partyName = name;
        }
    }