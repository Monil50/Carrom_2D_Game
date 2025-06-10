using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    // Call this to change the scene to the one in 'sceneToLoad'
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad) && Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene '" + sceneToLoad + "' cannot be loaded. Make sure it is added to Build Settings.");
        }
    }

    // Call this to quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();

#if UNITY_EDITOR
        // Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
