using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrincipalPanel : MonoBehaviour {

    public Button jogarButton;
    public Button configurarButton;
    public Button sairButton;
    public GameObject configuracaoPanel;
    public GameObject connectingText;
    private bool startSimulator;

    // Use this for initialization
    void Start () {        
        AquariumProperties.configs = ConfigProperties.loadConfig();        
        jogarButton.onClick.AddListener(jogarButtonFunc);
        configurarButton.onClick.AddListener(configurarButtonFunc);
        sairButton.onClick.AddListener(sairButtonFunc);
    }

    // Update is called once per frame    
    private void Update()
    {
        if (startSimulator)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }

    private void OnApplicationQuit()
    {
        AquariumProperties.conn.stop();
    }    

    void jogarButtonFunc()
    {
        AquariumProperties.conn = new IUTConnect();
        AquariumProperties.conn.OnConnect += connectCallback;
        AquariumProperties.conn.start(AquariumProperties.configs.token);
        connectingText.SetActive(true);
    }

    void configurarButtonFunc()
    {
        configuracaoPanel.SetActive(true);
        GameObject.Find("Principal_Panel").SetActive(false);
    }    

    void connectCallback(bool connected)
    {
        startSimulator = true;
    }

    private void sairButtonFunc()
    {
        Application.Quit();
    }
}
