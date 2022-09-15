using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChangeObsType : MonoBehaviour
{
    public Dropdown image_type;
    public int type;
    void OnMouseOver()
    {
        if (Input.GetMouseButton(0))
        {
            image_type.value = type;
        }
    }
}
