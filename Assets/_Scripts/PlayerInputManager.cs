using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    private MovementController moveController;
    // Start is called before the first frame update
    void Start()
    {
        moveController = GetComponent<MovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
