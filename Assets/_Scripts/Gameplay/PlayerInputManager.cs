﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    
    public PlayerController moveController;

    // Determines wheter Input.GetAxis or Input.GetAxisRaw is used
    public bool useRawAxisInput;


    private delegate void AxisInputFunction(float axis);
    private delegate void KeyInputFunction();

    Dictionary<string, KeyInputFunction> keyUpMappings;
    Dictionary<string, KeyInputFunction> keyHeldMappings;
    Dictionary<string, KeyInputFunction> keyDownMappings;
    Dictionary<string, AxisInputFunction> axisMappings;

    // Put default mappings in here, must be called in start
    private void InitialiseMappings()
    {
        if (moveController != null)
        {
            keyUpMappings = new Dictionary<string, KeyInputFunction>()
            {
                { "left shift", moveController.MakePlayerUnbouncy },
                { "left ctrl", moveController.MakePlayerUnsticky }
            };
            keyHeldMappings = new Dictionary<string, KeyInputFunction>()
            {
                { "left shift", moveController.MakePlayerBouncy },
                { "left ctrl", moveController.MakePlayerSticky }
            };
            keyDownMappings = new Dictionary<string, KeyInputFunction>()
            {
                { "space", moveController.Jump },
                { "left shift", moveController.MakePlayerBouncy },
                { "left ctrl", moveController.MakePlayerSticky }
            };
            axisMappings = new Dictionary<string, AxisInputFunction>()
            {
                { "Horizontal", moveController.AccelerateOnInput }
            };
        }
        else
        {
            keyUpMappings = new Dictionary<string, KeyInputFunction>();
            keyHeldMappings = new Dictionary<string, KeyInputFunction>();
            keyDownMappings = new Dictionary<string, KeyInputFunction>();
            axisMappings = new Dictionary<string, AxisInputFunction>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        moveController = GetComponent<PlayerController>();        InitialiseMappings();
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
        foreach (string key in keyUpMappings.Keys)
        {
            if (Input.GetKeyUp(key)) keyUpMappings[key]();

        }
    }

    void CallKeyHeldFunctions()
    {
        foreach (string key in keyHeldMappings.Keys)
        {
            if (Input.GetKey(key)) keyHeldMappings[key]();
        }
    }

    void CallKeyDownFunctions()
    {
        foreach (string key in keyDownMappings.Keys)
        {
            if (Input.GetKeyDown(key)) keyDownMappings[key]();
        }
    }
}