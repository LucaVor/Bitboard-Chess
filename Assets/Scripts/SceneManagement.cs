using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public InputField ip;
    public static string IPAddress;

    public void OnEnterGameMultiplayer()
    {
        IPAddress = ip.text;
        SceneManager.LoadScene ("Multiplayer");
    }

    public void OnEnterGameSingleplayer()
    {
        SceneManager.LoadScene ("Singleplayer");
    }
}
