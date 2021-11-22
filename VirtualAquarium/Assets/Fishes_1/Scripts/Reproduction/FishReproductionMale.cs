using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishReproductionMale : FishReproduction
{
    public override bool Reproduce()
    {
        foreach (EggFish egg in fish.fishArea.GetComponentsInChildren<EggFish>())
        {
            if (Vector3.Distance(fish.transform.position, egg.transform.position) < 4)
            {
                return egg.fertilize(fish);
            }
        }
        return false;
    }
    public FishReproductionMale(Fish fish): base(fish)
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
