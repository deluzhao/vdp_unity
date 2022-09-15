using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class ActivateKey : MonoBehaviour
{
    public GameObject light_source;
    private Light2D component;
    public Text myText;
    private float score;
    private float time;
    private int time_caster;
    // Start is called before the first frame update
    void Start()
    {
        component = light_source.GetComponent<Light2D>();
        component.enabled = false;
        score = 0;
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(1))
        {
            component.enabled = true;
            time += Time.deltaTime;
            time_caster = (int)(time * 10);
            score = (float)time_caster / 10;

        } else
        {
            component.enabled = false;
        }
        myText.text = "Score: " + score;
    }
}
