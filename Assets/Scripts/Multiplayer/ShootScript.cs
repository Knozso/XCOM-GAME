using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class ShootScript : NetworkBehaviour, IPointerDownHandler
{
    public TextMeshProUGUI _resultText;
    public Camera shootCamera;
    public Button aimButton;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isServer)
        {
            CmdShootOnServer();
        }
        else
        {
            CmdShootOnClient();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdShootOnServer()
    {
        if(Stepper.Instance().GetCurrentPlayer().PlayerColor.Equals(Color.blue))
        {
            int percentage = aimButton.GetComponent<AimScript>().Percentage;
            Unit currentUnit = aimButton.GetComponent<AimScript>().CurrentUnit;
            Unit enemyUnit = aimButton.GetComponent<AimScript>().TargetedUnit;
            bool sniperMode = aimButton.GetComponent<AimScript>().SniperMode;
            int minDamage = 3;
            int maxDamage = 4;
            if (sniperMode)
            {
                minDamage = 4;
                maxDamage = 5;
            }
            PlayShootAndOtherAnimations(percentage, currentUnit, enemyUnit, minDamage, maxDamage);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdShootOnClient()
    {
        if (Stepper.Instance().GetCurrentPlayer().PlayerColor.Equals(Color.red))
        {
            int percentage = aimButton.GetComponent<AimScript>().Percentage;
            Unit currentUnit = aimButton.GetComponent<AimScript>().CurrentUnit;
            Unit enemyUnit = aimButton.GetComponent<AimScript>().TargetedUnit;
            bool sniperMode = aimButton.GetComponent<AimScript>().SniperMode;
            int minDamage = 3;
            int maxDamage = 4;
            if(sniperMode)
            {
                minDamage = 4;
                maxDamage = 5;
            }
            PlayShootAndOtherAnimations(percentage, currentUnit, enemyUnit, minDamage, maxDamage);
        }
    }

    public void PlayShootAndOtherAnimations(int percentage, Unit currentUnit, Unit enemyUnit, int minDamage, int maxDamage)
    {
        currentUnit.Target(enemyUnit);
        currentUnit.Shoot(percentage, enemyUnit, minDamage, maxDamage);
    }
}
