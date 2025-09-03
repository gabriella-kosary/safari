using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Safari.Persistence.Entities
{
    public class Hyena : Carnivore
    {
        [JsonConstructor]
        public Hyena() : base() { }

        public Hyena(int group, (int, int) position, int fov, int birthDay, Animal? theOldest)
        {
            rnd = new Random();
            canSee = true;
            this.position = position;
            this.fov = fov;
            this.birthDay = birthDay;
            age = 0;
            isDead = false;
            if (theOldest is null)
            {
                this.theOldest = this;
                this.group = group;
                transitionVector = (0, 0);
            }
            else
            {
                this.group = theOldest.group;
                this.theOldest = theOldest;
                transitionVector = (position.Item1 - theOldest.position.Item1, position.Item1 - theOldest.position.Item1);
            }
            chipped = false;
            hunger = 10000;
            thirst = 10000;
            someoneNeedsSomething = 0;
            isEating = false;
            isDrinking = false;
            isResting = false;
            restingDay = 0;
            foodSource = new List<(int, int)>();
            waterSource = new List<(int, int)>();
            obstacles = new List<(int, int)>();
            destination = position;
        }

        [JsonInclude]
        public override string Type => "Hyena";
    }
}
