using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
public class AquariumUpdate : NetworkBehaviour {

    public FishArea fishArea;
    public Slider healthSlider;
    public Slider aquariumTemperatureSlider;
    public Slider lightingSlider;
    public Text foodCountText;
    public Text hourText;
    public Text externalTemperatureText;
    public Text heaterTemperatureText;
    public Text periodText;
    public RawImage wheaterImage;
    public Light directionalLight;
    private float accumulatedTime;
    private float second;
    private DateTime lastFoodHour;
    private float timeChangeWheater;

    NetworkManager manager;
    private float timeChangeExternalTemperature;
    [SyncVar]
    public float life;
    private bool night = false;
    private bool isNight {
        get {
            return night;
        }
        set {
            if (night != value) {
                night = value;
                AquariumProperties.currentWheater = (AquariumProperties.Wheater) changeWheater ();
            }

        }
    }
    public Texture2D sunImage;
    public Texture2D sunAndCloudImage;
    public Texture2D snowImage;
    GameController gameController;
    public Texture2D rainImage;
    public Text IP;
    public Texture2D moonImage;
    public ParticleSystem particleFood;
    public Button sairButton;
    private bool dropFood;
    private const string DEFAULT_HOUR_MASK = "HH:mm";
    private DateTime initialNight = DateTime.ParseExact ("19:00", DEFAULT_HOUR_MASK, CultureInfo.InvariantCulture);
    private DateTime finalNight = DateTime.ParseExact ("05:00", DEFAULT_HOUR_MASK, CultureInfo.InvariantCulture);
    [SerializeField] private bool vrModeEnabled;
    private void OnApplicationQuit () {
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
        if (gameController.iot) {
            AquariumProperties.conn.stop ();
        }
    }

    // Use this for initialization
    void Start () {
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
        if (!gameController.multi) {
            this.ativaPeixesSemMulti ();
        }
        /*if(!manager){
            manager = GameObject.FindObjectOfType<NetworkManager>();
        }*/
        IP.text = this.GetIP();
        if (!fishArea) {
            fishArea = GameObject.FindObjectOfType<FishArea> ();
        }
        XRSettings.enabled = vrModeEnabled;
        sairButton.onClick.AddListener (sairButtonFunc);
        dropFood = false;

        if (gameController.iot) {
            AquariumProperties.conn.OnReceive += socketCallback;
        }

        if (AquariumProperties.configs != null) {
            AquariumProperties.currentTimeSpeed = (AquariumProperties.TimeSpeed) AquariumProperties.configs.timeSpeed;
        } else {
            AquariumProperties.currentTimeSpeed = AquariumProperties.TimeSpeed.Normal;
        }
        AquariumProperties.aquariumTemperature = 25.0f;
        AquariumProperties.externalTemperature = 25.0f;
        AquariumProperties.heaterTemperature = 19;
        AquariumProperties.externalLightIntensity = UnityEngine.Random.Range (0.5f, 1.0f);
        AquariumProperties.foodAvailable = 10;
        AquariumProperties.currentWheater = AquariumProperties.Wheater.Sun;
        AquariumProperties.aquariumHour = DateTime.ParseExact ("08:00", DEFAULT_HOUR_MASK, CultureInfo.InvariantCulture);
        lastFoodHour = DateTime.ParseExact ("08:00", DEFAULT_HOUR_MASK, CultureInfo.InvariantCulture);
        switch (AquariumProperties.currentTimeSpeed) {
            case AquariumProperties.TimeSpeed.Fast:
                AquariumProperties.timeSpeedMultiplier = 30;
                break;
            case AquariumProperties.TimeSpeed.Normal:
                AquariumProperties.timeSpeedMultiplier = 60;
                break;
            case AquariumProperties.TimeSpeed.Slow:
                AquariumProperties.timeSpeedMultiplier = 120;
                break;
            case AquariumProperties.TimeSpeed.RealTime:
                AquariumProperties.timeSpeedMultiplier = 3600;
                break;
        }
    }

    public string GetIP () {
        string strHostName = "";
        strHostName = System.Net.Dns.GetHostName ();
        IPHostEntry ipEntry = System.Net.Dns.GetHostEntry (strHostName);
        IPAddress[] addr = ipEntry.AddressList;
        return addr[addr.Length - 1].ToString ();
    }

    IEnumerator CloseVR () {
        UnityEngine.XR.XRSettings.LoadDeviceByName ("None");
        yield return null;
        UnityEngine.XR.XRSettings.enabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (dropFood) {
            particleFood.Play ();
            dropFood = false;
        }
        updateTime ();
        heaterTemperatureText.text = AquariumProperties.heaterTemperature + "ºC";
    }

