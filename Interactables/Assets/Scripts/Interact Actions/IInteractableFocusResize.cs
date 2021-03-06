﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IInteractableFocusResize : MonoBehaviour, IInteractable
{
    public void Interact() {}

    public void InteractionFocus(bool focused)
    {
        var scale = (focused) ? 0.75f : 0.5f;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
