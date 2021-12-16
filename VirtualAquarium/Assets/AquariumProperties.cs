using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AquariumProperties : ScriptableObject {

    public static float aquariumTemperature;
    public static float externalTemperature;
    public static float heaterTemperature;
    public static float aquariumHealth;
    public static float lightIntensity;
    public static float externalLightIntensity;
    public static float sensorLightIntensity;
    public static float foodAvailable;
    public static Wheater currentWheater;
    public static DateTime aquariumHour;
    private static TimeSpeed currentTimeSpeed;
    public static float timeSpeedMultiplier;
    public static float lifeLostPerHour = 20.0f;
    public static float temperatureCoefficient;
    public static float lossLifeCoefficient;
    public static float MAX_TEMPERATURE_SUPPORTED = 25.5f;
    public static float MIN_TEMPERATURE_SUPPORTED = 22.5f;
    public static float MIN_LIGHT_SUPPORTED_NIGHT = 0.0f;
    public static float MAX_LIGHT_SUPPORTED_NIGHT = 1.0f;
    public static float MIN_LIGHT_SUPPORTED = 1.0f;
    public static float MAX_LIGHT_SUPPORTED = 2.0f;
    public static ConfigProperties configs;
    public static IUTConnect conn;

    public static TimeSpeed CurrentTimeSpeed { get => currentTimeSpeed; set {
            currentTimeSpeed = value;
            switch (currentTimeSpeed)
            {
                case TimeSpeed.RealTime:
                    timeSpeedMultiplier = 3600;
                    break;
                case TimeSpeed.Slow:
                    timeSpeedMultiplier = 120;
                    break;
                case TimeSpeed.Normal:
                    timeSpeedMultiplier = 60;
                    break;
                case TimeSpeed.Fast:
                    timeSpeedMultiplier = 30;
                    break;
                case TimeSpeed.SuperFast:
                    timeSpeedMultiplier = 20;
                    break;
            }
        } }

    public enum TimeSpeed
    {
        RealTime = 0, Slow = 1, Normal = 2, Fast = 3, SuperFast = 4
    }
    public enum Wheater
    {
        Sun = 0,  SunAndCloud = 1, Snow = 2, Rain = 3, Moon = 4
    }
}
