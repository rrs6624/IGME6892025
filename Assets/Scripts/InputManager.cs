using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    PlayerCarManager playerCarManager;
    public InputActionReference move;

    // Update is called once per frame
    void Update()
    {
        Vector2 input = Vector2.zero;
        input = move.action.ReadValue<Vector2>();
    }
}
