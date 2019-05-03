using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IInteractableInteractDestroy : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        GameObject.Destroy(this.gameObject);
    }

    public void InteractionFocus(bool focussed) { }
}
