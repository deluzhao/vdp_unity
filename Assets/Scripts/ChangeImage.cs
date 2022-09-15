using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChangeImage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private string objectName;
    private int image_index;
    public Sprite[] images;
    public Button nextButton;
    // Start is called before the first frame update
    void Start()
    {
        objectName = this.name;
        spriteRenderer = GetComponent<SpriteRenderer>();
        image_index = -1;
        nextButton.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TaskOnClick()
    {
        image_index++;
        if (image_index == images.Length)
        {
            nextButton.interactable = false;
            //SceneManager.LoadScene("End", LoadSceneMode.Single);
        } else
        {
            spriteRenderer.sprite = images[image_index];
        }
    }
}
