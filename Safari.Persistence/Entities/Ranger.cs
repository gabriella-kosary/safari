using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Entities
{
    public class Ranger : Hunter
    {
        public bool isUserDestination;
        public Ranger((int,int) position, int fov)
        {
            canSee = true;
            isUserDestination = false;
            this.position = position;
            this.fov = fov;
            entityToKill = null;
            rnd = new Random();
            destination = position;
        }
        public void Leave()
        {
            OnEntityDied();
        }
    }
}
