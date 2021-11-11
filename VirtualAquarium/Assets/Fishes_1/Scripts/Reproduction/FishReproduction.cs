using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    random,
    male,
    female
}
public abstract class FishReproduction
{
    private Fish _fish;

    public Fish fish { get => _fish; set => _fish = value; }

    public abstract bool Reproduce();

    public FishReproduction(Fish fish)
    {
        this.fish = fish;
    }
}