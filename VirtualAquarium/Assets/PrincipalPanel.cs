using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Mirror;

public class PrincipalPanel : MonoBehaviour {

    public Button jogarButton;
    public Button jogarRVButton;

    public InputField Token, Wifi, SenhaWifi, ServidorMult;

    public Toggle IOT, Multiplayer, CameraDesenvolvimento;
    public Toggle InterfaceInterativa;
    public Button configurarButton;
    GameController gameController;
    public Button sairButton;
    public GameObject configuracaoPanel;
    public Text connectingText;
    private bool startSimulator;
    private bool RV;
    [SerializeField] private bool vrModeEnabled;

    // Use this for initialization

    private void Start () {
        if(!gameController){
            gameController = GameObject.FindObjectOfType<GameController>();
        }
        XRSettings.enabled = vrModeEnabled;
        AquariumProperties.configs = ConfigProperties.loadConfig ();
        jogarButton.onClick.AddListener (jogarButtonFunc);
        jogarRVButton.onClick.AddListener (jogarRVButtonFunc);
        configurarButton.onClick.AddListener (configurarButtonFunc);
        sairButton.onClick.AddListener (sairButtonFunc);
    }

    // Update is called once per frame    
    private void Update () { }

    private void OnApplicationQuit () {
        if (IOT.isOn) {
            AquariumProperties.conn.stop ();
        }
    }

    private void initializeController()
    {
        Debug.Log("Ta la"+ InterfaceInterativa.isOn);
        gameController.iot = IOT.isOn;
        gameController.multi = Multiplayer.isOn;
        gameController.CameraDesenvolvimento = CameraDesenvolvimento.isOn;
        gameController.server = ServidorMult.text;
        gameController.interativa = InterfaceInterativa.isOn;
    }

    public void StartSceneAquarium () {
        if (Multiplayer.isOn) {
            if (ServidorMult.text != "") {
                if (IOT.isOn) {
                    if ((Token.text != "") && (Wifi.text != "") && (SenhaWifi.text != "")) {
                        connectingText.gameObject.SetActive (false);
                        initializeController();
                        SceneManager.LoadScene ("AquariumSceneClient", LoadSceneMode.Single);
                    } else {
                        connectingText.text = "Com o IOT Ativo, necessário configurar o WIFI";
                        connectingText.gameObject.SetActive (true);
                    }
                } else {
                    connectingText.gameObject.SetActive (false);
                    initializeController();
                    SceneManager.LoadScene ("AquariumSceneClient", LoadSceneMode.Single);
                }
            } else {
                connectingText.text = "Com o Multi Ativo, necessário configurar o Servidor Multi";
                connectingText.gameObject.SetActive (true);
            }

        } else {
            if (IOT.isOn) {
                if ((Token.text != "") && (Wifi.text != "") && (SenhaWifi.text != "")) {
                    connectingText.gameObject.SetActive (false);
                    initializeController();
                    SceneManager.LoadScene ("AquariumSceneClient", LoadSceneMode.Single);
                } else {
                    connectingText.text = "Com o IOT Ativo, necessário configurar o WIFI";
                    connectingText.gameObject.SetActive (true);
                }
            } else {
                initializeController();
                SceneManager.LoadScene ("AquariumSceneClient");
            }

        }

    }

    public void StartSceneVR () {
        gameController.iot = IOT.isOn;
        gameController.multi = Multiplayer.isOn;
        gameController.CameraDesenvolvimento = CameraDesenvolvimento.isOn;
        gameController.server = ServidorMult.text;
        SceneManager.LoadScene ("AquariumSceneVR");
    }

    void jogarButtonFunc () {
        if (IOT.isOn) {
            if ((Token.text != "") && (Wifi.text != "") && (SenhaWifi.text != "")) {
                AquariumProperties.conn = new IUTConnect ();
                AquariumProperties.conn.OnConnect += connectCallback;
                AquariumProperties.conn.start (AquariumProperties.configs.token);
                startSimulator = true;
                connectingText.text = "Conectando...";
                connectingText.gameObject.SetActive (true);
            } else {
                connectingText.text = "Com o IOT Ativo, necessário configurar o WIFI";
                connectingText.gameObject.SetActive (true);
            }

        } else {
            startSimulator = true;
        }

    }

    void jogarRVButtonFunc () {
        startSimulator = true;
        RV = true;
    }

    IEnumerator CloseVR () {
        UnityEngine.XR.XRSettings.LoadDeviceByName ("None");
        yield return null;
        UnityEngine.XR.XRSettings.enabled = false;
    }

    void configurarButtonFunc () {
        configuracaoPanel.SetActive (true);
        GameObject.Find ("Principal_Panel").SetActive (false);
    }

    void connectCallback (bool connected) {
        startSimulator = true;
    }

    private void sairButtonFunc () {
        Application.Quit ();
    }
}