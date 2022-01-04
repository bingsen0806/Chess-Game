using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }
    public Server server;
    public Client client;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;

    private void Awake()
    {
        Instance = this;
    }

    //Buttons
    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        server.Init(8080);
        client.Init("127.0.0.1", 8080);
    }

    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        server.Init(8080);
        client.Init("127.0.0.1", 8080);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        Debug.Log("Online Connect");
        client.Init(addressInput.text, 8080);
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        menuAnimator.SetTrigger("OnlineMenu");
    }
}
