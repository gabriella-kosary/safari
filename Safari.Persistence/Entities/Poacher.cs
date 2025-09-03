using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Entities
{
    public class Poacher : Hunter
    {
        public bool stoleOrKilledToday { get; set; }
        public List<Animal> stolenAnimals { get; set; }
        public Poacher(int fov)
        {
            canSee = false;
            position = (0, 0);
            this.fov = fov;
            entityToKill = null;
            rnd = new Random();
            destination = position;
            stolenAnimals = new List<Animal>();
            stoleOrKilledToday = false;
        }

        public void Steal(Animal animal)
        {
            stolenAnimals.Add(animal);
            animal.OnEntityDied();
        }
    }
}
