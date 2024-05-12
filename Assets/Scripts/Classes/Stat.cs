using UnityEngine;

[System.Serializable]
public class Stat
{
    [SerializeField]
    private float baseValue;

    public float GetValue ()
    { 
        return baseValue;
    }

    public float SetValue (float value)
    {
        //float x = baseValue + value;
        //return Mathf.Clamp(x, 0f, 1f);
        return baseValue += value;
    }
}
