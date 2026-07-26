using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwap : MonoBehaviour
{
    public void SceneSwapper(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
