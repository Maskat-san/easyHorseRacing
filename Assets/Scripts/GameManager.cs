using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Text winnerText;

    private bool isRaceFinished = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AnnounceWinner(string winnerTag)
    {
        if (isRaceFinished)
        {
            return;
        }

        isRaceFinished = true;

        if (winnerText != null)
        {
            if (winnerTag == "Player1")
            {
                winnerText.text = "WINNER: PLAYER 1";
            }
            else if (winnerTag == "Player2")
            {
                winnerText.text = "WINNER: PLAYER 2";
            }
            else
            {
                winnerText.text = "RACE FINISHED";
            }
            
            // テキストオブジェクトをアクティブにして表示
            winnerText.gameObject.SetActive(true);
        }

        // コンソールにも勝者を表示（デバッグ用）
        Debug.Log("The winner is: " + winnerTag);

        Time.timeScale = 0; // ゲーム全体の時間を止める
    }
}