using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVsAiScript : MonoBehaviour
{
    public void StartPlayerVsAi()
    {
        GameObject[] multObjs = GameObject.FindGameObjectsWithTag("Multiplayer");
        foreach (GameObject obj in multObjs)
        {
            Destroy(obj);
        }
        SceneManager.LoadScene("SingleplayerScene");
    }
}
