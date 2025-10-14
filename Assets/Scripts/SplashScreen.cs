using UnityEngine;
using UnityEngine.UI;

namespace ShootingSystem
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private float durationSeconds = 2f;
        [SerializeField] private string title = "Shooting prototype";
        [SerializeField] private string subtitle = "by Daniil Gorlov";
        
        private Canvas canvas;
        private Image background;
        private Text titleText;
        private Text subtitleText;
        private float timer;
        
        private void Awake()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("SplashCanvas");
            canvasObj.transform.SetParent(transform);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            background = bgObj.AddComponent<Image>();
            background.color = Color.black;
            RectTransform bgRt = background.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            
            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasObj.transform, false);
            titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform tRt = titleText.rectTransform;
            tRt.anchorMin = new Vector2(0.1f, 0.5f);
            tRt.anchorMax = new Vector2(0.9f, 0.7f);
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
            titleText.fontSize = 48;
            
            // Subtitle
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(canvasObj.transform, false);
            subtitleText = subObj.AddComponent<Text>();
            subtitleText.text = subtitle;
            subtitleText.color = new Color(1f, 1f, 1f, 0.85f);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform sRt = subtitleText.rectTransform;
            sRt.anchorMin = new Vector2(0.1f, 0.35f);
            sRt.anchorMax = new Vector2(0.9f, 0.5f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = Vector2.zero;
            subtitleText.fontSize = 28;
        }
        
        private void OnEnable()
        {
            timer = 0f;
            SetActive(true);
        }
        
        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= durationSeconds)
            {
                SetActive(false);
                enabled = false;
            }
        }
        
        private void SetActive(bool active)
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(active);
            }
        }
    }
}
