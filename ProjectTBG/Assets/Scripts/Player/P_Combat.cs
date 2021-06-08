using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class P_Combat : MonoBehaviour
{
    public static P_Combat instance;

    public bool canAttack;
    public bool inputReceived;

    private void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        canAttack = true;
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void Attack(InputAction.CallbackContext context) {
        if(context.performed) {
            if(canAttack) {
                inputReceived = true;
                canAttack = false;
            } else {
                return;
            }
        }
    }

    public void InputManager() {
        if(!canAttack) {
            canAttack = true;
        } else {
            canAttack = false;
        }
    }
}
