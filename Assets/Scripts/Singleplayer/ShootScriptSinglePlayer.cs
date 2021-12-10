using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Mirror;


public class ShootScriptSinglePlayer : MonoBehaviour, IPointerDownHandler
{
    public TextMeshProUGUI _resultText;
    public Camera shootCamera;
    public Button aimButton;

    public void OnPointerDown(PointerEventData eventData)
    {
        int percentage = aimButton.GetComponent<AimScriptSinglePlayer>().Percentage;
        Unit currentUnit = aimButton.GetComponent<AimScriptSinglePlayer>().CurrentUnit;
        Unit enemyUnit = aimButton.GetComponent<AimScriptSinglePlayer>().TargetedUnit;
        bool sniperMode = aimButton.GetComponent<AimScriptSinglePlayer>().SniperMode;
        int minDamage = 3;
        int maxDamage = 4;
        if (sniperMode)
        {
            minDamage = 4;
            maxDamage = 5;
        }
        PlayShootAndOtherAnimations(percentage, currentUnit, enemyUnit, minDamage, maxDamage);
    }

    public void PlayShootAndOtherAnimations(int percentage, Unit currentUnit, Unit enemyUnit, int minDamage, int maxDamage)
    {
        currentUnit.Target(enemyUnit);
        currentUnit.Shoot(percentage, enemyUnit, minDamage, maxDamage);
    }
}
