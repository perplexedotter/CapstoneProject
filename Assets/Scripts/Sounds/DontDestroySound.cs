 using UnityEngine;
 using System.Collections;
 using UnityEngine.SceneManagement;
 
 public class DontDestroySound : MonoBehaviour {
 
     void Awake ()
     {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Music");
        if (objs.Length > 1)
            Destroy(this.gameObject);
 
        DontDestroyOnLoad(this.gameObject);
 
     }
 
     void Update()
     {
		Scene scene = SceneManager.GetActiveScene();
        Debug.Log("Active scene is '" + scene.name + "'.");
        if (scene.name == "map1")
        {
            Destroy(this.gameObject);
        }
     }
 }