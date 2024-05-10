using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public Vector2 movementInput, lookInput;
    public bool jumpPressed, isSprinting, isAiming, isFiring;

        public void OnMove(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }
        public void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) {
                jumpPressed = true;
             }else if(context.canceled){
                jumpPressed = false;
             }
        }
        public void OnRunning(InputAction.CallbackContext context)
        {
            if (context.performed) {
                isSprinting = true;
             }else if(context.canceled){
                isSprinting = false;
             }
        }
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
            }
        }

        public void OnAiming(InputAction.CallbackContext context){
            if (context.performed) {
                isAiming = true;
             }else if(context.canceled){
                isAiming = false;
             }
        }
        public void OnShoot(InputAction.CallbackContext context){
            if (context.performed) {
                isFiring = true;
             }else if(context.canceled){
                isFiring = false;
             }
        }

}
