using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image bgImage;
    [SerializeField] private Sprite[] bgImages;

    [SerializeField] private Slider progressBar;
    [SerializeField] private float imageChangeInterval = 3f;
    [SerializeField] private CanvasFader fader;
    [SerializeField] private float fadeDuration = 0.5f;

    private string targetSpawnPointID = "";
    private GameObject currentCorePrefab;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // Tự động tìm Canvas ngoài cùng chứa LoadingPanel và giữ nó lại
            if (loadingPanel != null)
            {
                Transform canvasRoot = loadingPanel.transform.root;
                canvasRoot.SetParent(null);
                DontDestroyOnLoad(canvasRoot.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName, string spawnID = "", GameObject corePrefab = null)
    {
        targetSpawnPointID = spawnID;
        currentCorePrefab = corePrefab;
        loadingPanel.SetActive(true);
        StartCoroutine(StartLoadingWithFade(sceneName));
    }

    private IEnumerator StartLoadingWithFade(string sceneName)
    {
        if (fader != null) yield return StartCoroutine(fader.Fade(1f, fadeDuration));
        StartCoroutine(LoadSceneAsync(sceneName));
        StartCoroutine(ChangeImageRoutine());
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            yield return null;
        }

        operation.allowSceneActivation = true;
        yield return new WaitUntil(() => operation.isDone);

        // Chờ 1 frame để các Core trùng lặp trong scene mới tự tiêu diệt lẫn nhau
        yield return null;

        SetupCoreAndPlayer();

        if (fader != null) yield return StartCoroutine(fader.Fade(0f, fadeDuration));
        loadingPanel.SetActive(false);
    }

    private void SetupCoreAndPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            if (currentCorePrefab != null)
            {
                Instantiate(currentCorePrefab);
            }
            else
            {
                GameplayCoreManager.EnsureGameplayCoreExists();
            }
            playerObj = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObj != null && !string.IsNullOrEmpty(targetSpawnPointID))
        {
            MovePlayerToSpawnPoint(playerObj);
            targetSpawnPointID = "";
        }
    }

    private void MovePlayerToSpawnPoint(GameObject playerObj)
    {
        GameObject targetSpawn = GameObject.Find(targetSpawnPointID);

        if (targetSpawn != null)
        {
            VehicleEnterExit vehicleScript = FindFirstObjectByType<VehicleEnterExit>();

            if (vehicleScript != null)
            {
                vehicleScript.ForceEnterVehicleAndMove(targetSpawn.transform);
            }
            else
            {
                CharacterController cc = playerObj.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                playerObj.transform.position = targetSpawn.transform.position;
                playerObj.transform.rotation = targetSpawn.transform.rotation;

                Physics.SyncTransforms();

                if (cc != null) cc.enabled = true;
            }
        }
    }

    private IEnumerator ChangeImageRoutine()
    {
        if (bgImages.Length == 0 || bgImage == null) yield break;

        int index = 0;
        while (loadingPanel.activeSelf)
        {
            bgImage.sprite = bgImages[index];
            index = (index + 1) % bgImages.Length;
            yield return new WaitForSeconds(imageChangeInterval);
        }
    }
}