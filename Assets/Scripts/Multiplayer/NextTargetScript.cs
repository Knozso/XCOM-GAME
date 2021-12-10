using Mirror;
using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NextTargetScript : NetworkBehaviour, IPointerDownHandler
{
    public Camera shootCamera;
    public Button aimButton;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isServer)
        {
            CmdNextOnServer();
        }
        else
        {
            CmdNextOnClient();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdNextOnServer()
    {
        Stepper stepper = Stepper.Instance();
        if (stepper.GetCurrentPlayer().PlayerColor.Equals(Color.blue))
        {
            if (stepper.TargetedUnit != null)
            {
                Unit targetedUnit = stepper.TargetedUnit;
                Player enemyPlayer = stepper.GetEnemyPlayer();
                for (int i = 0; i < enemyPlayer.Units.Count; i++)
                {
                    if (targetedUnit.Equals(enemyPlayer.Units[i]))
                    {
                        if (i - 1 < 0)
                        {
                            targetedUnit = enemyPlayer.Units[enemyPlayer.Units.Count - 1];
                        }
                        else
                        {
                            targetedUnit = enemyPlayer.Units[i - 1];
                        }
                        break;
                    }
                }
                aimButton.GetComponent<AimScript>().SetTargetedUnit(targetedUnit);
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdNextOnClient()
    {
        Stepper stepper = Stepper.Instance();
        if (stepper.GetCurrentPlayer().PlayerColor.Equals(Color.red))
        {
            if (stepper.TargetedUnit != null)
            {
                Unit targetedUnit = stepper.TargetedUnit;
                Player enemyPlayer = stepper.GetEnemyPlayer();
                for (int i = 0; i < enemyPlayer.Units.Count; i++)
                {
                    if (targetedUnit.Equals(enemyPlayer.Units[i]))
                    {
                        if (i - 1 < 0)
                        {
                            targetedUnit = enemyPlayer.Units[enemyPlayer.Units.Count - 1];
                        }
                        else
                        {
                            targetedUnit = enemyPlayer.Units[i - 1];
                        }
                        break;
                    }
                }
                aimButton.GetComponent<AimScript>().SetTargetedUnit(targetedUnit);
            }
        }
    }
}
