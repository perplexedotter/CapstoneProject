using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSave : MonoBehaviour
{

    Scene scene;
    [SerializeField] GameObject ResumeButton;
    [SerializeField] Text ResumeButtonText;
    public int sceneIndex;
    

    // Use this for initialization
    void Start()
    {
        scene = SceneManager.GetActiveScene();
        Debug.Log(scene.name);
        if (scene.name == "mainMenu")
        {
            if (PlayerPrefs.HasKey("Map"))
            {
                //Debug.Log(PlayerPrefs.GetString("Map"));
                string index = PlayerPrefs.GetString("Map");

                if (index == "map1")
                {
                    sceneIndex = 1;
                }

                else if (index == "map2")
                {
                    ResumeButton.gameObject.SetActive(true);
                    sceneIndex = 2;
                    ResumeButtonText.text += "Map 2";
                }

                else if (index == "map3")
                {
                    ResumeButton.gameObject.SetActive(true);
                    sceneIndex = 3;
                    ResumeButtonText.text += "Map 3";
                }
            }
            else
            {
                PlayerPrefs.SetString("Map", "map1");
            }
        }

        else if (scene.name == "map1")
        {
            PlayerPrefs.SetString("Map", "map1");
            if (PlayerPrefs.HasKey("Map"))
            {
                //Debug.Log(PlayerPrefs.GetString("Map"));
            }
        }

        else if (scene.name == "map2")
        {
            PlayerPrefs.SetString("Map", "map2");
            if (PlayerPrefs.HasKey("Map"))
            {
                //Debug.Log(PlayerPrefs.GetString("Map"));
            }
        }

        else if (scene.name == "map3")
        {
            PlayerPrefs.SetString("Map", "map3");
            if (PlayerPrefs.HasKey("Map"))
            {
                //Debug.Log(PlayerPrefs.GetString("Map"));
            }
        }

        else if (scene.name == "credits")
        {
            PlayerPrefs.SetString("Map", "map1");
            if (PlayerPrefs.HasKey("Map"))
            {
                //Debug.Log(PlayerPrefs.GetString("Map"));
            }
        }

    }

    public void LoabByIndex()
    {
        Debug.Log(sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
