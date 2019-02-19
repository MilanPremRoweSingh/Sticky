using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    
    public static MovementController moveController;

    // Determines wheter Input.GetAxis or Input.GetAxisRaw is used
    public bool useRawAxisInput;


    private delegate void AxisInputFunction(float axis);
    private delegate void KeyInputFunction();

    Dictionary<KeyCode, KeyInputFunction> keyUpMappings;
    Dictionary<KeyCode, KeyInputFunction> keyHeldMappings;
    Dictionary<KeyCode, KeyInputFunction> keyDownMappings;
    Dictionary<string, AxisInputFunction> axisMappings;

    // Put default mappings in here, must be called in start
    private void InitialiseMappings()
    {
        if (moveController != null)
        {
            keyUpMappings = new Dictionary<KeyCode, KeyInputFunction>()
            {
            };
            keyHeldMappings = new Dictionary<KeyCode, KeyInputFunction>()
            {
            };
            keyDownMappings = new Dictionary<KeyCode, KeyInputFunction>()
            {
                { KeyCode.Space, moveController.Jump }
            };
            axisMappings = new Dictionary<string, AxisInputFunction>()
            {
                { "Horizontal", moveController.AccelerateOnInput }
            };
        }
        else
        {
            keyUpMappings = new Dictionary<KeyCode, KeyInputFunction>();
            keyHeldMappings = new Dictionary<KeyCode, KeyInputFunction>();
            keyDownMappings = new Dictionary<KeyCode, KeyInputFunction>();
            axisMappings = new Dictionary<string, AxisInputFunction>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        moveController = GetComponent<MovementController>();
        InitialiseMappings();
    }



    // Update is called once per frame
    void Update()
    {
        // Call Axis functions with appropriate input
        if (axisMappings != null) CallAxisFunctions();

        // Call Button Up methods for keys pressed
        if (keyUpMappings != null) CallKeyUpFunctions();

        // Call GetKey methods for keys pressed
        if (keyHeldMappings != null) CallKeyHeldFunctions();

        // Call Key Downmethods for keys pressed
        if (keyDownMappings != null) CallKeyDownFunctions();

    }

    void CallAxisFunctions()
    {
        foreach (string axis in axisMappings.Keys)
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
    }

    void CallKeyUpFunctions()
    {
        foreach (KeyCode key in keyUpMappings.Keys)
        {
            if (Input.GetKeyUp(key)) keyUpMappings[key]();
        }
    }

    void CallKeyHeldFunctions()
    {
        foreach (KeyCode key in keyHeldMappings.Keys)
        {
            if (Input.GetKey(key)) keyHeldMappings[key]();
        }
    }

    void CallKeyDownFunctions()
    {
        foreach (KeyCode key in keyDownMappings.Keys)
        {
            if (Input.GetKeyDown(key)) keyDownMappings[key]();
        }
    }
}
