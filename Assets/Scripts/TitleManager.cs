using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동 필수 네임스페이스
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public void OnClickNewGame()
    {
        Debug.Log("새로운 길드를 창설합니다...");
        
        // 데이터 초기화가 필요하면 여기서 GameManager 접근
        // GameManager.Instance.gold = 1000; 

        // "LobbyScene"으로 이동
        SceneManager.LoadScene("LobbyScene"); 
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}