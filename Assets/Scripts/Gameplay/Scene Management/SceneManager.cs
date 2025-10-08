using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField]
    private string m_TitleSceneName = "Title";
    
    [SerializeField]
    private string m_HomeScreenSceneName = "Home";
    
    [SerializeField]
    private string m_MainGameSceneName = "Main";
    
    [SerializeField]
    private string m_WinSceneName = "Win";
    
    [SerializeField]
    private string m_TransitionSceneName = "Transition";
    
    [Header("Transition Settings")]
    [SerializeField]
    private float m_TitleDisplayDuration = 4.0f; // increase default display time
    
    private bool m_TitleSequenceStarted = false;
    
    private static GameSceneManager s_Instance;
    private readonly System.Collections.Generic.List<GameObject> m_DeactivatedRoots = new System.Collections.Generic.List<GameObject>();
    
    public static GameSceneManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<GameSceneManager>();
                if (s_Instance == null)
                {
                    GameObject go = new GameObject("GameSceneManager");
                    s_Instance = go.AddComponent<GameSceneManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return s_Instance;
        }
    }
    
    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (s_Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Auto-start the title sequence once
        if (!m_TitleSequenceStarted)
        {
            m_TitleSequenceStarted = true;
            StartTitleSequence();
        }
    }
    
    public void StartTitleSequence()
    {
        StartCoroutine(TitleSequence());
    }
    
    private IEnumerator TitleSequence()
    {
        // Load title scene
        yield return StartCoroutine(LoadScene(m_TitleSceneName));
        
        // Wait for title to display
        yield return new WaitForSeconds(m_TitleDisplayDuration);
        
        // Load home screen
        yield return StartCoroutine(LoadScene(m_HomeScreenSceneName));
    }
    
    public void LoadMainGameAsCop()
    {
        RoleSelectionManager.Instance.SetPlayerRole(PlayerRole.Cop);
        StartCoroutine(LoadScene(m_MainGameSceneName));
    }
    
    public void LoadMainGameAsRobber()
    {
        RoleSelectionManager.Instance.SetPlayerRole(PlayerRole.Robber);
        StartCoroutine(LoadScene(m_MainGameSceneName));
    }
    
    public void LoadHomeScreen()
    {
        StartCoroutine(LoadScene(m_HomeScreenSceneName));
    }
    
    public void LoadWinScreen(string winner = "")
    {
        // Optionally store winner somewhere global if a win UI needs it later
        StartCoroutine(LoadScene(m_WinSceneName));
    }
    
    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Wait a frame for the scene to fully load
        yield return null;
        
        // Ensure UI is interactive for HomeScreen
        if (sceneName == m_HomeScreenSceneName)
        {
            Debug.Log("Setting up HomeScreen UI interactivity...");
            EnsureUIInteractive();
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    private void EnsureUIInteractive()
    {
        Debug.Log("Starting UI interactivity setup...");
        
        // Ensure canvases are active and have proper sorting order
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Found {canvases.Length} Canvas(es)");
        
        foreach (Canvas canvas in canvases)
        {
            if (!canvas.name.Contains("Fade") && !canvas.name.Contains("fade"))
            {
                canvas.gameObject.SetActive(true);
                canvas.enabled = true;
                
                // Ensure the main UI canvas has a high sorting order
                if (canvas.name.Contains("Canvas") || canvas.name == "Canvas")
                {
                    canvas.sortingOrder = 100;
                    Debug.Log($"Set canvas '{canvas.name}' sorting order to {canvas.sortingOrder}");
                }
            }
        }
        
        // Ensure buttons are interactive
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log($"Found {buttons.Length} Button(s)");
        
        foreach (Button button in buttons)
        {
            button.interactable = true;
            button.gameObject.SetActive(true);
            Debug.Log($"Button '{button.name}' - Interactable: {button.interactable}, Active: {button.gameObject.activeInHierarchy}");
        }
        
        // Ensure GraphicRaycasters are enabled
        GraphicRaycaster[] raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        Debug.Log($"Found {raycasters.Length} GraphicRaycaster(s)");
        
        foreach (GraphicRaycaster raycaster in raycasters)
        {
            raycaster.enabled = true;
            Debug.Log($"Enabled GraphicRaycaster on '{raycaster.gameObject.name}'");
        }
        
        Debug.Log("UI interactivity setup complete");
    }
    
    public void LoadTransitionScreen()
    {
        StartCoroutine(LoadTransitionSceneAdditive(m_TransitionSceneName));
    }
    
    public void ContinueToNextPlayer()
    {
        StartCoroutine(UnloadTransitionScene(m_TransitionSceneName));
    }
    
    private IEnumerator LoadTransitionSceneAdditive(string sceneName)
    {
        DeactivateActiveSceneRoots();
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        yield return null;
    }
    
    private IEnumerator UnloadTransitionScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
        }
        ReactivateActiveSceneRoots();
        yield return null;
    }

    private void DeactivateActiveSceneRoots()
    {
        m_DeactivatedRoots.Clear();
        Scene active = SceneManager.GetActiveScene();
        var roots = active.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            // Skip if already inactive
            if (!root.activeSelf) continue;
            // Do not touch EventSystem-like global systems if you prefer; comment out next lines if needed
            // if (root.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) continue;
            m_DeactivatedRoots.Add(root);
            root.SetActive(false);
        }
    }

    private void ReactivateActiveSceneRoots()
    {
        foreach (GameObject go in m_DeactivatedRoots)
        {
            if (go != null)
            {
                go.SetActive(true);
            }
        }
        m_DeactivatedRoots.Clear();
    }
    
    [ContextMenu("Force Fix Buttons")]
    public void ForceFixButtons()
    {
        Debug.Log("Manually fixing button interactivity...");
        EnsureUIInteractive();
        
        // Additional button fixes
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            // Force enable the button component
            button.enabled = true;
            button.interactable = true;
            button.gameObject.SetActive(true);
            
            // Check if button has OnClick events
            if (button.onClick.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning($"Button '{button.name}' has no OnClick events assigned!");
            }
            else
            {
                Debug.Log($"Button '{button.name}' has {button.onClick.GetPersistentEventCount()} OnClick events");
            }
        }
    }
}
