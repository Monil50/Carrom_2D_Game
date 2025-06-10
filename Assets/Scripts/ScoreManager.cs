using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int playerScore = 0, compScore = 0;
    [SerializeField] TMP_Text playerScoreText, compScoreText;

    public void AddScore(int value)
    {
        Turn check = PlayManager.instance.currentTurn;
        if (check == Turn.User)
        {
            playerScore += value;

            playerScoreText.text = "You : " + playerScore;
        }
        else
        {
            compScore += value;

            compScoreText.text = "Comp : " + compScore;
        }

    }

}
