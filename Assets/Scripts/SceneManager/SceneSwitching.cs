using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitching : MonoBehaviour
{
    public static SceneSwitching Instance;
    public void GoGameScene()
    {
        SceneManager.LoadScene("2_Ingame_Scene");
    }

}
