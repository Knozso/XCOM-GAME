using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using UnityEngine.EventSystems;
using Mirror;

public class NextTurn : NetworkBehaviour, IPointerDownHandler
{
    public void Start()
    {
        Stepper.Instance().EnableButtonPressAction += () => { transform.parent.gameObject.SetActive(true); };
        Stepper.Instance().DisableButtonPressAction += () => { transform.parent.gameObject.SetActive(false); };
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isServer)
        {
            CmdEndTurnForServer();
        }
        else
        {
            CmdEndTurnForClient();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdEndTurnForServer()
    {
        Player player = Stepper.Instance().GetCurrentPlayer();
        if(player.PlayerColor.Equals(Color.blue))
        {
            foreach (Unit unit in player.Units)
            {
                unit.Actions = 0;
            }
            Stepper.Instance().Step();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdEndTurnForClient()
    {
        Player player = Stepper.Instance().GetCurrentPlayer();
        if (player.PlayerColor.Equals(Color.red))
        {
            foreach (Unit unit in player.Units)
            {
                unit.Actions = 0;
            }
            Stepper.Instance().Step();
        }
    }
}
