using UnityEngine;
using UnityEngine.SceneManagement;
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
    private float m_FadeDuration = 1.0f;
    
    [SerializeField]
    private FadeController m_FadeController;
    
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
        yield return StartCoroutine(LoadSceneWithFade(m_TitleSceneName));
        
        // Wait for title to display (you can adjust this duration)
        yield return new WaitForSeconds(2.0f);
        
        // Fade to home screen
        yield return StartCoroutine(LoadSceneWithFade(m_HomeScreenSceneName));
    }
    
    public void LoadMainGameAsCop()
    {
        RoleSelectionManager.Instance.SetPlayerRole(PlayerRole.Cop);
        StartCoroutine(LoadSceneWithFade(m_MainGameSceneName));
    }
    
    public void LoadMainGameAsRobber()
    {
        RoleSelectionManager.Instance.SetPlayerRole(PlayerRole.Robber);
        StartCoroutine(LoadSceneWithFade(m_MainGameSceneName));
    }
    
    public void LoadHomeScreen()
    {
        StartCoroutine(LoadSceneWithFade(m_HomeScreenSceneName));
    }
    
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // Fade out
        if (m_FadeController != null)
        {
            yield return StartCoroutine(m_FadeController.FadeOut(m_FadeDuration));
        }
        
        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Fade in
        if (m_FadeController != null)
        {
            yield return StartCoroutine(m_FadeController.FadeIn(m_FadeDuration));
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
