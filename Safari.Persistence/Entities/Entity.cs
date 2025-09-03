using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Safari.Persistence.Entities
{
    public abstract class Entity
    {
        protected Random? rnd = null;

        #region Properties
        public bool canSee { get; set; }
        public (int, int) position { get; set; }
        public int fov { get; set; }
        public (int, int) destination { get; set; }
        #endregion

        #region Events
        public event EventHandler<PlacedEventargs>? EntityDied;
        public event EventHandler? EntityMurdered;
        public event EventHandler<EntityMovedEventArgs>? EntityMoved;
        public event EventHandler<AnimalStateEventargs>? AnimalStateChanged;
        #endregion

        [JsonConstructor]
        protected Entity() {
            rnd = new Random();
        }

        #region PublicFunctions
        public void Move(int canvasWidth, int canvasHeight, int tileWidth, int tileHeight, int index)
        {
            int speed = 1;
            if (this is Jeep jeep && !jeep.onThePath) return;
            if (this is Jeep && (Math.Abs(position.Item1 - destination.Item1) > 1 || Math.Abs(position.Item2 - destination.Item2) > 1)) speed = 3; 
            if (this is Ranger ranger && ranger.entityToKill != null && ranger.entityToKill.canSee) NewDestination(canvasWidth, canvasHeight, tileWidth, tileHeight);
            else if (this is Animal animal)
            {
                if(animal.theOldest == null) throw new NullReferenceException("The Oldest was null");
                if (animal.waterSource == null) throw new NullReferenceException("waterSource was null");
                if (animal.foodSource == null) throw new NullReferenceException("foodSource was null");
                if (animal.theOldest.waterSource == null) throw new NullReferenceException("waterSource was null");
                if (animal.theOldest.foodSource == null) throw new NullReferenceException("foodSource was null");
                if (animal.theOldest.someoneNeedsSomething > 0)
                {
                    animal.NewDestination(canvasWidth, canvasHeight, tileWidth, tileHeight);
                    if (animal.theOldest.someoneNeedsSomething == 1 && animal.theOldest.waterSource.Contains(position)) animal.isDrinking = true;
                    if (animal.theOldest.someoneNeedsSomething == 2 && animal.theOldest.foodSource.Contains(position)) animal.isEating = true;
                }
                else if (destination == position) NewDestination(canvasWidth, canvasHeight, tileWidth, tileHeight);
                if (animal.isHungry && (Math.Abs(position.Item1 - destination.Item1) > 1 || Math.Abs(position.Item2 - destination.Item2) > 1)) speed = 2;
            }
            else if (destination == position) NewDestination(canvasWidth, canvasHeight, tileWidth, tileHeight);
            (int, int) nextPosition;

            (int, int) v = (destination.Item1 - position.Item1, destination.Item2 - position.Item2);
            if (v.Item1 == 0)
            {
                nextPosition = (position.Item1, position.Item2 + Math.Sign(v.Item2) * speed);
            }
            else
            {
                double m = v.Item2 / v.Item1 * 1.0;
                double b = position.Item2 - m * position.Item1;
             
                if (Math.Abs(m) <= 1)
                {
                    int x = position.Item1 + Math.Sign(v.Item1) * speed;
                    double y = m * x + b;
                    nextPosition = (x, (int)Math.Round(y));
                }
                else
                {
                    int y = position.Item2 + Math.Sign(v.Item2) * speed;
                    double x = (y - b) / m;
                    nextPosition = ((int)Math.Round(x), y);
                }
            }
            OnEntityMoved(this.GetType().Name, position.Item1, position.Item2, nextPosition.Item1, nextPosition.Item2, destination, index, this is Animal);
            position = nextPosition;
        }
        #endregion

        #region ProtectedFunctions
        protected virtual void NewDestination(int canvasWidth, int canvasHeight, int tileWidth, int tileHeight) { }
        #endregion

        #region EventHandlers
        protected void OnEntityMurdered() {
            EntityMurdered?.Invoke(this, EventArgs.Empty);
        }
        public void OnEntityDied()
        {
            EntityDied?.Invoke(this, new PlacedEventargs
            {
                Type = GetType().Name,
                X = position.Item1,
                Y = position.Item2,
                IsAnimal = this is Animal
            }) ;
        }
        protected void OnAnimalStateChanged(String type, String state, bool isAnimal)
        {
            if (isAnimal)
            {
                AnimalStateChanged?.Invoke(this, new AnimalStateEventargs
                {
                    Type = type,
                    State = state,
                    X = position.Item1,
                    Y = position.Item2,
                    Index = -1
                });
            }
        }
        protected void OnEntityMoved(String type, int oldX, int oldY, int newX, int newY, (int, int) dest, int index, bool isAnimal)
        {
            EntityMoved?.Invoke(this, new EntityMovedEventArgs
            {
                Type = type,
                OldX = oldX,
                OldY = oldY,
                NewX = newX,
                NewY = newY,
                Destination = dest,
                Index = index,
                IsAnimal = isAnimal
            });
        }
        #endregion
    }
}