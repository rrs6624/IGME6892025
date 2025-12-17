using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float gameLength = 300f;
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private TextMeshProUGUI timerText;

    private float timeRemaining;
    private bool gameEnded = false;

    void Start()
    {
        timeRemaining = gameLength;
    }

    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;

        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            Win();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void Win()
    {
        gameEnded = true;
        Debug.Log("Player survived 5 minutes! YOU WIN!");

        SceneManager.LoadScene(winSceneName);
    }
}