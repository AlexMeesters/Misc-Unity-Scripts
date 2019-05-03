using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interacts with objects that implement the IInteractable interface
/// </summary>
public class Interactor : MonoBehaviour
{
    /// <summary>
    /// Sends a callback when it is able to interact. When not able to interact it will send null.
    /// </summary>
    public System.Action<Interactable> OnCanInteract = delegate { };

    [Header("Configuration")]
    [Tooltip("When focussing objects, should we compare the look angle?")]
    [SerializeField] private bool prioritizeWithAngle = true;
    [Tooltip("0 means it will only focus on objects that are at least within 90 degrees of the forward vector." +
        " 1 = exactly in front. -1 is exactly behind")]
    [SerializeField] private float requiredDot = 0;

    // All interactable objects the interactor is currently within range of
    private List<Interactable> interactableObjects = new List<Interactable>();

    public Interactable activeInteractable { get; private set; }

    public bool TryInteract()
    {
        if (activeInteractable != null)
        {
            activeInteractable.Interact();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            var go = other.gameObject;
            var interactable = go.GetComponent<Interactable>();

            if (interactable != null)
            {
                interactableObjects.Add(interactable);
                UpdateInteractables();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            var go = other.gameObject;
            var interactable = go.GetComponent<Interactable>();


            if (interactable != null)
            {
                interactableObjects.Remove(interactable);
                UpdateInteractables();
            }
        }
    }

    private void Update()
    {
        // In case we move the object, we want to check if there have been any interaction changes.
        if (transform.hasChanged)
        {
            UpdateInteractables();
        }
    }

    private void UpdateInteractables()
    {
        var newInteractable = CalculateClosestInteractable();

        if (newInteractable != activeInteractable)
        {
            if (activeInteractable != null)
            {
                // Notify the interfaces of the last active interactable that it is no longer being focused.
                activeInteractable.Focus(false);
            }

            if (newInteractable != null)
            {
                // Notify the new interactable that it is being focussed.
                newInteractable.Focus(true);
            }

            // We update the active interactable to the new interactable
            activeInteractable = newInteractable;

            // We send out an event that we can interact
            OnCanInteract.Invoke(activeInteractable);
        }
    }

    /// <summary>
    /// Compares all interactables within range.
    /// </summary>
    /// <returns></returns>
    private Interactable CalculateClosestInteractable()
    {
        if (interactableObjects.Count == 0)
            return null;

        // Sort objects based on distance
        int index = -1;
        float closestDistance = float.MaxValue;
        Vector3 position = this.transform.position;

        // Reverse forloop, allows for removal of elements, since removal of list elements
        // Shifts the order towards the zero index.
        for (int i = interactableObjects.Count - 1; i >= 0; i--)
        {
            if (interactableObjects[i] == null)
            {
                // Remove all references in case the interactable has been removed.
                interactableObjects.RemoveAt(i);
                continue;
            }

            Vector3 direction = (interactableObjects[i].transform.position - position);

            float distance = direction.sqrMagnitude;

            // We also sort based on the viewing vector. if dotProduct == 1 it means
            // it is looking straight at the target. -1 means it is the opposite.
            // You can comment this part out if you don't want it to happen.
            float dotProduct = Vector3.Dot(transform.forward, direction);
            if (!(dotProduct > requiredDot))
            {
                continue;
            }

            distance -= dotProduct;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                index = i;
            }
        }

        if (index != -1)
        {
            if (interactableObjects.Count > index)
            {
                return interactableObjects[index];
            }
            else
            {
                return null;
            }
        }
        else
            return null;
    }
}

