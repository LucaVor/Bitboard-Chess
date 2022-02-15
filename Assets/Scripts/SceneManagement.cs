using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public InputField ip;
    public static string IPAddress;
    public static string openingBookFilepath = "";
    public static int DEPTH;
    public InputField depth;
    public InputField openingBook;
    public Toggle useQuiesence;
    public Toggle _playAsWhite;
    public static bool boolQuiesence = true;
    public static bool playAsWhite = true;
    public GameObject fileBrowser;
    public InputField directory;
    public Text filePathDis;

    void Update ()
    {
        if (SimpleFileBrowser.FileBrowser.txtPath != "Null")
        {
            filePathDis.text = SimpleFileBrowser.FileBrowser.txtPath;
        }
    }

    public void OnEnterGameMultiplayer()
    {
        IPAddress = ip.text;
        SceneManager.LoadScene ("Multiplayer");
    }

    public void OnEnterGameSingleplayer()
    {
        DEPTH = int.Parse (depth.text);
        SceneManager.LoadScene ("Singleplayer");
        openingBookFilepath = SimpleFileBrowser.FileBrowser.txtPath;
        boolQuiesence = useQuiesence.isOn;
        playAsWhite = _playAsWhite.isOn;
    }

    public void OpenFileBrowser ()
    {
        fileBrowser.SetActive (true);

        string startingPath = "";
        // #if UNITY_EDITOR
        // startingPath = Application.dataPath + "/Resources/";
        // #else
        startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        // #endif

        directory.text = startingPath;
    }
}