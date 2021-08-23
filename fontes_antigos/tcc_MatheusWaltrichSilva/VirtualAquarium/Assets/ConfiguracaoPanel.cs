using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class ConfiguracaoPanel : MonoBehaviour {

    public Button confirmarButton;
    public Button voltarButton;
    public InputField SSIDInput;
    public InputField PasswordInput;
    public GameObject principalPanel;
    public InputField tokenInput;
    public Slider speedSlider;
    public Text speedText;
    // Use this for initialization
    void Start () {
        confirmarButton.onClick.AddListener(confirmarButtonFunc);
        voltarButton.onClick.AddListener(voltarButtonFunc);
        tokenInput.text = AquariumProperties.configs.token;
        SSIDInput.text = AquariumProperties.configs.ssid;
        PasswordInput.text = AquariumProperties.configs.password;
        speedSlider.value = AquariumProperties.configs.timeSpeed;
    }
	
	// Update is called once per frame
	void Update () {
        int value = (int) speedSlider.value;
        switch (value)
        {
            case 0:
                speedText.text = "Lento";
                break;
            case 1:
                speedText.text = "Normal";
                break;
            case 2:
                speedText.text = "Rápido";
                break;
            case 3:
                speedText.text = "Tempo Real";
                break;
        }
	}

    IEnumerator sendMessage(string route, string content) 
    {
        UnityWebRequest www = UnityWebRequest.Post(route, content);
        www.SetRequestHeader("Content-Type", "text/plain");        
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            www = UnityWebRequest.Post("http://192.168.4.1/wifi_config", content);
            www.SetRequestHeader("Content-Type", "text/plain");
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
        }
    }

    void confirmarButtonFunc()
    {
        ConfigProperties configs = new ConfigProperties();
        configs.token = tokenInput.text;
        configs.timeSpeed = (int) speedSlider.value;
        configs.ssid = SSIDInput.text;
        configs.password = PasswordInput.text;
        configs.moduleIP = IUTModuleProperties.ModuleAddress != null && !"".Equals(IUTModuleProperties.ModuleAddress) ? IUTModuleProperties.ModuleAddress : IUTModuleProperties.DEFAULT_MODULE_ADDRESS;        
        string route = "http://" + configs.moduleIP + "/wifi_config";        
        string message = "ssid=" + configs.ssid + "&password=" + configs.password;
        Debug.Log(route);
        StartCoroutine(sendMessage(route, message));        
        configs.saveConfig();
        AquariumProperties.configs = configs;
        principalPanel.SetActive(true);
        GameObject.Find("Configuracao_Panel").SetActive(false);        
    }

    void voltarButtonFunc()
    {
        principalPanel.SetActive(true);
        GameObject.Find("Configuracao_Panel").SetActive(false);
    }
}
