using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShootingSystem
{
    public class GameUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI targetsText;
        [SerializeField] private TextMeshProUGUI gameStatusText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        private GameManager gameManager;
        private TargetPool targetPool;
        
        private void Start()
        {
            gameManager = GameManager.Instance;
            targetPool = TargetPool.Instance;
            
        }
        
        private void Update()
        {
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (gameManager != null)
            {
                // Update game status
                if (gameStatusText != null)
                {
                    if (gameManager.GameEnded)
                    {
                        if (gameManager.CurrentScore == 0)
                        {
                            gameStatusText.text = "LOSE GAME!";
                            gameStatusText.color = Color.red;
                        }
                        else
                        {
                            gameStatusText.text = $"WIN GAME! Score: {gameManager.CurrentScore}";
                            gameStatusText.color = Color.green;
                        }
                    }
                    else
                    {
                        gameStatusText.text = gameManager.IsGameActive ? "Game Active" : "Game Over";
                        gameStatusText.color = Color.white;
                    }
                }
                
                // Update timer
                if (timerText != null)
                {
                    float timeRemaining = gameManager.GameTimeRemaining;
                    int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                    int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                    timerText.text = $"Time: {minutes:00}:{seconds:00}";
                }
                
                // Update score
                if (scoreText != null)
                {
                    scoreText.text = $"Score: {gameManager.CurrentScore}";
                }
            }
            
            // Update targets count
            if (targetsText != null && targetPool != null)
            {
                int activeTargets = targetPool.GetActiveTargetCount();
                int maxTargets = gameManager != null ? gameManager.MaxTargets : 5;
                targetsText.text = $"Active Targets: {activeTargets}/{maxTargets}";
            }
        }
        
        private void OnRestartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }
}
