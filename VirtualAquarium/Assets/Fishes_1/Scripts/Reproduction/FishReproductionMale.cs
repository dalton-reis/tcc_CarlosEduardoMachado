using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualAquarium
{
    public class FishReproductionMale : FishReproduction
    {
        public override bool Reproduce()
        {
            if (timeSinceReproduction == 0 || timeSinceReproduction + AquariumProperties.timeSpeedMultiplier * 2 <= Time.fixedTime)
            {
                timeSinceReproduction = Time.fixedTime;
                foreach (EggFish egg in fish.fishArea.GetComponentsInChildren<EggFish>())
                {
                    if (Vector3.Distance(fish.transform.position, egg.transform.position) < 3 && egg.fertilize(fish))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public FishReproductionMale(Fish fish) : base(fish)
        {
        }

        public override string getLetterIndentify()
        {
            return "M";
        }

        public override Color getLetterIndentifyColor()
        {
            return Color.cyan;
        }
    }
}