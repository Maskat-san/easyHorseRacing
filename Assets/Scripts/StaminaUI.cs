using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [Header("プレイヤー1")]
    public HorseController player1Controller;
    public Text player1StaminaText;

    [Header("プレイヤー2")]
    public HorseController player2Controller;
    public Text player2StaminaText;

    void Update()
    {
        if (player1Controller != null && player1StaminaText != null)
        {
            int p1_pace = player1Controller.GetCurrentPace();
            
            player1StaminaText.text = $"GEAR: {p1_pace}\n" + // \n で改行
                                      $"STAMINA: {Mathf.RoundToInt(player1Controller.currentStamina)} / {player1Controller.maxStamina}";
        }

        if (player2Controller != null && player2StaminaText != null)
        {
            int p2_pace = player2Controller.GetCurrentPace();

            player2StaminaText.text = $"GEAR: {p2_pace}\n" +
                                      $"STAMINA: {Mathf.RoundToInt(player2Controller.currentStamina)} / {player2Controller.maxStamina}";
        }
    }
}