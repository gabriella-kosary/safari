using Safari.Persistence.DataAccess;
using Safari.Persistence.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Safari.Persistence.Entities
{
    [JsonConverter(typeof(AnimalConverter))]
    public abstract class Animal : Entity
    {
        [JsonIgnore]
        public virtual string Type => GetType().Name;

        #region Properties
        public bool isDead { get; set; }
        public bool isCaught { get; set; }
        public int birthDay {  get; set; }
        public int age { get; set; }
        public int group { get; set; }
        //Ezt nem menthetjük el
        [JsonIgnore]
        public Animal? theOldest { get; set; }
        public (int, int) transitionVector { get; set; }
        public bool chipped { get; set; }
        public double hunger { get; set; }
        public double thirst { get; set; }
        // -----------------------------
        public int someoneNeedsSomething { get; set; }
        public bool isThirsty { get; set; }
        public bool isHungry { get; set; }
        // -----------------------------
        public bool isEating { get; set; }
        public bool isDrinking { get; set; }
        public bool isResting { get; set; }
        public int restingDay { get; set; }
        public List<(int, int)>? foodSource { get; set; }
        public List<(int, int)>? waterSource { get; set; }
        public List<(int, int)>? obstacles { get; set; }
        #endregion

        #region PublicFunctions
        public void GetOlder(int days)
        {
            int old = (days - birthDay) / 365;
            if (old > age) age++;
        }
        public void GenerateHungerAndThirst()
        {
            hunger -= 3 + age;
            thirst -= 5;
        }
        public void Eat(int day)
        {
            if (hunger < 9950)
            {
                OnAnimalStateChanged(Type, "Eating", true);
                hunger += 50;
            }
            else
            {
                hunger = 10000;
                isEating = false;
                isHungry = false;
                isResting = true;
                OnAnimalStateChanged(Type, "Resting", true);
                restingDay = day;
                if(this is Carnivore) {
                    OnEntityMurdered();
                }
            }
        }

        public void Drink()
        {
            if (thirst < 9900) 
            {
                OnAnimalStateChanged(Type, "Eating", true);
                thirst += 100;
            }
            else
            {
                thirst = 10000;
                isDrinking = false;
                OnAnimalStateChanged(Type, "Base", true);
            }
        }
        public void Rest(int day)
        {
            if (theOldest?.someoneNeedsSomething > 0 || day > restingDay + 1)
            {
                isResting = false;
                OnAnimalStateChanged(Type, "Base", true);
            }
            else
            {
                isResting = true;
                OnAnimalStateChanged(Type, "Resting", true);
            }
        }
        public void CheckDead()
        {
            if (isDead) { 
                OnEntityDied();
                return;
            }
            if (age > 5 && !isCaught) isDead = true;
            else if (thirst < 0 && !isCaught) isDead = true;
            else if (hunger < 0 && !isCaught) isDead = true;
            if (isDead && !isCaught) isDead = true;

            if (isDead) OnEntityDied();
        }
        public void NeedsSomething()
        {
            if (thirst < 3000)
            {
                //someoneNeedsSomething = 1;
                isThirsty = true;
            }
            else
            {
                //someoneNeedsSomething = 0;
                isThirsty = false;
            }
            if (hunger < 3500)
            {
                //someoneNeedsSomething = 2;
                isHungry = true;
            }
            else 
            {
                //someoneNeedsSomething = 0; 
                isHungry = false;
            }
        }

        public void GetCaught()
        {
            if (isDead) return;
            destination = position;
            isCaught = true;
            OnAnimalStateChanged(Type, "Base", true);
        }
        #endregion

        #region ProtectedFunctions
        protected override void NewDestination(int canvasWidth, int canvasHeight, int tileWidth, int tileHeight)
        {
            if(isCaught) { return; }
            if (waterSource == null) throw new Exception("WaterSource was null");
            if (foodSource == null) throw new Exception("FoodSource was null");
            if (theOldest == null) throw new Exception("The oldest was null");
            if (rnd == null) throw new Exception("rnd was null");
            if (theOldest == this)
            {
                if (someoneNeedsSomething == 1 && waterSource.Count != 0)
                {
                    if (!waterSource.Contains(destination) || destination == position) {
                        destination = NearestWaterSource(tileWidth, tileHeight);
                    }
                }
                else if (someoneNeedsSomething == 2 && foodSource.Count != 0)
                {
                    if(!foodSource.Contains(destination) || destination == position) {
                        destination = NearestFoodSource(tileWidth, tileHeight);
                    }
                }
                else
                {
                    destination = (rnd.Next((tileWidth / 2) + 5, canvasWidth - (tileWidth / 2) - 5), rnd.Next((tileHeight / 2) + 5, canvasHeight - (tileHeight / 2) - 5));
                }
            }
            else
            {
                if (theOldest?.someoneNeedsSomething == 1 || theOldest?.someoneNeedsSomething == 2)
                {
                    destination = theOldest.destination;
                }
                else 
                {
                    if (theOldest == null) throw new Exception("theoldest was null");
                    int x = theOldest.destination.Item1 + transitionVector.Item1;
                    int y = theOldest.destination.Item2 + transitionVector.Item2;
                    if (x < tileWidth + 5) x = tileWidth + 5;
                    if (x > canvasWidth - tileWidth - 5) x = canvasWidth - tileWidth - 5;
                    if (y < tileHeight) y = tileHeight;
                    if (y > canvasHeight - tileHeight) y = canvasHeight - tileHeight;
                    destination = (x, y);
                }
            }
        }
        #endregion

        #region PrivateFunctions
        private (int, int) NearestFoodSource(int tileWidth, int tileHeight)
        {
            if (foodSource == null) throw new NullReferenceException("foodSource was null");
            (int, int) nearest = foodSource[0];
            double nearestDistance = DistanceWithObstacles(position, nearest, tileWidth, tileHeight);
            double distance;
            foreach ((int, int) food in foodSource)
            {
                distance = DistanceWithObstacles(position, food, tileWidth, tileHeight);
                if (distance < nearestDistance)
                {
                    nearest = food;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }
        private (int, int) NearestWaterSource(int tileWidth, int tileHeight)
        {
            if (waterSource == null) throw new NullReferenceException("waterSource was null");
            (int, int) nearest = waterSource[0];
            double nearestDistance = DistanceWithObstacles(position, nearest, tileWidth, tileHeight);
            double distance;
            foreach ((int, int) water in waterSource)
            {
                distance = DistanceWithObstacles(position, water, tileWidth, tileHeight);
                if (distance < nearestDistance)
                {
                    nearest = water;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }
        private double DistanceWithObstacles((int, int) animalPosition, (int, int) otherPosition, int tileWidth, int tileHeight)
        {
            if (obstacles == null) throw new NullReferenceException("obstacles was null");
            int obstacleCount = 0;
            (int, int) v = (otherPosition.Item1 - animalPosition.Item1, otherPosition.Item2 - animalPosition.Item2);
            if (v.Item1 == 0)
            {
                foreach ((int, int) obstacle in obstacles)
                {
                    if (Math.Abs(obstacle.Item2 - v.Item2) < tileHeight / 2)
                    {
                        obstacleCount++;
                    }
                }
            }
            else if (v.Item2 == 0)
            {
                foreach ((int, int) obstacle in obstacles)
                {
                    if (Math.Abs(obstacle.Item1 - v.Item1) < tileWidth / 2) 
                    {
                        obstacleCount++;
                    }
                }
            }
            else
            {
                // egyenes egyenlete: y = mx + b 
                double m = (double)v.Item2 / v.Item1;
                double b = position.Item2 - m * position.Item1;

                foreach ((int, int) obstacle in obstacles)
                {
                    double x = (obstacle.Item2 - b) / m;
                    double y = m * obstacle.Item1 + b;
                    if (Distance(obstacle, ((int)x, obstacle.Item2)) + 
                        Distance(obstacle, (obstacle.Item1, (int)y)) < tileWidth + tileHeight)
                    {
                        obstacleCount++;
                    }
                }
            }

            double distance = Distance(animalPosition, otherPosition) + obstacleCount * (tileWidth + tileHeight) / 4;
            return distance;
        }
        private double Distance ((int, int) pos1, (int, int) pos2)
        {
            return Math.Sqrt(Math.Pow(pos1.Item1 - pos2.Item1, 2) + Math.Pow(pos1.Item2 - pos2.Item2, 2));
        }
        #endregion

    }
}
