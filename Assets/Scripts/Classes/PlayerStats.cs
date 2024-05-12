using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float value = 0.5f;

    public TextMeshProUGUI programmingText;
    public TextMeshProUGUI hardwareText;
    public TextMeshProUGUI networkingText;
    public TextMeshProUGUI cybersecurityText;
    public TextMeshProUGUI designText;

    public Image programmingBar;
    public Image hardwareBar;
    public Image networkingBar;
    public Image cybersecurityBar;
    public Image designBar;

    //public float maxEnergy = 100f;
    //public float currentEnergy {  get; private set; }

    public Stat programming;
    public Stat hardware;
    public Stat networking;
    public Stat cybersecurity;
    public Stat design;

    private void Start()
    {
        UpdateStats();
    }

    //private void Awake()
    //{
    //    currentEnergy = maxEnergy;
    //}

    //public void TakeEnergy(float energy)
    //{
    //    energy -= programming.GetValue();
    //    energy = Mathf.Clamp(energy, 0, float.MaxValue);

    //    currentEnergy -= energy;
    //    Debug.Log($"{name} takes {energy} energy");

    //    if ( currentEnergy <= 0 )
    //    {
    //        Sleep();
    //    }
    //}

    //public virtual void Sleep()
    //{
    //    Debug.Log($"{name} sleeps");
    //}

    public void IncreaseStat(int stat)
    {
        switch (stat)
        {
            case 0: programming.SetValue(value); break;
            case 1: hardware.SetValue(value); break;
            case 2: networking.SetValue(value); break;
            case 3: cybersecurity.SetValue(value); break;
            case 4: design.SetValue(value); break;
            default: Debug.Log("Wrong skill index"); break;
        }

        UpdateStats();
    }



    public void UpdateStats()
    {
        programmingText.text = programming.GetValue().ToString("F1");
        hardwareText.text = hardware.GetValue().ToString("F1");
        networkingText.text = networking.GetValue().ToString("F1");
        cybersecurityText.text = cybersecurity.GetValue().ToString("F1");
        designText.text = design.GetValue().ToString("F1");

        programmingBar.fillAmount = programming.GetValue();
        hardwareBar.fillAmount = hardware.GetValue();
        networkingBar.fillAmount = networking.GetValue();
        cybersecurityBar.fillAmount = cybersecurity.GetValue();
        designBar.fillAmount = design.GetValue();
    }
}
