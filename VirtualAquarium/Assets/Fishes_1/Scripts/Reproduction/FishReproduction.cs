using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VirtualAquarium
{
    public enum Gender
    {
        random,
        male,
        female
    }
    public abstract class FishReproduction
    {
        private Fish _fish;
        protected float timeSinceReproduction = 0;

        public Fish fish { get => _fish; set => _fish = value; }

        public abstract bool Reproduce();

        public FishReproduction(Fish fish)
        {
            this.fish = fish;
        }

        public virtual string getLetterIndentify()
        {
            return "";
        }

        public virtual Color getLetterIndentifyColor()
        {
            return Color.white;
        }

        public virtual void Reset()
        {
            timeSinceReproduction = 0;
        }
    }
}