using System;
using UnityEngine;
using ScriptableObjectArchitecture;


// This class controls player movement
// Script is based from Coursera Game Design and Development 1: 2D Shooter project from MSU
public class PlayerController : MonoBehaviour {
    public static PlayerController Instance;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float backwardsMoveSpeed;
    
    public enum AimModes { AimTowardsMouse, AimForwards };
    public AimModes aimMode = AimModes.AimTowardsMouse;

 
    public enum MovementModes {  FreeRoam};
    public MovementModes movementMode = MovementModes.FreeRoam;
    
    private bool canAimWithMouse => aimMode == AimModes.AimTowardsMouse;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
    }

    void Update()
    {
        HandleInput();
    }
    

    private void HandleInput()
    {
        // Find the position that the player should look at
        Vector2 lookPosition = GetLookPosition();
        // Get movement input from the inputManager
        Vector3 movementVector = new Vector3(InputManager.Instance.horizontalMoveAxis, InputManager.Instance.verticalMoveAxis, 0);
        // Move the player
        MovePlayer(movementVector);
        LookAtPoint(lookPosition);
    }

    public Vector2 GetLookPosition()
    {
        Vector2 result = transform.up;
        
        if (aimMode != AimModes.AimForwards)
        {
            result = new Vector2(InputManager.Instance.horizontalLookAxis, InputManager.Instance.verticalLookAxis);
        }
        else
        {
            result = transform.up;
        }
        return result;
    }


    private void MovePlayer(Vector3 movement) {

        // when moving forwards, move at moveSpeed
        // when moving backwards, move at backwardsMoveSpeed
        // if moving sideways, move at speed = halfway between them 

        float dot = Vector3.Dot(movement, transform.right);
        float f = (dot + 1) / 2.0f;
        float speed = Mathf.Lerp(backwardsMoveSpeed, moveSpeed, f);
        // Debug.Log("dot = " + dot + ", f = " + f + ", speed = " + speed);
        
        // Move the player's transform
        transform.position = transform.position + (movement * Time.deltaTime * speed);
    }

    private void LookAtPoint(Vector3 point)
    {
        if (Time.timeScale > 0)
        {
            // Rotate the player to look at the mouse.
            Vector2 lookDirection = Camera.main.ScreenToWorldPoint(point) - transform.position;

            if (canAimWithMouse)
            {
                transform.right = lookDirection;
            }
            else
            {
                if (rigidBody != null)
                {
                    rigidBody.freezeRotation = true;
                }
            }
        }
    }
}