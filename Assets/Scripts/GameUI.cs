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
        [SerializeField] private Button restartButton;
        
        private GameManager gameManager;
        private TargetPool targetPool;
        
        private void Start()
        {
            gameManager = GameManager.Instance;
            targetPool = TargetPool.Instance;
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }
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
                    gameStatusText.text = gameManager.IsGameActive ? "Game Active" : "Game Over";
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
