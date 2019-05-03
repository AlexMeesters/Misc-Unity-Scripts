using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Interactor interactor;

    [Header("Configuration")]
    [SerializeField] private float movementSpeed = 2;
    [SerializeField] private float rotationSpeed = 3;

    private void Awake()
    {
        interactor.OnCanInteract += OnCanInteract;
    }

    private void OnCanInteract(Interactable obj)
    {
        if (obj != null)
        {
            Debug.Log($"We can interact with {obj}");
        }
        else
        {
            Debug.Log("We can no longer interact");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = 0,
            z = Input.GetAxisRaw("Vertical")
        };

        movement = movement.normalized;
        transform.position = transform.position + (movement * (Time.deltaTime * movementSpeed));

        if (movement.x != 0 || movement.z != 0)
        {
            movement.y = transform.position.y;
            var targetRotation = Quaternion.LookRotation((movement).normalized, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation , Time.deltaTime * rotationSpeed);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        { 
            var interacted = interactor.TryInteract();

            if (interacted)
            {
                Debug.Log($"Interaction success!");
            }
        }
    }
}
