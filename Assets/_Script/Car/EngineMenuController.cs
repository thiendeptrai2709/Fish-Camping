using UnityEngine;
using UnityEngine.UI;

public class EngineMenuController : MonoBehaviour
{
    [SerializeField] private EngineRepairMinigame engineMinigame;
    [SerializeField] private QTEMinigame qteMinigame;
    [SerializeField] private BalanceMinigame balanceMinigame;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (engineMinigame == null)
            engineMinigame = Object.FindFirstObjectByType<EngineRepairMinigame>();

        if (menuPanel != null && closeButton == null)
        {
            // Tự động tìm nút X / Close trong menuPanel
            Button[] btns = menuPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                if (b != null && (b.name.ToLower().Contains("close") || b.name.ToLower().Contains("exit") || b.name == "X" || b.name.ToLower().Contains("x")))
                {
                    closeButton = b;
                    break;
                }
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void OnCloseButtonClicked()
    {
        if (engineMinigame != null)
        {
            engineMinigame.ExitRepairMode();
        }
    }

    private void Update()
    {
        if (engineMinigame == null)
        {
            engineMinigame = Object.FindFirstObjectByType<EngineRepairMinigame>();
            if (engineMinigame == null) return;
        }

        if (menuPanel == null) return;

        bool isEngineReady = engineMinigame.IsEngineOut;
        bool isQTEPlaying = qteMinigame != null && qteMinigame.IsPlaying;
        bool isBalancePlaying = balanceMinigame != null && balanceMinigame.IsPlaying;

        bool shouldShow = isEngineReady && !isQTEPlaying && !isBalancePlaying;

        if (menuPanel.activeSelf != shouldShow)
        {
            menuPanel.SetActive(shouldShow);
        }

        // Thoát nhanh bằng phím ESC hoặc E khi đang mở menu sửa xe
        if (shouldShow && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            engineMinigame.ExitRepairMode();
        }
    }
}