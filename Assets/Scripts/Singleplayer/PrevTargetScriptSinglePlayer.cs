using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PrevTargetScriptSinglePlayer : MonoBehaviour, IPointerDownHandler
{
    public Camera shootCamera;
    public Button aimButton;

    public void OnPointerDown(PointerEventData eventData)
    {
        Stepper stepper = Stepper.Instance();
        if (stepper.TargetedUnit != null)
        {
            Unit targetedUnit = stepper.TargetedUnit;
            Player enemyPlayer = stepper.GetEnemyPlayer();
            for (int i = 0; i < enemyPlayer.Units.Count; i++)
            {
                if (targetedUnit.Equals(enemyPlayer.Units[i]))
                {
                    if (i + 1 == enemyPlayer.Units.Count)
                    {
                        targetedUnit = enemyPlayer.Units[0];
                    }
                    else
                    {
                        targetedUnit = enemyPlayer.Units[i + 1];
                    }
                    break;
                }
            }
            aimButton.GetComponent<AimScriptSinglePlayer>().SetTargetedUnit(targetedUnit);
        }
    }
}
