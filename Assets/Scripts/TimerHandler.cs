using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class TimerHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timeText;
    float stoptime = 1111;

    private void Update()
    {
        if (stoptime > 0)
        {
            stoptime -= Time.deltaTime;
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }


        float min = Mathf.FloorToInt(stoptime / 60);
        float sec = Mathf.FloorToInt(stoptime % 60);
        timeText.text = string.Format("{0:00}:{1:00}",min,sec);
    }
}
