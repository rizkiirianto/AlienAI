using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("GameFlowManager instance not found!");
        }
    }

    public void QuitGame()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.QuitGame();
        }
        else
        {
            Debug.LogError("GameFlowManager instance not found!");
        }
    }
}
