using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private bool isPlayerInside = false;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private float requiredHoldTime;
    private Pathfinding hunterPathfinding;

    private void Start()
    {
        hunterPathfinding = FindObjectOfType<Pathfinding>();
        // Randomize between 5 and 10 seconds
        requiredHoldTime = Random.Range(3f, 5f);
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Gathering)
        {
            ResetHold();
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!isHolding)
            {
                isHolding = true;
                if (hunterPathfinding != null)
                {
                    hunterPathfinding.isForcedChase = true;
                }
            }

            holdTimer += Time.deltaTime;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowGeneralPrompt(true, $"Mengambil... {(int)((holdTimer/requiredHoldTime)*100)}%");
            }

            if (holdTimer >= requiredHoldTime)
            {
                CollectItem();
            }
        }
        else if (isHolding)
        {
            // Cancel hold
            ResetHold();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowGeneralPrompt(true, "Tahan E untuk ambil");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Gathering)
            {
                isPlayerInside = true;
                GameManager.Instance.ShowGeneralPrompt(true, "Tahan E untuk ambil");
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
                ResetHold();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowGeneralPrompt(false);
                }
            }
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;
        if (hunterPathfinding != null)
        {
            hunterPathfinding.isForcedChase = false;
        }
    }

    private void CollectItem()
    {
        ResetHold();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGeneralPrompt(false);
            GameManager.Instance.ItemCollected();
        }
        
        Destroy(gameObject);
    }
}
