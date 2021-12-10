using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVsPlayerScript : MonoBehaviour
{
    public void StartPlayerVsPlayer()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerVSAi");
        GameObject[] multObjs = GameObject.FindGameObjectsWithTag("Multiplayer");
        foreach (GameObject obj in objs)
        {
            Destroy(obj);
        }
        foreach (GameObject obj in multObjs)
        {
            Destroy(obj);
        }
        SceneManager.LoadScene("SingleplayerScene");
    }
}
