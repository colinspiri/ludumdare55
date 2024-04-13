using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject objectToFollow;

    [SerializeField] private float lookAheadDistance;
    [SerializeField] private float moveSpeed;

    private Vector3 _targetPosition;
    private Vector3 _offset;

    // Start is called before the first frame update
    void Start()
    {
        _offset = transform.position - objectToFollow.transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (objectToFollow != null) {
            _targetPosition = objectToFollow.transform.position + _offset +
                              objectToFollow.transform.right * lookAheadDistance;
        }

        transform.position = Vector3.Lerp(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
    }
}