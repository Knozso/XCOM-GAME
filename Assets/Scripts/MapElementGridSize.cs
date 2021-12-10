using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class MapElementGridSize : MonoBehaviour
    {
        public Vector2 Size;

        public void ChangeColor(Color color)
        {
            gameObject.GetComponent<Renderer>().material.color = color;
        }
    }
}
