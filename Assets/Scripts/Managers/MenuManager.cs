using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private void Start()
    {
        transform.gameObject.SetActive(true);
    }

    //public void ToggleSiblings(bool x)
    //{
    //    Transform parent = transform.parent;

    //    foreach (Transform child in parent)
    //        if (child != transform) child.gameObject.SetActive(x);
    //}
}
