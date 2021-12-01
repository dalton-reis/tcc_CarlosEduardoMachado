using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualAquarium
{
    public class FishInformation : MonoBehaviour
    {
        // Start is called before the first frame update
        public Text TextFishName;
        public Text TextFishGender;
        public Slider healthSlider;
        private Fish fish;
        private Button button;
        private FishArea fishArea;
        private FishesInformation fishesInformation;

        public Fish Fish
        {
            get => fish; set
            {
                fish = value;
                if (Fish != null)
                {
                    TextFishGender.text = Fish.fishReproduction?.getLetterIndentify();
                    if (Fish.fishReproduction != null)
                        TextFishGender.color = Fish.fishReproduction.getLetterIndentifyColor();
                    TextFishName.text = Fish.specie.ToString();
                    fish.fishInformation = this;
                }
                else
                {
                    TextFishGender.text = "";
                    TextFishName.text = "Todos os peixes";
                }
            }
        }

        void Start()
        {
            fishArea = GameObject.FindObjectOfType<FishArea>();
            fishesInformation = GameObject.FindObjectOfType<FishesInformation>(); ;
            button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(buttonClick);
        }

        private void buttonClick()
        {
            fishArea.SelectedFish = Fish;
            fishesInformation.fishesInformation.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (fishArea.SelectedFish == Fish)
            {
                TextFishName.color = Color.green;
            }
            else
            {
                if (Fish != null && AquariumProperties.aquariumTemperature > Fish.MaxTemperatureSupported)
                {
                    TextFishName.color = Color.red;
                } else if (Fish != null && AquariumProperties.aquariumTemperature < Fish.MinTemperatureSupported)
                {
                    TextFishName.color = Color.cyan;
                } else
                {
                    TextFishName.color = Color.white;
                }
            }

            if (Fish != null)
                healthSlider.value = Fish.life * 0.01f;
            else
                healthSlider.value = AquariumProperties.aquariumHealth * 0.01f;
        }
    }
}