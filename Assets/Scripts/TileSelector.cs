using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    void OnMouseEnter()
    {
        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        renderer.material.color = Color.blue;
    }

    void OnMouseExit()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        renderer.material.color = Color.white;
    }
}
