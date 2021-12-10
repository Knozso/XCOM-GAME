using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartOnlineScript : MonoBehaviour
{
    public MyNetworkManager networkManager;
    public Canvas canvas;

    public void EnableNetworkManager()
    {
        canvas.gameObject.SetActive(false);
        //networkManager.gameObject.SetActive(true);
        SceneManager.LoadScene("MultiplayerScene");
    }
}
