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
        
        result = new Vector2(InputManager.Instance.horizontalLookAxis, InputManager.Instance.verticalLookAxis);
        
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
            var screenToWorldPoint = Camera.main.ScreenToWorldPoint(point);
            Vector2 lookDirection = screenToWorldPoint - transform.position;

            transform.right = lookDirection;
        }
    }
}