    void updateTime () {
        accumulatedTime += Time.deltaTime;
        if (AquariumProperties.currentTimeSpeed != AquariumProperties.TimeSpeed.RealTime) {
            if (accumulatedTime >= AquariumProperties.timeSpeedMultiplier) {
                AquariumProperties.aquariumHour = AquariumProperties.aquariumHour.AddHours (1);
                accumulatedTime = 0;
            }
        } else {
            AquariumProperties.aquariumHour = AquariumProperties.aquariumHour.AddSeconds (Time.deltaTime);
        }
        hourText.text = AquariumProperties.aquariumHour.ToString (DEFAULT_HOUR_MASK);
        second += Time.deltaTime;
        if (second >= 1) {
            updateHealth ();
            updateFood ();
            updateWheater ();
            updateTemperature ();
            updateLightItensity ();
            updateHealthCoefficient ();
            second = 0;
        }
    }

    [ServerCallback]
    void updateHealth () {
        float totalLife = 0;
        int countFishes = 0;
        for (int i = 0; i < fishArea.fishes.Count; i++) {
            if (fishArea.fishes[i].gameObject.activeSelf) {
                totalLife += fishArea.fishes[i].life;
                ++countFishes;
            }

        }
        if (countFishes > 0) {
            AquariumProperties.aquariumHealth = totalLife / countFishes;
            if (gameController.multi) {
                life = AquariumProperties.aquariumHealth * 0.01f;
                healthSlider.value = life;
            } else {
                healthSlider.value = AquariumProperties.aquariumHealth * 0.01f;
            }

        } else {
            AquariumProperties.aquariumHealth = 0;
            healthSlider.value = 0;
        }
    }

    void updateFood () {
        if ((AquariumProperties.aquariumHour.Subtract (lastFoodHour).Hours >= 2) && AquariumProperties.foodAvailable < 10) {
            AquariumProperties.foodAvailable++;
            lastFoodHour = AquariumProperties.aquariumHour;
        }
        foodCountText.text = AquariumProperties.foodAvailable.ToString ();
    }

    void updateWheater () {
        isNight = AquariumProperties.aquariumHour.Hour >= initialNight.Hour || AquariumProperties.aquariumHour.Hour <= finalNight.Hour;
        periodText.text = isNight ? "Noite" : "Dia";
        timeChangeWheater++;
        if (timeChangeWheater >= AquariumProperties.timeSpeedMultiplier * 4) {
            int wheater = changeWheater ();
            AquariumProperties.currentWheater = (AquariumProperties.Wheater) wheater;
            timeChangeWheater = 0;
        }
        timeChangeExternalTemperature++;
        if (timeChangeExternalTemperature >= AquariumProperties.timeSpeedMultiplier) {
            switch (AquariumProperties.currentWheater) {
                case AquariumProperties.Wheater.Sun:
                    wheaterImage.texture = sunImage;
                    AquariumProperties.externalTemperature = UnityEngine.Random.Range (25, 43);
                    AquariumProperties.externalLightIntensity = UnityEngine.Random.Range (0.5f, 1.0f);
                    break;
                case AquariumProperties.Wheater.SunAndCloud:
                    wheaterImage.texture = sunAndCloudImage;
                    AquariumProperties.externalTemperature = UnityEngine.Random.Range (18, 33);
                    AquariumProperties.externalLightIntensity = UnityEngine.Random.Range (0.3f, 0.6f);
                    break;
                case AquariumProperties.Wheater.Snow:
                    wheaterImage.texture = snowImage;
                    AquariumProperties.externalTemperature = UnityEngine.Random.Range (-2, 5);
                    AquariumProperties.externalLightIntensity = isNight ? UnityEngine.Random.Range (0.0f, 0.3f) : UnityEngine.Random.Range (0.2f, 0.5f);
                    break;
                case AquariumProperties.Wheater.Rain:
                    wheaterImage.texture = rainImage;
                    AquariumProperties.externalTemperature = UnityEngine.Random.Range (10, 24);
                    AquariumProperties.externalLightIntensity = isNight ? UnityEngine.Random.Range (0.0f, 0.3f) : UnityEngine.Random.Range (0.2f, 0.5f);
                    break;
                case AquariumProperties.Wheater.Moon:
                    wheaterImage.texture = moonImage;
                    AquariumProperties.externalTemperature = UnityEngine.Random.Range (8, 21);
                    AquariumProperties.externalLightIntensity = 0.0f;
                    break;
            }
            externalTemperatureText.text = AquariumProperties.externalTemperature + "ºC";
            timeChangeExternalTemperature = 0;
        }
    }

    int changeWheater () {
        bool acceptable = false;
        int wheater;
        do {
            wheater = UnityEngine.Random.Range (0, 4);
            if (isNight && (wheater == 0 || wheater == 1)) {
                acceptable = false;
            } else if (!isNight && wheater == 4) {
                acceptable = false;
            } else {
                acceptable = true;
            }
        } while (!acceptable);
        return wheater;
    }

