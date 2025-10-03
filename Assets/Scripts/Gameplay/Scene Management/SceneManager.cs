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
    private string m_HomeScreenSceneName = "HomeScreen";
    
    [SerializeField]
    private string m_MainGameSceneName = "Main";
    
    [Header("Transition Settings")]
    [SerializeField]
    private float m_TitleDisplayDuration = 2.0f;
    
    private static GameSceneManager s_Instance;
    
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
        // Auto-start the title sequence
        StartTitleSequence();
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
