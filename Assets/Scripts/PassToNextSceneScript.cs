using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassToNextSceneScript : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
