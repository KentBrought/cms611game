using UnityEngine;

public class RoleSelectionManager : MonoBehaviour
{
    private static RoleSelectionManager s_Instance;
    private PlayerRole m_SelectedRole = PlayerRole.Robber; // Default role
    
    public static RoleSelectionManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<RoleSelectionManager>();
                if (s_Instance == null)
                {
                    GameObject go = new GameObject("RoleSelectionManager");
                    s_Instance = go.AddComponent<RoleSelectionManager>();
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
    
    public void SetPlayerRole(PlayerRole role)
    {
        m_SelectedRole = role;
        Debug.Log($"Player role set to: {role}");
    }
    
    public PlayerRole GetPlayerRole()
    {
        return m_SelectedRole;
    }
}
