using UnityEngine;

public class PlayerHiding : MonoBehaviour
{
    private Renderer[] cachedRenderers;
    private Movement movement;

    public bool IsHidden { get; private set; }

    private void Awake()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        movement = GetComponent<Movement>();
    }

    public void Hide()
    {
        if (IsHidden)
            return;

        IsHidden = true;
        if (movement != null)
            movement.SetMovementLocked(true);
        SetVisible(false);
    }

    public void Unhide()
    {
        if (!IsHidden)
            return;

        IsHidden = false;
        if (movement != null)
            movement.SetMovementLocked(false);
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
                cachedRenderers[i].enabled = visible;
        }
    }
}
