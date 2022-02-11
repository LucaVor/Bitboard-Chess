using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public InputField ip;
    public static string IPAddress;
    public static string openingBookFilepath = "C:\\Users\\lbvor\\Bitboard Chess\\Assets\\Scripts\\AI\\OpeningBookBytes.txt";
    public static int DEPTH;
    public InputField depth;
    public InputField openingBook;
    public Toggle useQuiesence;
    public static bool boolQuiesence = true;

    public void OnEnterGameMultiplayer()
    {
        IPAddress = ip.text;
        SceneManager.LoadScene ("Multiplayer");
    }

    public void OnEnterGameSingleplayer()
    {
        DEPTH = int.Parse (depth.text);
        SceneManager.LoadScene ("Singleplayer");
        openingBookFilepath = openingBook.text;
        boolQuiesence = useQuiesence.isOn;
    }
}
