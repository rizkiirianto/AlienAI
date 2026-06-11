using UnityEngine;

public class LockerHideTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private PlayerHiding currentPlayerHiding;
    private bool playerInside;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (!playerInside || currentPlayerHiding == null)
            return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Gathering)
        {
            if (currentPlayerHiding.IsHidden)
                currentPlayerHiding.Unhide();
            
            if (gameManager != null)
                gameManager.ShowHidePrompt(false);
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (gameManager != null)
            {
                if (currentPlayerHiding.IsHidden)
                    gameManager.ShowHidePrompt(true, "Press E to hide");
                else
                    gameManager.ShowHidePrompt(true, "Press E to exit");
            }

            if (currentPlayerHiding.IsHidden)
                currentPlayerHiding.Unhide();
            else
                currentPlayerHiding.Hide();

            if (gameManager != null)
            {
                if (currentPlayerHiding.IsHidden)
                    gameManager.ShowHidePrompt(true, "Press E to exit");
                else
                    gameManager.ShowHidePrompt(true, "Press E to hide");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHiding playerHiding = other.GetComponentInParent<PlayerHiding>();
        if (playerHiding == null)
            return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Gathering)
            return;

        currentPlayerHiding = playerHiding;
        playerInside = true;

        if (gameManager != null)
        {
            if (currentPlayerHiding.IsHidden)
                gameManager.ShowHidePrompt(true, "Press E to exit");
            else
                gameManager.ShowHidePrompt(true, "Press E to hide");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHiding playerHiding = other.GetComponentInParent<PlayerHiding>();
        if (playerHiding == null)
            return;

        if (currentPlayerHiding == playerHiding)
        {
            if (currentPlayerHiding.IsHidden)
                currentPlayerHiding.Unhide();

            playerInside = false;
            currentPlayerHiding = null;

            if (gameManager != null)
                gameManager.ShowHidePrompt(false);
        }
    }
}
