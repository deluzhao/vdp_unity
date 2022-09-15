using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public Button btn;
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(Begin);
    }

    void Begin()
    {
        Debug.Log("clicked");
        SceneManager.LoadScene("MainScene");
    }
}
