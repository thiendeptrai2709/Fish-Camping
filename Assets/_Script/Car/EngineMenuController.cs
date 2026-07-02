using UnityEngine;

public class EngineMenuController : MonoBehaviour
{
    [SerializeField] private EngineRepairMinigame engineMinigame;
    [SerializeField] private QTEMinigame qteMinigame;
    [SerializeField] private BalanceMinigame balanceMinigame;
    [SerializeField] private GameObject menuPanel;

    private void Update()
    {
        if (engineMinigame == null || menuPanel == null) return;

        bool isEngineReady = engineMinigame.IsEngineOut;
        bool isQTEPlaying = qteMinigame != null && qteMinigame.IsPlaying;
        bool isBalancePlaying = balanceMinigame != null && balanceMinigame.IsPlaying;

        bool shouldShow = isEngineReady && !isQTEPlaying && !isBalancePlaying;

        if (menuPanel.activeSelf != shouldShow)
        {
            menuPanel.SetActive(shouldShow);
        }
    }
}