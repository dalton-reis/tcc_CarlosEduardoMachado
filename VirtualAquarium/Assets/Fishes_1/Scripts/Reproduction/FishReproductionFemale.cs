using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualAquarium
{
    public class FishReproductionFemale : FishReproduction
    {
        public override bool Reproduce()
        {
            if (fish.transform.localPosition.y <= -2 && (timeSinceReproduction == 0 || timeSinceReproduction + AquariumProperties.timeSpeedMultiplier * 24 * 7 <= Time.fixedTime))
            {
                timeSinceReproduction = Time.fixedTime;
                return EggFish.layEgg(fish);
            }
            return false;
        }
        public FishReproductionFemale(Fish fish) : base(fish)
        {
        }

        public override string getLetterIndentify()
        {
            return "F";
        }
        public override Color getLetterIndentifyColor()
        {
            return Color.magenta;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}