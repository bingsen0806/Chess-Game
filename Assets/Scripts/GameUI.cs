using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CameraAngle
{
    menu = 0,
    whiteTeam = 1,
    blackTeam = 2
}

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }
    public Server server;
    public Client client;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame;

    private void Awake()
    {
        Instance = this;
        RegisterEvents();
    }

    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }
        cameraAngles[(int)index].SetActive(true);
    }

    //Buttons
    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8080);
        client.Init("127.0.0.1", 8080);
    }

    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8080);
        client.Init("127.0.0.1", 8080);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
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

    public void OnLeaveToGameMenu()
    {
        ChangeCamera(CameraAngle.menu);
        menuAnimator.SetTrigger("StartMenu");
    }

    #region
    private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }

    private void UnregisterToEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }

    private void OnApplicationQuit()
    {
        UnregisterToEvents();
    }

    private void OnDestroy()
    {
        UnregisterToEvents();
    }

    private void OnStartGameClient(NetMessage msg)
    {
        menuAnimator.SetTrigger("InGameMenu");
    }
    #endregion
}
