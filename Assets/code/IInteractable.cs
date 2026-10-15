using UnityEngine;

public interface IInteractable
{
    void OnLookStart();
    void OnLookEnd();
    void Interact();
    Transform GetTransform();
}
