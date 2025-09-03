using Safari.Model;
using Safari.Persistence.Entities;
using Safari.Persistence;
using System.Data;
using System.Net.Http.Headers;
using System.Xml;
using Safari.Persistence.Tiles;
namespace Safari.Test;

public class TestEntity : Entity
{
    public bool NewDestinationCalled { get; private set; } = false;

    protected override void NewDestination(int canvasWidth, int canvasHeight, int tileWidth, int tileHeight)
    {
        NewDestinationCalled = true;
        destination = (position.Item1 + 1, position.Item2); // egy egyszerû új cél
    }
}

[TestClass]
public class TestEntities
{
    [TestMethod]
    public void OnEntityDied_ShouldRaiseEntityDiedEventRanger()
    {
        // Arrange
        var entity = new Ranger((5,5), 5);
        bool eventRaised = false;

        entity.EntityDied += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Ranger", args.Type);
            Assert.AreEqual(5, args.X);
            Assert.AreEqual(5, args.Y);
            Assert.IsFalse(args.IsAnimal);
        };

        entity.OnEntityDied();

        // Assert
        Assert.IsTrue(eventRaised);
    }
    [TestMethod]
    public void OnEntityDied_ShouldRaiseEntityDiedEventPoacher()
    {
        // Arrange
        var entity = new Poacher(5); //0,0 spawnol
        bool eventRaised = false;

        entity.EntityDied += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Poacher", args.Type);
            Assert.AreEqual(0, args.X);
            Assert.AreEqual(0, args.Y);
            Assert.IsFalse(args.IsAnimal);
        };
        
        entity.OnEntityDied();

        // Assert
        Assert.IsTrue(eventRaised);
    }
    [TestMethod]
    public void OnEntityDied_ShouldRaiseEntityDiedEventHyena()
    {
        // Arrange
        var entity = new Hyena(1, (5,5), 5, 0, null);
        bool eventRaised = false;

        entity.EntityDied += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Hyena", args.Type);
            Assert.AreEqual(5, args.X);
            Assert.AreEqual(5, args.Y);
            Assert.IsTrue(args.IsAnimal);
        };

        entity.OnEntityDied();

        // Assert
        Assert.IsTrue(eventRaised);
    }
    [TestMethod]
    public void OnEntityDied_ShouldRaiseEntityDiedEventLion()
    {
        // Arrange
        var entity = new Lion(1, (5, 5), 5, 0, null);
        bool eventRaised = false;

        entity.EntityDied += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Lion", args.Type);
            Assert.AreEqual(5, args.X);
            Assert.AreEqual(5, args.Y);
            Assert.IsTrue(args.IsAnimal);
        };

        entity.OnEntityDied();

        // Assert
        Assert.IsTrue(eventRaised);
    }
    [TestMethod]
    public void OnEntityDied_ShouldRaiseEntityDiedEventAntilope()
    {
        // Arrange
        var entity = new Antilope(1, (5, 5), 5, 0, null);
        bool eventRaised = false;

        entity.EntityDied += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Antilope", args.Type);
            Assert.AreEqual(5, args.X);
            Assert.AreEqual(5, args.Y);
            Assert.IsTrue(args.IsAnimal);
        };

        entity.OnEntityDied();

        // Assert
        Assert.IsTrue(eventRaised);
    }
    [TestMethod]
    public void OnEntityDied_ShouldRaiseEntityDiedEventElephant()
    {
        // Arrange
        var entity = new Elephant(1, (5, 5), 5, 0, null);
        bool eventRaised = false;

        entity.EntityDied += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Elephant", args.Type);
            Assert.AreEqual(5, args.X);
            Assert.AreEqual(5, args.Y);
            Assert.IsTrue(args.IsAnimal);
        };

        entity.OnEntityDied();

        // Assert
        Assert.IsTrue(eventRaised);
    }
    [TestMethod]
    public void Move_WhenAtDestination_ShouldCallNewDestination()
    {
        // Arrange
        var entity = new TestEntity
        {
            position = (2, 2),
            destination = (2, 2)
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0); //kb mindegy

        // Assert
        Assert.IsTrue(entity.NewDestinationCalled);
    }
    [TestMethod]
    public void Move_WhenAtDestination_ShouldNotCallNewDestination()
    {
        // Arrange
        var entity = new TestEntity
        {
            position = (2, 2),
            destination = (30, 30)
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsFalse(entity.NewDestinationCalled);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventHyena()
    {
        // Arrange
        var entity = new Hyena(1, (1, 1), 5, 0, null); //1,1bõl megy 3,1be
        entity.destination = (3, 1);

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Hyena", args.Type);
            Assert.AreEqual((1, 1), (args.OldX, args.OldY));
            Assert.AreEqual((2, 1), (args.NewX, args.NewY));
            Assert.AreEqual((3, 1), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsTrue(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((2, 1), entity.position);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventLion()
    {
        // Arrange
        var entity = new Lion(1, (1, 1), 5, 0, null); //1,1bõl megy 3,3be
        entity.destination = (3, 3);

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Lion", args.Type);
            Assert.AreEqual((1, 1), (args.OldX, args.OldY));
            Assert.AreEqual((2, 2), (args.NewX, args.NewY));
            Assert.AreEqual((3, 3), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsTrue(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((2, 2), entity.position);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventAntilope()
    {
        // Arrange
        var entity = new Antilope(1, (2, 2), 5, 0, null); //2,2bõl megy 2,1be
        entity.destination = (2, 1);

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Antilope", args.Type);
            Assert.AreEqual((2, 2), (args.OldX, args.OldY));
            Assert.AreEqual((2, 1), (args.NewX, args.NewY));
            Assert.AreEqual((2, 1), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsTrue(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((2, 1), entity.position);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventElephant()
    {
        // Arrange
        var entity = new Elephant(1, (2, 2), 5, 0, null); //2,2bõl megy 1,1be
        entity.destination = (1, 1);

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Elephant", args.Type);
            Assert.AreEqual((2, 2), (args.OldX, args.OldY));
            Assert.AreEqual((1, 1), (args.NewX, args.NewY));
            Assert.AreEqual((1, 1), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsTrue(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((1, 1), entity.position);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventRanger()
    {
        // Arrange
        var entity = new Ranger((2, 2), 5); //2,2bõl megy 1,2be
        entity.destination = (1, 2);

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Ranger", args.Type);
            Assert.AreEqual((2, 2), (args.OldX, args.OldY));
            Assert.AreEqual((1, 2), (args.NewX, args.NewY));
            Assert.AreEqual((1, 2), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsFalse(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((1, 2), entity.position);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventPoacher()
    {
        // Arrange
        var entity = new Poacher(5); //2,2bõl megy 1,3be
        entity.position = (2, 2);
        entity.destination = (1, 3);

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Poacher", args.Type);
            Assert.AreEqual((2, 2), (args.OldX, args.OldY));
            Assert.AreEqual((1, 3), (args.NewX, args.NewY));
            Assert.AreEqual((1, 3), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsFalse(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((1, 3), entity.position);
    }
    [TestMethod]
    public void Move_ShouldRaiseEntityMovedEventJeep()
    {
        // Arrange
        var entity = new Jeep((0,0), 5); //0,0bõl megy 1,0be
        entity.destination = (1, 0);

        entity.onThePath = true;

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
            Assert.AreEqual("Jeep", args.Type);
            Assert.AreEqual((0, 0), (args.OldX, args.OldY));
            Assert.AreEqual((1, 0), (args.NewX, args.NewY));
            Assert.AreEqual((1, 0), args.Destination);
            Assert.AreEqual(0, args.Index);
            Assert.IsFalse(args.IsAnimal);
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsTrue(eventRaised);
        Assert.AreEqual((1, 0), entity.position);
    }
    [TestMethod]
    public void Move_ShouldNotRaiseEntityMovedEventJeep()
    {
        // Arrange
        var entity = new Jeep((0, 0), 5); //0,0bõl menne 1,0be
        entity.destination = (1, 0);

        entity.onThePath = false;

        bool eventRaised = false;

        entity.EntityMoved += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        entity.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.IsFalse(eventRaised);
        Assert.AreEqual((0, 0), entity.position);
    }
    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void Move_AnimalWithNullOldest_ThrowsException()
    {
        var animal = new Lion(1, (0, 0), 5, 0, null);
        animal.theOldest = null; // ez dob majd
        animal.waterSource = new List<(int, int)>();
        animal.foodSource = new List<(int, int)>();

        animal.Move(1200, 600, 20, 20, 0);
    }
    [TestMethod]
    public void Move_AnimalDrinksWhenOnWaterSource()
    {
        var oldestanimal = new Lion(1, (0, 0), 5, 0, null);

        oldestanimal.waterSource = new List<(int, int)> { (1, 1) };
        oldestanimal.foodSource = new List<(int, int)> { (2, 2) };

        var animal = new Lion(1, (0, 0), 5, 0, oldestanimal);
        animal.position = (1, 1);
        animal.destination = (1, 1);
        if (animal.theOldest is not null)
        {
            animal.theOldest.someoneNeedsSomething = 1;
        }
        animal.Move(1200, 600, 20, 20, 0);
        Assert.IsTrue(animal.isDrinking);
    }
    [TestMethod]
    public void Move_AnimalEatsWhenOnFoodSource()
    {
        var oldestanimal = new Antilope(1, (0, 0), 5, 0, null);

        oldestanimal.waterSource = new List<(int, int)> { (1, 1) };
        oldestanimal.foodSource = new List<(int, int)> { (2, 2) };

        var animal = new Antilope(1, (0, 0), 5, 0, oldestanimal);

        animal.position = (2, 2);
        animal.destination = (2, 2);
        if (animal.theOldest is not null)
        {
            animal.theOldest.someoneNeedsSomething = 2;
        }
        animal.Move(1200, 600, 20, 20, 0);

        Assert.IsTrue(animal.isEating);
    }
    [TestMethod]
    public void Move_AnimalDestinationEqualsPosition_CallsNewDestination()
    {
        var animal = new Antilope(1, (0, 0), 5, 0, null);
        animal.position = (5, 5);
        animal.destination = (5, 5);
        if (animal.theOldest is not null)
        {
            animal.theOldest.someoneNeedsSomething = 0;
        }

        animal.Move(1200, 600, 20, 20, 0);

        Assert.AreNotEqual((5, 5), animal.destination);
    }
}
[TestClass]
public class HunterTests
{
    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void Attack_NullTarget_ThrowsException()
    {
        // Arrange
        var hunter = new Hunter();

        // Act
        hunter.Attack();
    }

    [TestMethod]
    public void Attack_AnimalTarget_KillsAnimal()
    {
        // Arrange
        var hunter = new Hunter();
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        hunter.entityToKill = animal;

        // Act
        hunter.Attack();

        // Assert
        Assert.IsTrue(animal.isDead);
        Assert.IsNull(hunter.entityToKill);
    }

    [TestMethod]
    public void Roll_Ranger_ReturnsNumberBetween1And7()
    {
        // Arrange
        var ranger = new Ranger((0, 0), 5);
        ranger.GetType().BaseType?.GetField("rnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ranger, new Random());

        // Act
        int roll = ranger.Roll();

        // Assert
        Assert.IsTrue(roll >= 1 && roll <= 7);
    }
}
[TestClass]
public class AnimalTest
{
    [TestMethod]
    public void GetOlder_ShouldIncreaseAge_WhenBirthdayPassed()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.birthDay = 0;
        animal.age = 1;

        animal.GetOlder(730); // 2 év telt el

        Assert.AreEqual(2, animal.age);
    }
    [TestMethod]
    public void GenerateHungerAndThirst_ShouldDecreaseHungerAndThirst()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.age = 3;
        animal.hunger = 10000;
        animal.thirst = 10000;

        animal.GenerateHungerAndThirst();

        Assert.AreEqual(9994, animal.hunger);
        Assert.AreEqual(9995, animal.thirst);
    }
    [TestMethod]
    public void Eat_ShouldIncreaseHunger()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.hunger = 9900;

        animal.Eat(1);

        Assert.AreEqual(9950, animal.hunger);
    }

    [TestMethod]
    public void Eat_ShouldCapHungerAndStartResting()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.hunger = 9999;

        animal.Eat(42);

        Assert.AreEqual(10000, animal.hunger);
        Assert.IsFalse(animal.isEating);
        Assert.IsTrue(animal.isResting);
        Assert.AreEqual(42, animal.restingDay);
    }
    [TestMethod]
    public void Drink_ShouldIncreaseThirst()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.thirst = 9800;

        animal.Drink();

        Assert.AreEqual(9900, animal.thirst);
    }

    [TestMethod]
    public void Drink_ShouldCapThirstAndStopDrinking()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.thirst = 9999;
        animal.isDrinking = true;

        animal.Drink();

        Assert.AreEqual(10000, animal.thirst);
        Assert.IsFalse(animal.isDrinking);
    }
    [TestMethod]
    public void Rest_ShouldStopResting_WhenOldestNeedsSomething()
    {
        var oldest = new Antilope(1, (1, 1), 5, 1, null);
        var animal = new Antilope(1, (1, 1), 5, 1, oldest);
        oldest.someoneNeedsSomething = 1;

        animal.Rest(10);

        Assert.IsFalse(animal.isResting);
    }

    [TestMethod]
    public void Rest_ShouldStopResting_WhenTooManyDaysPassed()
    {
        var oldest = new Antilope(1, (1, 1), 5, 1, null);
        var animal = new Antilope(1, (1, 1), 5, 1, oldest);
        animal.restingDay = 2;

        animal.Rest(10);

        Assert.IsFalse(animal.isResting);
    }

    [TestMethod]
    public void Rest_ShouldContinueResting_IfConditionsMet()
    {
        var oldest = new Antilope(1, (1, 1), 5, 1, null);
        var animal = new Antilope(1, (1, 1), 5, 1, oldest);
        animal.restingDay = 2;

        animal.Rest(3);

        Assert.IsTrue(animal.isResting);
    }
    [TestMethod]
    public void CheckDead_ShouldDie_WhenAgeTooHigh()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.age = 16;

        bool died = false;
        animal.EntityDied += (_, _) => died = true;

        animal.CheckDead();

        Assert.IsTrue(animal.isDead);
        Assert.IsTrue(died);
    }

    [TestMethod]
    public void CheckDead_ShouldDie_WhenThirstBelowZero()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.age = 10;
        animal.thirst = -1;

        bool died = false;
        animal.EntityDied += (_, _) => died = true;

        animal.CheckDead();

        Assert.IsTrue(animal.isDead);
        Assert.IsTrue(died);
    }

    [TestMethod]
    public void CheckDead_ShouldDie_WhenHungerBelowZero()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);

        animal.age = 10;
        animal.hunger = -1;

        bool died = false;
        animal.EntityDied += (_, _) => died = true;

        animal.CheckDead();

        Assert.IsTrue(animal.isDead);
        Assert.IsTrue(died);
    }
    [TestMethod]
    public void NeedsSomething_ShouldSetThirstyAndHungryCorrectly()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.thirst = 1000;
        animal.hunger = 2000;

        animal.NeedsSomething();

        Assert.IsTrue(animal.isThirsty);
        Assert.IsTrue(animal.isHungry);
    }

    [TestMethod]
    public void NeedsSomething_ShouldSetNotThirstyAndNotHungry()
    {
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.thirst = 5000;
        animal.hunger = 5000;

        animal.NeedsSomething();

        Assert.IsFalse(animal.isThirsty);
        Assert.IsFalse(animal.isHungry);
    }
    [TestMethod]
    public void Move_WhenAtDestinationAndNeedsWater_NewDestinationIsNearestWater()
    {
        // Arrange
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.position = (10, 10);
        animal.destination = (10, 10); // Új célpontot fog generálni
        animal.age = 5;
        animal.hunger = 10000;
        animal.thirst = 1000; // Szomjas
        animal.someoneNeedsSomething = 1; // Vizet keres

        animal.obstacles = new List<(int, int)> { (30, 30) };
        animal.foodSource = new List<(int, int)> { (80, 80) };
        animal.waterSource = new List<(int, int)> { (20, 20), (15, 15) };

        animal.theOldest = animal;

        // Act
        animal.Move(1200, 600, 20, 20,0);

        // Assert
        Assert.AreEqual((11, 11), animal.position);
        Assert.AreEqual((15, 15), animal.destination);
        Assert.IsTrue(animal.isDrinking == false);
    }
    [TestMethod]
    public void Move_WhenAtDestinationAndNeedsFood_NewDestinationIsNearestFood()
    {
        // Arrange
        var animal = new Antilope(1, (1, 1), 5, 1, null);
        animal.position = (10, 10);
        animal.destination = (10, 10); // Új célpontot fog generálni
        animal.age = 5;
        animal.hunger = 1000; // éhes
        animal.thirst = 10000;
        animal.someoneNeedsSomething = 2; // kaját keres

        animal.obstacles = new List<(int, int)> { (30, 30) };
        animal.foodSource = new List<(int, int)> { (80, 80), (90, 90) };
        animal.waterSource = new List<(int, int)> { (20, 20), (15, 15) };

        animal.theOldest = animal;

        // Act
        animal.Move(1200, 600, 20, 20, 0);

        // Assert
        Assert.AreEqual((11, 11), animal.position);
        Assert.AreEqual((80, 80), animal.destination);
        Assert.IsTrue(animal.isEating == false);
    }
}
//poachertests
[TestClass]
public class PoacherTests
{
    [TestMethod]
    public void Steal_AddsAnimalToListAndCallsOnEntityDied()
    {
        // Arrange
        var poacher = new Poacher(5); // konstruktor használata csak példányosításhoz
        var animal = new Antilope(1, (1, 1), 5, 1, null);

        // Act
        poacher.Steal(animal);

        // Assert
        Assert.AreEqual(1, poacher.stolenAnimals.Count);
        Assert.AreSame(animal, poacher.stolenAnimals[0]);
    }
}

[TestClass]
public class JeepTest {
    private TestJeepWrapper CreateJeep(List<(int, int)> path, int passengers = 0) {
        var jeep = new TestJeepWrapper((0, 0), 5);
        jeep.GiveJeepPath(path);
        jeep.passengers = passengers;
        jeep.onThePath = true;
        return jeep;
    }

    [TestMethod]
    public void NewDestination_AtPathEndWithPassengers_ReversesPath() {
        var startPath = new List<(int, int)> { (10, 10), (20, 20), (30, 30) };
        var path = new List<(int, int)> { (10, 10), (20, 20), (30, 30) };
        var jeep = CreateJeep(path, passengers: 4);
        jeep.pathIndex = path.Count;

        jeep.CallNewDestination(1000, 600, 20, 20);

        Assert.AreEqual(0, jeep.passengers);
        CollectionAssert.AreEqual(startPath.AsEnumerable().Reverse().ToList(), jeep.path);
        Assert.AreEqual(0, jeep.pathIndex);
    }

    [TestMethod]
    public void NewDestination_AtReversedPathEndWithoutPassengers_ClearsPath() {
        var reversedPath = new List<(int, int)> { (30, 30), (20, 20), (10, 10) };
        var jeep = CreateJeep(reversedPath);
        jeep.pathIndex = reversedPath.Count;

        jeep.CallNewDestination(1000, 600, 20, 20);

        Assert.AreEqual(0, jeep.path.Count);
        Assert.AreEqual(-1, jeep.pathIndex);
        Assert.IsFalse(jeep.onThePath);
    }

    [TestMethod]
    public void NewDestination_DuringMovement_AdvancesToNextPoint() {
        var path = new List<(int, int)> { (10, 10), (20, 20), (30, 30) };
        var jeep = CreateJeep(path);
        jeep.pathIndex = 1;

        jeep.CallNewDestination(1000, 600, 20, 20);

        Assert.AreEqual(path[1], jeep.destination);
        Assert.AreEqual(2, jeep.pathIndex);
    }

    [TestMethod]
    public void NewDestination_WithEmptyPath_NoChanges() {
        var jeep = CreateJeep(new List<(int, int)>());
        jeep.onThePath = true;

        jeep.CallNewDestination(1000, 600, 20, 20);

        Assert.IsFalse(jeep.onThePath);
        Assert.AreEqual(-1, jeep.pathIndex);
    }
}

internal class TestJeepWrapper : Jeep {
    public TestJeepWrapper((int, int) position, int fov) : base(position, fov) { }

    public void CallNewDestination(int w, int h, int tw, int th)
        => NewDestination(w, h, tw, th);

    public new List<(int, int)> path => base.path;
    public new int pathIndex {
        get => base.pathIndex;
        set => base.pathIndex = value;
    }
    public new bool onThePath {
        get => base.onThePath;
        set => base.onThePath = value;
    }
}

