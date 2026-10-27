using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual string Prompt
    {
        get { return "Press E to interact"; }
    }

    public virtual bool CanInteract()
    {
        return true;
    }

    public abstract void Interact();
}
