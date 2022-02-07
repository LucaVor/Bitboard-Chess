using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public static Color legalMoveColour = Color.red;
    public MeshRenderer mr;
    public Color colour;
    public Color defaultColour;
    public bool setColour = false;

    void Start()
    {
        defaultColour = mr.material.color;
    }
    
    void Update()
    {
        if (setColour)
        {
            mr.material.color = colour;
            setColour = false;
        } else {
            mr.material.color = defaultColour;
        }
    }

    public void SetColourThisFrame( Color _c)
    {
        colour = _c;
        setColour = true;
    }
}