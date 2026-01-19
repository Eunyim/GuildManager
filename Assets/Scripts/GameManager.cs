using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // [싱글톤 패턴] 전역에서 접근 가능한 유일한 인스턴스
    public static GameManager Instance;

    [Header("길드 데이터")]
    public string guildName = "용감한 모험가들"; // 길드 이름
    public int gold = 1000;          // 초기 자금
    public int reputation = 0;       // 명성
    public int day = 1;              // 날짜

    [Header("건물 정보")]
    public int guildLevel = 0; // 0: 텐트/판잣집, 1: 목조 건물, 2: 석조 건물 ...

    public List<Adventurer> adventurers = new List<Adventurer>(); //모험가 명단

    [Header("파티 데이터")]
    public List<Party> partyList = new List<Party>();


    private void Awake()
    {
        // 싱글톤 보장 로직
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    // 테스트용 함수
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"[재정] 현재 골드: {gold} G");
    }
}