    void ativaPeixesSemMulti () {
        if (!fishArea) {
            fishArea = GameObject.FindObjectOfType<FishArea> ();
        }
        Fish[] peixes = fishArea.GetComponentsInChildren<Fish> (true);
        foreach (Fish f in peixes) {
            f.gameObject.SetActive (true);
        }
    }

    void updateTemperature () {
        float heaterAquariumDiff = AquariumProperties.heaterTemperature - AquariumProperties.aquariumTemperature;
        float externalAquariumDiff = AquariumProperties.externalTemperature - AquariumProperties.aquariumTemperature;
        float timeCoefficient = 0;
        switch (AquariumProperties.currentTimeSpeed) {
            case AquariumProperties.TimeSpeed.Fast:
                timeCoefficient = 150;
                break;
            case AquariumProperties.TimeSpeed.Normal:
                timeCoefficient = 180;
                break;
            case AquariumProperties.TimeSpeed.RealTime:
                timeCoefficient = AquariumProperties.timeSpeedMultiplier * 3;
                break;
            case AquariumProperties.TimeSpeed.Slow:
                timeCoefficient = 240;
                break;
        }
        AquariumProperties.temperatureCoefficient = externalAquariumDiff / timeCoefficient + heaterAquariumDiff / ((AquariumProperties.timeSpeedMultiplier * 2.5f));
        AquariumProperties.aquariumTemperature += AquariumProperties.temperatureCoefficient;
        aquariumTemperatureSlider.value = AquariumProperties.aquariumTemperature;
    }

    void updateLightItensity () {
        AquariumProperties.lightIntensity = AquariumProperties.externalLightIntensity + AquariumProperties.sensorLightIntensity;
        directionalLight.intensity = AquariumProperties.lightIntensity;
        lightingSlider.value = AquariumProperties.lightIntensity;
    }

    void updateHealthCoefficient () {
        float temperatureCoefficient = 0;
        float lightCoefficient = 0;
        if (AquariumProperties.aquariumTemperature >= AquariumProperties.MIN_TEMPERATURE_SUPPORTED && AquariumProperties.aquariumTemperature <= AquariumProperties.MAX_TEMPERATURE_SUPPORTED) {
            temperatureCoefficient = 0;
        } else if (AquariumProperties.aquariumTemperature > AquariumProperties.MAX_TEMPERATURE_SUPPORTED) {
            temperatureCoefficient = (AquariumProperties.aquariumTemperature - AquariumProperties.MAX_TEMPERATURE_SUPPORTED) * 0.03f;
        } else if (AquariumProperties.aquariumTemperature < AquariumProperties.MIN_TEMPERATURE_SUPPORTED) {
            temperatureCoefficient = (AquariumProperties.MIN_TEMPERATURE_SUPPORTED - AquariumProperties.aquariumTemperature) * 0.03f;
        }
        float maxLight = isNight ? AquariumProperties.MAX_LIGHT_SUPPORTED_NIGHT : AquariumProperties.MAX_LIGHT_SUPPORTED;
        float minLight = isNight ? AquariumProperties.MIN_LIGHT_SUPPORTED_NIGHT : AquariumProperties.MIN_LIGHT_SUPPORTED;
        if (AquariumProperties.lightIntensity >= minLight && AquariumProperties.lightIntensity <= maxLight) {
            lightCoefficient = 0;
        } else if (AquariumProperties.lightIntensity > maxLight) {
            lightCoefficient = (AquariumProperties.lightIntensity - maxLight) * 0.02f;
        } else if (AquariumProperties.lightIntensity < minLight) {
            lightCoefficient = (minLight - AquariumProperties.lightIntensity) * 0.02f;
        }
        AquariumProperties.lossLifeCoefficient = lightCoefficient + temperatureCoefficient;
    }

    public void socketCallback (string message) {
        string[] items = message.Split ('|');
        string tag = items[0];
        string value = items[1];
        if (tag.Equals ("TEMP")) {
            AquariumProperties.heaterTemperature = float.Parse (value);

        } else if (tag.Equals ("LIGHT")) {
            AquariumProperties.sensorLightIntensity = (100 - float.Parse (value)) * 0.01f;
        } else if (tag.Equals ("FOOD") && AquariumProperties.foodAvailable > 0) {
            AquariumProperties.foodAvailable--;
            dropFood = true;
        }
    }

    void sairButtonFunc () {
        if (gameController.iot) {
            AquariumProperties.conn.stop ();
            StartCoroutine (CloseVR ());
        }
        SceneManager.LoadScene (0, LoadSceneMode.Single);
    }
}