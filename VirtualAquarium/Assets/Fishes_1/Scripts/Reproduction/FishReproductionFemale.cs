using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishReproductionFemale : FishReproduction
{
    float timeSinceReproduction = 0;
    public override bool Reproduce()
    {
        if (fish.transform.localPosition.y <= -2 && (timeSinceReproduction == 0 || timeSinceReproduction + AquariumProperties.timeSpeedMultiplier * 24 <= Time.fixedTime))
        {
            timeSinceReproduction = Time.fixedTime;
            return EggFish.layEgg(fish);
        }
        return false;
    }
    public FishReproductionFemale(Fish fish) : base(fish)
    {
    }
}
