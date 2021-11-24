namespace VirtualAquarium
{
    public class FishReproductionFactory
    {
        public static FishReproduction createFishReproduction(Fish fish)
        {
            switch (fish.gender)
            {
                case Gender.male:
                    return new FishReproductionMale(fish);
                case Gender.female:
                    return new FishReproductionFemale(fish);
                default:
                    return null;
            }
        }
    }
}