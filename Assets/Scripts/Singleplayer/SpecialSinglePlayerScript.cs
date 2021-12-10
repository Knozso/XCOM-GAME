using Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpecialSinglePlayerScript : MonoBehaviour, IPointerDownHandler
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
        if (unit.AbilityName == null || unit.AbilityName.Equals(""))
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
        if (Stepper.Instance().SelectedUnit != null)
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
