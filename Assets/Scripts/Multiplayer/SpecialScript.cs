using Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

public class SpecialScript : NetworkBehaviour, IPointerDownHandler
{
    private bool grenadeMode = false;

    public void Start()
    {
        Stepper.Instance().EnableButtonPressAction += () => { transform.parent.gameObject.SetActive(true); };
        Stepper.Instance().DisableButtonPressAction += () => { transform.parent.gameObject.SetActive(false); };
        Stepper.Instance().SelectedUnitChanged += u => ChangeSpecial(u);
        Stepper.Instance().GrenadeMode += b => GrenadeModeChanged(b);
        grenadeMode = false;
    }

    public void ChangeSpecial(Unit unit)
    {
        if(unit.AbilityName==null || unit.AbilityName.Equals(""))
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
            GetComponentInChildren<Text>().text = unit.AbilityName;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isServer)
        {
            CmdGrenadeOnServer();
        }
        else
        {
            CmdGrenadeOnClient();
        }

    }

    [Command(requiresAuthority = false)]
    private void CmdGrenadeOnServer()
    {
        if (Stepper.Instance().SelectedUnit != null && Stepper.Instance().GetCurrentPlayer().PlayerColor.Equals(Color.blue))
        {
            if (grenadeMode)
            {
                Stepper.Instance().SetGrenadeMode(false);
            }
            else
            {
                Stepper.Instance().SelectedUnit.SpecialAction();
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdGrenadeOnClient()
    {
        if (Stepper.Instance().SelectedUnit != null && Stepper.Instance().GetCurrentPlayer().PlayerColor.Equals(Color.red))
        {
            if (grenadeMode)
            {
                Stepper.Instance().SetGrenadeMode(false);
            }
            else
            {
                Stepper.Instance().SelectedUnit.SpecialAction();
            }
        }
    }

    private void GrenadeModeChanged(bool grenadeMode)
    {
        this.grenadeMode = grenadeMode;
    }
}
