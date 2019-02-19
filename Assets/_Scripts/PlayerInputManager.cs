using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    private MovementController movementController;

    // Determines wheter Input.GetAxis or Input.GetAxisRaw is used
    public bool useRawAxisInput;

    // Start is called before the first frame update
    void Start()
    {
        movementController = GetComponent<MovementController>();
    }

    private delegate void AxisInputFunction(float axis);
    private delegate void KeyInputFunction();

    // Default Key bindings here, key/axis -> function
    Dictionary<string, string> bindings = new Dictionary<string, string>() 
    { 
        { "Horizontal", "AccelerateOnInput" },
        { "Space", "Jump" }
    };

    Dictionary<KeyCode, KeyInputFunction> keyUpMappings = new Dictionary<KeyCode, KeyInputFunction>();
    Dictionary<KeyCode, KeyInputFunction> keyMappings = new Dictionary<KeyCode, KeyInputFunction>();
    Dictionary<KeyCode, KeyInputFunction> keyDownMappings = new Dictionary<KeyCode, KeyInputFunction>();
    Dictionary<string, AxisInputFunction> axisMappings = new Dictionary<string, AxisInputFunction>();

    // Update is called once per frame
    void Update()
    {
        // Call Axis functions with appropriate input
        foreach( string axis in axisMappings.Keys )
        {
            float axisValue = 0;
            if (useRawAxisInput)
            {
                axisValue = Input.GetAxisRaw(axis);
            }
            else
            {
                axisValue = Input.GetAxis(axis);
            }
            axisMappings[axis](axisValue);
        }

        // Call Button Up methods for keys pressed
        foreach ( KeyCode key in keyUpMappings.Keys )
        {
            if (Input.GetKeyUp(key)) keyUpMappings[key]();
        }

        // Call GetKey methods for keys pressed
        foreach (KeyCode key in keyMappings.Keys)
        {
            if (Input.GetKey(key)) keyMappings[key]();
        }

        // Call Key Downmethods for keys pressed
        foreach (KeyCode key in keyDownMappings.Keys)
        {
            if (Input.GetKeyDown(key)) keyDownMappings[key]();
        }

    }
}
