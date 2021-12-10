using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CancelScriptSinglePlayer : MonoBehaviour, IPointerDownHandler
{
    public Camera shootCamera;
    public Button aimButton;

    public void OnPointerDown(PointerEventData eventData)
    {
        aimButton.GetComponent<AimScriptSinglePlayer>().Cancel();
    }
}
