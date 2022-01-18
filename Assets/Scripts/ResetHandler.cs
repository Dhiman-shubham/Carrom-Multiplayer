using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetHandler : MonoBehaviour
{
    public void handleReset()
    {
        SceneManager.LoadScene(0);
    }
}
