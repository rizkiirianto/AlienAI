using UnityEngine;

public class EscapeLift : MonoBehaviour
{
    private bool isPlayerInside = false;

    private void Update()
    {
        if (!isPlayerInside) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Escape)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowGeneralPrompt(false);
                    
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.ReturnToMainMenu();
                }
                else
                {
                    Debug.Log("GameFlowManager not found! Cannot return to Main Menu.");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Escape)
            {
                isPlayerInside = true;
                GameManager.Instance.ShowGeneralPrompt(true, "Tekan E untuk kabur");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isPlayerInside)
            {
                isPlayerInside = false;
                if (GameManager.Instance != null)
                {
                    // Kembalikan prompt "keluar" jika pemain keluar dari collider lift tanpa menekan E
                    GameManager.Instance.ShowGeneralPrompt(true, "keluar");
                }
            }
        }
    }
}
