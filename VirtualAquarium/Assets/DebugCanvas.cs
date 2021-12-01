using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace VirtualAquarium
{
    public class DebugCanvas : MonoBehaviour
    {
        public Text textEpisodeCount;
        public Text textEat;
        public Text textReproduce;
        public Text textReward;
        public int eat = 0, reproduce = 0, episode = 0;
        FishArea fishArea;
        // Start is called before the first frame update
        void Start()
        {
            fishArea = GameObject.FindObjectOfType<FishArea>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void reloadReward()
        {
            episode++;
            int count = 0;
            textEpisodeCount.text = "Episode count: " + (episode / fishArea.fishes.Count).ToString();
            float reward = 0;
            foreach (Fish fish in fishArea.fishes)
            {
                if (fish.StepCount == 0)
                {
                    reward += fish.totalReward;
                    count++;
                }

            }
            if (count > 0)
                textReward.text = "Reward Last Episode: " + (reward / count).ToString();
        }

        public void AddEat()
        {
            eat++;
            textEat.text = "Eat: " + eat.ToString();
        }

        public void AddReproduce()
        {
            reproduce++;
            textReproduce.text = "Reproduce: " + reproduce.ToString();
        }
    }
}