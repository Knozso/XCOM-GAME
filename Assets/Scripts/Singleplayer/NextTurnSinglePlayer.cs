using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using UnityEngine.EventSystems;
using Mirror;

public class NextTurnSinglePlayer : MonoBehaviour, IPointerDownHandler
{
    public void Start()
    {
        Stepper.Instance().EnableButtonPressAction += () => { transform.parent.gameObject.SetActive(true); };
        Stepper.Instance().DisableButtonPressAction += () => { transform.parent.gameObject.SetActive(false); };
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Player player = Stepper.Instance().GetCurrentPlayer();
        foreach (Unit unit in player.Units)
        {
            unit.Actions = 0;
        }
        Stepper.Instance().Step();
    }
}
