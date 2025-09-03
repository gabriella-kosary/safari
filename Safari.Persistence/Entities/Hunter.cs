using Safari.Persistence.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.Entities
{
    public class Hunter : Entity
    {
        #region Properties
        public Entity? entityToKill { get; set; }
        #endregion

        #region PublicFunctions
        public void Attack()
        {
            if (entityToKill == null) throw new Exception("EntityToKill is null, yet attack was invoked");
            if (entityToKill is Animal animalToKill)
            {
                animalToKill.isDead = true;
                entityToKill = null;
            }
            else if (this is Ranger && entityToKill is Poacher poacher)
            {
                int ranger_number;
                int poacher_number;
                do
                {
                    ranger_number = Roll();
                    poacher_number = poacher.Roll();
                    if (ranger_number > poacher_number)
                    {
                        poacher.OnEntityDied();
                        entityToKill = null;
                    }
                    else if (poacher_number > ranger_number) OnEntityDied();
                } while (ranger_number == poacher_number);
            }
            else if (this is Poacher && entityToKill is Ranger ranger)
            {
                int ranger_number;
                int poacher_number;
                do
                {
                    ranger_number = ranger.Roll();
                    poacher_number = Roll();
                    if (ranger_number > poacher_number) OnEntityDied();
                    else if (poacher_number > ranger_number)
                    {
                        ranger.OnEntityDied();
                        entityToKill = null;
                    }
                } while (ranger_number == poacher_number);
            }
        }
        public int Roll()
        {
            if (rnd == null) throw new NullReferenceException("random was null");

            int number;
            if (this is Ranger)
            {
                number = rnd.Next(1, 8);
            }
            else
            {
                number = rnd.Next(1, 6);
            }
            return number;
        }
        #endregion

        #region ProtectedFunctions
        protected override void NewDestination(int canvasWidth, int canvasHeight, int tileWidth, int tileHeight)
        {
            if (rnd == null) throw new NullReferenceException("random was null");
            if (entityToKill != null)
            {
                destination = entityToKill.position;
            }
            else
            {
                if (this is Ranger ranger) ranger.isUserDestination = false;
                destination = (rnd.Next((tileWidth / 2) + 5, canvasWidth - (tileWidth / 2) - 5), rnd.Next((tileHeight / 2) + 5, canvasHeight - (tileHeight / 2) - 5));
            }
        }
        #endregion
    }
}