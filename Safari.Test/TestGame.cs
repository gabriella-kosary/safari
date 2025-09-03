using Safari.Model;
using Safari.Persistence.Entities;
using Safari.Persistence;
using System.Data;
using System.Net.Http.Headers;
using System.Xml;
using Safari.Persistence.Tiles;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using System.IO.IsolatedStorage;
using Safari.Persistence.DataAccess;

namespace Safari.Test;

[TestClass]
public class TestGame
{
    private Game? _game = null;

    [TestInitialize]
    public void InitGameMechanicsTest() {
        _game = new Game();
        //alap �rt�kek
        if (_game is null) throw new Exception("Game was null while testing");
        _game.gameTable = new GameTable();
        _game.NewGame(1);
        _game.gameTable.rangers = new List<Ranger> { new Ranger((200, 200), 5) };
        _game.gameTable.poachers = new List<Poacher> { new Poacher(5) };
    }

    [DataTestMethod]
    [DataRow(200, 200, 0, 0)]
    [DataRow(200, 200, 45, 45)]
    public void TestKillWithRanger(int rangerX, int rangerY, int targetX, int targetY) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        _game.KillWithRanger((rangerX, rangerY), (targetX, targetY));

        Ranger ranger = _game.gameTable.rangers.First(r => r.position == (rangerX, rangerY));
        Assert.IsNotNull(ranger.entityToKill);
        Assert.AreEqual((targetX, targetY), ranger.entityToKill.position);
        Assert.IsTrue(ranger.isUserDestination);
    }
    
    [DataTestMethod]
    [DataRow(1, 1, 2, 200)]
    [DataRow(1, 1, 300, 3)]
    public void TestKillWithRangerWrongTarget(int rangerX, int rangerY, int targetX, int targetY) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        Assert.ThrowsException<Exception>(() => _game.KillWithRanger((rangerX, rangerY), (targetX, targetY)));

        Entity? entity = _game.gameTable.animals.FirstOrDefault(selectedAnimal => selectedAnimal.position == (targetX, targetY));
        Assert.IsNull(entity);
        entity = _game.gameTable.poachers.FirstOrDefault(selectedPoacher => selectedPoacher.position == (targetX, targetY));
        Assert.IsNull(entity);
    }
    
    [DataTestMethod]
    [DataRow(100, 1, 3, 3)]
    [DataRow(1, 100, 3, 3)]
    public void TestKillWithRangerWrongRanger(int rangerX, int rangerY, int targetX, int targetY) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        Assert.ThrowsException<Exception>(() => _game.KillWithRanger((rangerX, rangerY), (targetX, targetY)));

        Ranger? ranger = _game.gameTable.rangers.FirstOrDefault(selectedRanger => selectedRanger.position == (rangerX, rangerY));
        Assert.IsNull(ranger);
    }

    [DataTestMethod]
    [DataRow(1, 24 * 7.0)]
    [DataRow(2, 24.0)]
    [DataRow(3, 1.0)]
    public void SpeedChanged_ValidSpeed_UpdatesAndReturnsSpeed(int inputSpeed, double expectedSpeed) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        double result = _game.SpeedChanged(inputSpeed);

        Assert.AreEqual(expectedSpeed, _game.gameTable.speed);
        Assert.AreEqual(expectedSpeed, result);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void SpeedChanged_InvalidSpeed_ThrowsException() {
        Assert.IsNotNull(_game);
        _game.SpeedChanged(99);
    }

    [TestMethod]
    public void InsideSight_AnimalOnHill_CalculatesCorrectRadius() {
        var game = new Game();
        game.gameTable = new GameTable {
            tileHeight = 20,
            tileWidth = 20,
            gameBoard = new Tile[30, 60]
        };
        var hillPosition = (15, 25);
        game.gameTable.gameBoard[5, 5] = new Hill(hillPosition);
        var animal = new Antilope(1, hillPosition, fov: 3, 0, null) {
            waterSource = new List<(int, int)>()
        };

        game.InsideSight(animal);

        // Ha Hill-en �ll: r = (3+1) * (20+20)/2 = 4*40/2 = 80
        // Ellen�rizni kell, hogy a v�zforr�sok ezen a k�r�n bel�l vannak-e
        Assert.IsFalse(animal.waterSource.Any());
    }

    [TestMethod]
    public void InsideSight_CarnivoreSeesHerbivore_AddsToFoodSource() {
        var game = new Game();
        game.gameTable = new GameTable {
            tileHeight = 20,
            tileWidth = 20,
            gameBoard = new Tile[30, 60],
            animals = new List<Animal>()
        };
        var carnivore = new Lion(1, (100, 100), fov: 5, 0, null) {
            foodSource = new List<(int, int)>()
        };
        var herbivore = new Antilope(2, (120, 120), fov: 5, 0, null);
        game.gameTable.animals.Add(carnivore);
        game.gameTable.animals.Add(herbivore);

        game.InsideSight(carnivore);

        Assert.IsTrue(carnivore.foodSource.Contains(herbivore.position));
    }

    [TestMethod]
    public void InsideSight_NightTime_AnimalSeesRanger() {
        var game = new Game { IsDay = false };
        game.gameTable = new GameTable {
            tileHeight = 20,
            tileWidth = 20,
            rangers = new List<Ranger>(),
            vehicles = new List<Jeep>(),
            gameBoard = new Tile[30, 60]
        };
        var animal = new Antilope(1, (100, 100), fov: 5, 0, null) {
            canSee = false,
            waterSource = new List<(int, int)>()
        };
        var ranger = new Ranger((120, 120), fov: 5); // Elvileg l�tja
        game.gameTable.rangers.Add(ranger);

        game.InsideSight(animal);

        Assert.IsTrue(animal.canSee);
    }

    [TestMethod]
    public void InsideSight_PoacherSeesRanger_SetsCanSeeTrue() {
        var game = new Game();
        game.gameTable = new GameTable {
            tileHeight = 20,
            tileWidth = 20,
            rangers = new List<Ranger>(),
            gameBoard = new Tile[30, 60]
        };
        var poacher = new Poacher(fov: 5) {
            position = (100, 100),
            canSee = false
        };
        var ranger = new Ranger((105, 105), fov: 5); // Elvileg l�tja
        game.gameTable.rangers.Add(ranger);

        game.InsideSight(poacher);

        Assert.IsTrue(poacher.canSee);
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void InsideSight_NullGameTable_ThrowsException() {
        var game = new Game { gameTable = null };
        var animal = new Antilope(1, (0, 0), fov: 5, 0, null);

        game.InsideSight(animal);
    }

    [TestMethod]
    public void Test_GameProgress_TickIncrement() {
        Assert.IsNotNull(_game);

        int initialTicks = _game.Ticks;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(++initialTicks, _game.Ticks);
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(++initialTicks, _game.Ticks);
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(++initialTicks, _game.Ticks);
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(++initialTicks, _game.Ticks);
    }

    [TestMethod]
    public void Test_GameProgress_HoursIncrement() {
        Assert.IsNotNull(_game);

        _game.Ticks = 39;
        int initialHours = _game.Hours;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(initialHours + 1, _game.Hours);
    }

    [TestMethod]
    public void Test_GameProgress_DaysIncrement() {
        Assert.IsNotNull(_game);

        _game.Ticks = 959;
        int initialDays = _game.Days;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(initialDays + 1, _game.Days);
    }

    [TestMethod]
    public void Test_GameProgress_WeeksIncrement() {
        Assert.IsNotNull(_game);

        _game.Ticks = 6719;
        int initialWeeks = _game.Weeks;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(initialWeeks + 1, _game.Weeks);
    }

    [TestMethod]
    public void Test_GameProgress_DayNightToggle() {
        Assert.IsNotNull(_game);
        bool originalIsDay = _game.IsDay;
        _game.Ticks = 40 * 2 * 8 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreNotEqual(originalIsDay, _game.IsDay);
    }

    [TestMethod]
    public void Test_GameProgress_VisitorsToJeepsTriggered() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.Ticks = 0;
        _game.gameTable.visitors = 10;
        _game.gameTable.vehicles.Clear();
        _game.gameTable.vehicles.Add(new Jeep((0, 0), 5));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(_game.gameTable.visitors < 10);
    }

    [TestMethod]
    public void Test_GameProgress_AnimalAging() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        var animal = new Antilope(1, (10, 10), 5, _game.Days, null);
        int initialAge = animal.age;
        _game.gameTable.animals.Add(animal);
        _game.Days += 365;
        _game.Ticks = 40 * 24 - 1;
        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreEqual(initialAge + 1, animal.age);
    }

    [TestMethod]
    public void Test_GameProgress_GameEndWinConditionEasy() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        bool gameFinishedCalled = false;
        _game.GameFinished += (s, winner) => {
            gameFinishedCalled = true;
        };
        _game.gameTable.difficulty = 0;
        for(int i = 0; i < 21; i++)
        {
            _game.Buy("Antilope", (200,300));
        }
        _game.gameTable.money = 6000;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(gameFinishedCalled);
    }

    [TestMethod]
    public void Test_GameProgress_GameEndLoseCondition() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        bool gameFinishedCalled = false;
        _game.GameFinished += (s, winner) => {
            gameFinishedCalled = true;
        };
        _game.gameTable.animals.Clear();
        _game.gameTable.money = 0;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(gameFinishedCalled);
    }

    [TestMethod]
    public void Test_GameProgress_MultipleSequentialCalls() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialTicks = _game.Ticks;
        int initialHours = _game.Hours;
        int initialDays = _game.Days;
        int initialWeeks = _game.Weeks;

        for (int i = 0; i < 40; i++) {
            _game.GameProgress(null, EventArgs.Empty);
        }
        Assert.AreEqual(initialTicks + 40, _game.Ticks);
        Assert.AreEqual(initialHours + 1, _game.Hours);
        Assert.AreEqual(initialDays, _game.Days);
        Assert.AreEqual(initialWeeks, _game.Weeks);
    }

    [TestMethod]
    public void Test_NewGame_InitializesGameTable() {
        Assert.IsNotNull(_game);

        _game.NewGame(2);
        Assert.IsNotNull(_game.gameTable);
    }

    [TestMethod]
    public void Test_NewGame_InitializesTicksAndTime() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.AreEqual(0, _game.Ticks);
        Assert.AreEqual(0, _game.Hours);
        Assert.AreEqual(0, _game.Days);
        Assert.AreEqual(0, _game.Weeks);
    }

    [TestMethod]
    public void Test_NewGame_SetsDifficultyCorrectly() {
        Assert.IsNotNull(_game);

        int difficulty = 3;
        _game.NewGame(difficulty);
        Assert.IsNotNull(_game.gameTable);
        Assert.AreEqual(difficulty, _game.gameTable.difficulty);
    }

    [TestMethod]
    public void Test_NewGame_SetsStartingMoneyEasy() {
        Assert.IsNotNull(_game);

        _game.NewGame(0);
        Assert.IsNotNull(_game.gameTable);
        Assert.AreEqual(6500, _game.gameTable.money);
    }
    [TestMethod]
    public void Test_NewGame_SetsStartingMoneyMedium()
    {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        Assert.AreEqual(5500, _game.gameTable.money);
    }
    [TestMethod]
    public void Test_NewGame_SetsStartingMoneyHard()
    {
        Assert.IsNotNull(_game);

        _game.NewGame(2);
        Assert.IsNotNull(_game.gameTable);
        Assert.AreEqual(4700, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_NewGame_SetsCorrectSpeed() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        Assert.AreEqual(24.0, _game.gameTable.speed);
    }

    [TestMethod]
    public void Test_NewGame_SetsTileDimensionsCorrectly() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        Assert.AreEqual(600 / 30, _game.gameTable.tileHeight);
        Assert.AreEqual(1200 / 60, _game.gameTable.tileWidth);
    }

    [TestMethod]
    public void Test_NewGame_GameBoardIsNotNull() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        Assert.IsNotNull(_game.gameTable.gameBoard);
        Assert.AreEqual(30, _game.gameTable.gameBoard.GetLength(0));
        Assert.AreEqual(60, _game.gameTable.gameBoard.GetLength(1));
    }

    [TestMethod]
    public void Test_NewGame_AnimalsAddedToGameBoard() {
        Assert.IsNotNull(_game);
        
        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        Assert.IsTrue(_game.gameTable.animals.Count > 0);
    }

    [TestMethod]
    public void Test_NewGame_AnimalsIncludeAntilopeAndLion() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        var types = _game.gameTable.animals.Select(a => a.GetType().Name).ToList();
        Assert.IsTrue(types.Contains("Antilope"));
        Assert.IsTrue(types.Contains("Lion"));
    }

    [TestMethod]
    public void Test_NewGame_SetsStartDayTimeToTrue() {
        Assert.IsNotNull(_game);

       _game.NewGame(1);
        Assert.IsTrue(_game.IsDay);
    }

    [TestMethod]
    public void Test_NewGame_TileTypesIncludePlantsAndRiver() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        bool hasPlant = false;
        bool hasHill = false;
        Assert.IsNotNull(_game.gameTable);

        foreach (var tile in _game.gameTable.gameBoard) {
            if (tile is Plant)
                hasPlant = true;
            if (tile is Hill)
                hasHill = true;
        }

        Assert.IsTrue(hasPlant);
        Assert.IsTrue(hasHill);
    }

    [TestMethod]
    public void Test_NewGame_OnMoneyChangedEventInvoked() {
        Assert.IsNotNull(_game);

        bool moneyChangedInvokes = false;
        _game.MoneyChanged += (s, amount) =>
        {
            moneyChangedInvokes = true;
            Assert.AreEqual(5500, amount);
        };
        _game.NewGame(1);
        Assert.IsTrue(moneyChangedInvokes);
    }

    [TestMethod]
    public void Test_NewGame_OnTilesCreatedEventInvoked() {
        Assert.IsNotNull(_game);

        bool tilesCreatedIvoked = false;
        _game.TilesCreated += (s, args) =>
        {
            tilesCreatedIvoked = true;
            Assert.AreEqual(600, args.CanvasHeight);
            Assert.AreEqual(1200, args.CanvasWidth);
            Assert.IsNotNull(args.Tiles);
            Assert.IsNotNull(args.Enitites);
        };
        _game.NewGame(1);
        Assert.IsTrue(tilesCreatedIvoked);
    }

    [TestMethod]
    public void Test_NewGame_DoesNotThrowException() {
        try {
            Assert.IsNotNull(_game);
            _game.NewGame(1);
        } catch (Exception) {
            Assert.Fail("NewGame threw an exception unexpectedly.");
        }
    }

    [TestMethod]
    public void Test_NewGame_AllAnimalsHavePositionsInCanvasRange() {
        Assert.IsNotNull(_game);

        _game.NewGame(1);
        Assert.IsNotNull(_game.gameTable);
        foreach (var animal in _game.gameTable.animals) {
            Assert.IsTrue(animal.position.Item1 >= 0 && animal.position.Item1 <= 1200);
            Assert.IsTrue(animal.position.Item2 >= 0 && animal.position.Item2 <= 600);
        }
    }

    [DataTestMethod]
    [DataRow(0, 0, 0, 0, 0, 0)]
    [DataRow(10, 0, 0, 0, 0, 0)]
    [DataRow(0, 10, 0, 0, 0, 0)]
    [DataRow(20, 0, 0, 1, 20, 0)]
    [DataRow(0, 20, 1, 0, 0, 20)]
    [DataRow(30, 30, 2, 2, 40, 40)]
    [DataRow(29, 29, 1, 1, 20, 20)]
    [DataRow(31, 31, 2, 2, 40, 40)]
    [DataRow(69, 70, 4, 3, 60, 80)]
    [DataRow(1179, 0, 0, 59, 1180, 0)]
    [DataRow(0, 589, 29, 0, 0, 580)]
    [DataRow(1179, 589, 29, 59, 1180, 580)]
    public void Test_Buy_CanvasToMatrix(int canvasPosX, int canvasPosY, int matrixPosX, int matrixPosY, int positionX, int positionY)
    {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.Buy("Road", (canvasPosX, canvasPosY));
        Assert.IsTrue(_game.gameTable.gameBoard[matrixPosX, matrixPosY] is Road);
        Assert.AreEqual(positionX, _game.gameTable.gameBoard[matrixPosX, matrixPosY].position.Item1);
        Assert.AreEqual(positionY, _game.gameTable.gameBoard[matrixPosX, matrixPosY].position.Item2);
    }

    [TestMethod]
    public void Test_Buy_Road_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 50;
        int initialMoney = _game.gameTable.money;
        bool tilePlacedFired = false;
        (string type, int x, int y) args = default;
        _game.TilePlaced += (s, e) => {
            tilePlacedFired = true;
            args = (e.Type, e.X, e.Y);
        };

        _game.Buy("Road", (0, 0));

        Assert.IsTrue(tilePlacedFired);
        Assert.AreEqual("Road", args.type);
        Assert.AreEqual(0, args.x);
        Assert.AreEqual(0, args.y);
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Road_NotEnoughMoney() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 0;
        bool notEnough = false;
        _game.NotEnoughMoney += (s, e) => notEnough = true;

        _game.Buy("Road", (0, 0));

        Assert.IsTrue(notEnough);
        Assert.AreEqual(0, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Grass_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 50;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Grass", (0, 0));
        Assert.IsInstanceOfType(_game.gameTable.gameBoard[0, 0], typeof(Grass));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Bush_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 70;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Bush", (0, 0));
        Assert.IsInstanceOfType(_game.gameTable.gameBoard[0, 0], typeof(Bush));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Tree_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 90;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Tree", (0, 0));
        Assert.IsInstanceOfType(_game.gameTable.gameBoard[0, 0], typeof(Tree));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Lake_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 180;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Lake", (0, 0));
        Assert.IsInstanceOfType(_game.gameTable.gameBoard[0, 0], typeof(Lake));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Ranger_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 300;
        int initialMoney = _game.gameTable.money;
        bool entityPlaced = false;
        _game.EntityPlaced += (s, e) => {
            if (e.Type == "Ranger") entityPlaced = true;
        };

        _game.Buy("Ranger", (100, 200));

        Assert.IsTrue(entityPlaced);
        Assert.AreEqual(2, _game.gameTable.rangers.Count);
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Ranger_NotEnoughMoney() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 0;
        bool notEnough = false;
        _game.NotEnoughMoney += (s, e) => notEnough = true;

        _game.Buy("Ranger", (0, 0));

        Assert.IsTrue(notEnough);
        // mivel az initbe már van benne alapból egy
        Assert.AreEqual(1, _game.gameTable.rangers.Count);
    }

    [TestMethod]
    public void Test_Buy_Lion_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 600;
        int initialMoney = _game.gameTable.money;
        bool entityPlaced = false;
        _game.EntityPlaced += (s, e) => {
            if (e.Type == "Lion") entityPlaced = true;
        };

        _game.Buy("Lion", (5, 5));

        Assert.IsTrue(entityPlaced);
        Assert.IsTrue(_game.gameTable.animals.Any(a => a is Lion));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Lion_NotEnoughMoney() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int startingLionCnt = _game.gameTable.animals.Where(a => a is Lion).Count();
        _game.gameTable.money = 0;
        bool notEnough = false;
        _game.NotEnoughMoney += (s, e) => notEnough = true;

        _game.Buy("Lion", (0, 0));

        Assert.IsTrue(notEnough);
        Assert.AreEqual(startingLionCnt, _game.gameTable.animals.Where(a => a is Lion).Count());
    }

    [TestMethod]
    public void Test_Buy_Hyena_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 500;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Hyena", (5, 5));
        Assert.IsTrue(_game.gameTable.animals.Any(a => a is Hyena));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Elephant_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 450;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Elephant", (5, 5));
        Assert.IsTrue(_game.gameTable.animals.Any(a => a is Elephant));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }
    
    [TestMethod]
    public void Test_Buy_Elephant_PlacedEventArgs() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 450;
        int initialMoney = _game.gameTable.money;
        PlacedEventargs? placedEventargs = null;
        _game.EntityPlaced += (sender, eventArg) => {
            placedEventargs = eventArg;
        };
        _game.Buy("Elephant", (5, 5));
        Assert.IsTrue(_game.gameTable.animals.Any(a => a is Elephant));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
        Assert.IsNotNull(placedEventargs);
        Assert.IsTrue(placedEventargs.Cansee);
        Assert.IsTrue(placedEventargs.IsAnimal);
        Assert.IsTrue(placedEventargs.IsHerbivore);
        Assert.IsFalse(placedEventargs.IsCarnivore);
        Assert.AreEqual("Elephant", placedEventargs.Type);
        Assert.AreEqual(_game.gameTable.animals.Count(animal => animal is Elephant) - 1, placedEventargs.Index);
        Assert.AreEqual(5, placedEventargs.X);
        Assert.AreEqual(5, placedEventargs.Y);
    }
    
    [TestMethod]
    public void Test_Buy_Elephant_StatsEventArgs() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialMoney = _game.gameTable.money;
        StatEventArgs? statEventargs = null;
        _game.StatsChanged += (sender, eventArg) => {
            statEventargs = eventArg;
        };
        _game.Buy("Elephant", (5, 5));

        Assert.IsNotNull(statEventargs);
        Assert.AreEqual(3, statEventargs.Carnivores);
        Assert.AreEqual(12, statEventargs.Herbivores);
        Assert.AreEqual(0, statEventargs.Visitors);
    }

    //Volt, hogy fail-elt a money-nál nem tudom miért
    [TestMethod]
    public void Test_Buy_Antilope_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 350;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Antilope", (5, 5));
        Assert.IsTrue(_game.gameTable.animals.Any(a => a is Antilope));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }
    
    [TestMethod]
    public void Test_Buy_Chip_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        var firstAnimalPos = _game.gameTable.animals[0].position;

        int cost = 300;
        int initialMoney = _game.gameTable.money;
        _game.Buy("Chip", firstAnimalPos);
        Assert.IsTrue(_game.gameTable.animals[0].chipped);
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }
    
    [TestMethod]
    public void Test_Buy_Chip_NotEnoughMoney() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        var firstAnimalPos = _game.gameTable.animals[0].position;

        int cost = 300;
        _game.gameTable.money = 0;
        int initialMoney = _game.gameTable.money;
        bool notEnough = false;
        _game.NotEnoughMoney += (s, e) => notEnough = true;
        _game.Buy("Chip", firstAnimalPos);

        Assert.IsFalse(_game.gameTable.animals[0].chipped);
        Assert.AreNotEqual(initialMoney - cost, _game.gameTable.money);
        Assert.IsTrue(notEnough);
    }

    [TestMethod]
    public void Test_Buy_Jeep_Success() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int cost = 200;
        int initialMoney = _game.gameTable.money;
        bool entityPlaced = false;
        _game.EntityPlaced += (s, e) => {
            if (e.Type == "Jeep") entityPlaced = true;
        };

        _game.Buy("Jeep", (0, 0));

        Assert.IsTrue(entityPlaced);
        Assert.AreEqual(1, _game.gameTable.vehicles.Count);
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
    }

    [TestMethod]
    public void Test_Buy_Jeep_NotEnoughMoney() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 0;
        bool notEnough = false;
        _game.NotEnoughMoney += (s, e) => notEnough = true;

        _game.Buy("Jeep", (0, 0));

        Assert.IsTrue(notEnough);
        Assert.AreEqual(0, _game.gameTable.vehicles.Count);
    }

    [TestMethod]
    public void Test_Buy_Invalid_ThrowsException() {
        Assert.IsNotNull(_game);

        Assert.ThrowsException<Exception>(() => {
            _game.Buy("InvalidItem", (0, 0));
        });
    }

    [DataTestMethod]
    [DataRow("Grass", 50)]
    [DataRow("Bush", 70)]
    [DataRow("Tree", 90)]
    [DataRow("Lake", 180)]
    public void Test_Buy_AllTiles(string type, int cost) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialMoney = _game.gameTable.money;
        _game.Buy(type, (0, 0));
        Assert.AreEqual(initialMoney - cost, _game.gameTable.money);
        Assert.AreEqual(_game.gameTable.gameBoard[0, 0].GetType().Name, type);
    }
    
    [DataTestMethod]
    [DataRow("Grass", 50)]
    [DataRow("Bush", 70)]
    [DataRow("Tree", 90)]
    [DataRow("Lake", 180)]
    [DataRow("Road", 50)]
    public void Test_Buy_AllTiles_NotEnoughMoney(string type, int cost) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 0;
        int initMoney = 0;
        bool isNotEnoughMoneyTriggered = false;
        _game.NotEnoughMoney += (sender, eventArg) => {
            isNotEnoughMoneyTriggered = true;
        };

        _game.Buy(type, (0, 0));
        Assert.IsTrue(isNotEnoughMoneyTriggered);
        Assert.AreEqual(initMoney, _game.gameTable.money);
    }
    
    [DataTestMethod]
    [DataRow("Elephant")]
    [DataRow("Antilope")]
    [DataRow("Hyena")]
    [DataRow("Lion")]
    [DataRow("Ranger")]
    [DataRow("Jeep")]
    public void Test_Buy_AllEntities_NotEnoughMoney(string type) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 0;
        int initMoney = 0;
        bool isNotEnoughMoneyTriggered = false;
        _game.NotEnoughMoney += (sender, eventArg) => {
            isNotEnoughMoneyTriggered = true;
        };

        _game.Buy(type, (100, 100));
        Assert.IsTrue(isNotEnoughMoneyTriggered);
        Assert.AreEqual(initMoney, _game.gameTable.money);
    }
    
    [DataTestMethod]
    [DataRow("Elephant", true)]
    [DataRow("Antilope", true)]
    [DataRow("Hyena", false)]
    [DataRow("Lion", false)]
    public void Test_Buy_AllAnimals_EventArgs(string type, bool isHerb) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialMoney = _game.gameTable.money;
        PlacedEventargs? placedEventargs = null;
        _game.EntityPlaced += (sender, eventArg) => {
            placedEventargs = eventArg;
        };
        _game.Buy(type, (5, 5));
        Assert.IsTrue(_game.gameTable.animals.Any(a => a.Type == type));
        Assert.IsFalse(initialMoney == _game.gameTable.money);
        Assert.IsNotNull(placedEventargs);
        Assert.IsTrue(placedEventargs.Cansee);
        Assert.IsTrue(placedEventargs.IsAnimal);
        Assert.AreEqual(placedEventargs.IsHerbivore, isHerb);
        Assert.AreEqual(placedEventargs.IsCarnivore, !isHerb);
        Assert.AreEqual(type, placedEventargs.Type);
        Assert.AreEqual(_game.gameTable.animals.Count(animal => animal.GetType().Name == type) - 1, placedEventargs.Index);
        Assert.AreEqual(5, placedEventargs.X);
        Assert.AreEqual(5, placedEventargs.Y);
    }
    
    [DataTestMethod]
    [DataRow("Ranger")]
    public void Test_Buy_Ranger_EventArgs(string type) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialMoney = _game.gameTable.money;
        PlacedEventargs? placedEventargs = null;
        _game.EntityPlaced += (sender, eventArg) => {
            placedEventargs = eventArg;
        };
        _game.Buy(type, (0, 0));
        Assert.IsFalse(initialMoney == _game.gameTable.money);
        Assert.IsNotNull(placedEventargs);
        Assert.IsTrue(placedEventargs.Cansee);
        Assert.IsFalse(placedEventargs.IsAnimal);
        Assert.IsFalse(placedEventargs.IsHerbivore);
        Assert.IsFalse(placedEventargs.IsCarnivore);
        Assert.AreEqual(type, placedEventargs.Type);
        Assert.IsTrue(placedEventargs.Index >= 0);
        Assert.AreEqual(0, placedEventargs.X);
        Assert.AreEqual(0, placedEventargs.Y);
    }
    
    [DataTestMethod]
    [DataRow("Jeep")]
    public void Test_Buy_Jeep_EventArgs(string type) {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialMoney = _game.gameTable.money;
        PlacedEventargs? placedEventargs = null;
        _game.EntityPlaced += (sender, eventArg) => {
            placedEventargs = eventArg;
        };
        var startInd = _game.gameTable.startPos;
        var startPos = _game.gameTable.gameBoard[startInd.Item1, startInd.Item2].position;

        _game.Buy(type, (0, 0));
        Assert.IsFalse(initialMoney == _game.gameTable.money);
        Assert.IsNotNull(placedEventargs);
        Assert.IsTrue(placedEventargs.Cansee);
        Assert.IsFalse(placedEventargs.IsAnimal);
        Assert.IsFalse(placedEventargs.IsHerbivore);
        Assert.IsFalse(placedEventargs.IsCarnivore);
        Assert.AreEqual(type, placedEventargs.Type);
        Assert.IsTrue(placedEventargs.Index >= 0);
        Assert.AreEqual(startPos.Item1, placedEventargs.X);
        Assert.AreEqual(startPos.Item2, placedEventargs.Y);
    }

    [TestMethod]
    public void Test_PayRangers_RemovesRangersWhenPoor() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int rangerCnt = _game.gameTable.rangers.Count;
        _game.NewGame(1);
        _game.gameTable.money = 3;

        _game.Ticks = 2 * 24 * 7 * 4 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.AreEqual(rangerCnt - 1, _game.gameTable.rangers.Count);
    }

    [TestMethod]
    public void Test_FindPath_NoRoads_TriggersIncompleteRoadEvent() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        bool incompleteRoadTriggered = false;
        _game.IncompleteRoad += (s, e) => incompleteRoadTriggered = e;

        _game.Ticks = 100;
        _game.gameTable.visitors = 1000;
        _game.Buy("Jeep", (0, 0));
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(incompleteRoadTriggered);
    }

    [TestMethod]
    public void Test_AnimalReproduction_AddsNewAnimal() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int lionCnt = _game.gameTable.animals.Count(animal => animal is Lion);
        Animal? oldestLion = _game.gameTable.animals.First(animal => animal is Lion && animal.theOldest == animal);
        oldestLion.age = 3;

        _game.Ticks = 2 * 24 * 30 - 1;
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(_game.gameTable.animals.Count > lionCnt);
    }

    [TestMethod]
    public void Test_GenMap_TileCounts() {
        // max numbers in rows: hil:15, plant:3, lake:3
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int hillCnt = 0;
        int lakeCnt = 0;
        int plantCnt = 0;
        foreach (var tile in _game.gameTable.gameBoard) {
            if(tile is Hill) {
                hillCnt++;
            } else if(tile is Lake) {
                lakeCnt++;
            } else if(tile is Plant) {
                plantCnt++;
            }
        }

        Assert.IsTrue(hillCnt >= 0 && hillCnt < 15 * 30);
        Assert.IsTrue(lakeCnt >= 0 && lakeCnt < 3 * 30);
        Assert.IsTrue(plantCnt >= 0 && plantCnt < 3 * 30);
    }
    
    [TestMethod]
    public void Test_FindPath_NoRoads_ThenRoads_TriggersIncompleteRoadEvent() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        bool incompleteRoadTriggered = false;
        _game.IncompleteRoad += (s, e) => incompleteRoadTriggered = e;

        _game.Ticks = 100;
        _game.gameTable.visitors = 1000;
        _game.Buy("Jeep", (0, 0));
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(incompleteRoadTriggered);

        var jeepPath = FindPath(_game.gameTable.startPos, _game.gameTable.endPos, "Empty");
        foreach (var pos in jeepPath) {
            (int x, int y) = _game.gameTable.gameBoard[pos.row, pos.col].position;
            _game.gameTable.gameBoard[pos.row, pos.col] = new Road((x, y));
        }
        _game.Ticks = 200;
        _game.gameTable.visitors = 1000;
        _game.Buy("Jeep", (0, 0));
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(incompleteRoadTriggered);
    }
    
    [TestMethod]
    public void Test_CarnivoreLeader_GoesToFoodSource_WhenHungry() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal oldest = new Lion(5, (990, 500), 5, 0, null);
        oldest.age = 3;
        _game.gameTable.animals.Add(oldest);
        _game.gameTable.animals.Add(new Lion(5, (1000, 500), 5, 1, oldest));
        oldest.hunger = 30;
        Assert.IsNotNull(oldest.foodSource);
        oldest.foodSource.Add((30, 30));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(oldest.isHungry);
        Assert.AreEqual(2, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).All(animal => animal.destination == (30, 30)));
    }
    
    [TestMethod]
    public void Test_CarnivoreLeader_GoesToWater_WhenThirsty() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        for (int i = 0; i < _game.gameTable.gameBoard.GetLength(0); i++) {
            for (int j = 0; j < _game.gameTable.gameBoard.GetLength(1); j++) {
                (int x, int y) = _game.gameTable.gameBoard[i, j].position;
                _game.gameTable.gameBoard[i, j] = new Empty((x, y));
            }
        }

        Animal oldest = new Lion(5, (990, 500), 5, 0, null);
        oldest.age = 3;
        _game.gameTable.animals.Add(oldest);
        _game.gameTable.animals.Add(new Lion(5, (1000, 500), 5, 1, oldest));
        oldest.thirst = 30;
        Assert.IsNotNull(oldest.waterSource);
        oldest.waterSource.Add((30, 30));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(oldest.isThirsty);
        Assert.AreEqual(1, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).All(animal => animal.destination == (30, 30)));
    }
    
    [TestMethod]
    public void Test_Carnivore_GoesToWater_WhenThirsty() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        for (int i = 0; i < _game.gameTable.gameBoard.GetLength(0); i++) {
            for (int j = 0; j < _game.gameTable.gameBoard.GetLength(1); j++) {
                (int x, int y) = _game.gameTable.gameBoard[i, j].position;
                _game.gameTable.gameBoard[i, j] = new Empty((x, y));
            }
        }

        Animal oldest = new Lion(5, (990, 500), 5, 0, null);
        oldest.age = 3;
        _game.gameTable.animals.Add(oldest);
        _game.gameTable.animals.Add(new Lion(5, (1000, 500), 5, 1, oldest));
        Animal notTheOldest = new Lion(5, (1000, 500), 5, 1, oldest);
        _game.gameTable.animals.Add(notTheOldest);
        notTheOldest.thirst = 30;
        Assert.IsNotNull(notTheOldest.waterSource);
        notTheOldest.waterSource.Add((30, 30));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(notTheOldest.isThirsty);
        Assert.AreEqual(1, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).All(animal => animal.destination == (30, 30)));
    }
    
    [TestMethod]
    public void Test_CarnivoreLeader_GoesToFoodSource_WhenHungryButDoesntHaveFoodSources() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal oldest = new Lion(5, (990, 500), 5, 0, null);
        oldest.age = 3;
        _game.gameTable.animals.Add(oldest);
        Animal notTheOldest = new Lion(5, (1000, 500), 5, 1, oldest);
        _game.gameTable.animals.Add(notTheOldest);
        oldest.hunger = 30;

        Assert.IsNotNull(notTheOldest.foodSource);
        notTheOldest.foodSource.Add((30, 30));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(oldest.isHungry);
        Assert.AreEqual(2, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).Any(animal => animal.destination == (30, 30)));
    }
    
    [TestMethod]
    public void Test_Carnivore_GoesToFoodSource_WhenHungry() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal oldest = new Lion(5, (990, 500), 5, 0, null);
        oldest.age = 3;
        _game.gameTable.animals.Add(oldest);
        Animal notTheOldest = new Lion(5, (1000, 500), 5, 1, oldest);
        _game.gameTable.animals.Add(notTheOldest);
        notTheOldest.hunger = 30;
        
        Assert.IsNotNull(notTheOldest.foodSource);
        notTheOldest.foodSource.Add((30, 30));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(notTheOldest.isHungry);
        Assert.AreEqual(2, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).All(animal => animal.destination == (30, 30)));
    }

    [TestMethod]
    public void Test_Carnivore_GoesToFoodSource_WhenHungryButDoesntHaveFoodSources() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal oldest = new Lion(5, (990, 500), 5, 0, null);
        oldest.age = 3;
        _game.gameTable.animals.Add(oldest);
        Animal notTheOldest = new Lion(5, (1000, 500), 5, 1, oldest);
        _game.gameTable.animals.Add(notTheOldest);
        notTheOldest.hunger = 30;

        Assert.IsNotNull(oldest.foodSource);
        oldest.foodSource.Add((30, 30));
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(notTheOldest.isHungry);
        Assert.AreEqual(2, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).All(animal => animal.destination == (30, 30)));
    }
    
    [TestMethod]
    public void Test_Animal_OldestKill_GenNewOldest() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal? oldest = _game.gameTable.animals.First(animal => animal.group == 2).theOldest;
        Assert.IsNotNull(oldest);
        oldest.isDead = true;

        _game.GameProgress(null, EventArgs.Empty);

        int maxAge = _game.gameTable.animals.Where(animal => animal.group == oldest.group).Max(animal => animal.age);

        Assert.IsFalse(_game.gameTable.animals.Contains(oldest));
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == oldest.group).All(animal => animal.theOldest?.age == maxAge));
    }

    [TestMethod]
    public void Test_JeepFinishes_MoneyGrows() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        bool wasIncompleteRoad = false;
        _game.IncompleteRoad += (s, e) => wasIncompleteRoad = e;

        for (int i = 0; i < _game.gameTable.gameBoard.GetLength(0); i++) {
            for (int j = 0; j < _game.gameTable.gameBoard.GetLength(1); j++) {
                (int x, int y) = _game.gameTable.gameBoard[i, j].position;
                _game.gameTable.gameBoard[i, j] = new Empty((x, y));
            }
        }
        
        var jeepPath = FindPath(_game.gameTable.startPos, _game.gameTable.endPos, "Empty");
        foreach (var pos in jeepPath) {
            (int x, int y) = _game.gameTable.gameBoard[pos.row, pos.col].position;
            _game.gameTable.gameBoard[pos.row, pos.col] = new Road((x, y));
        }
        _game.Ticks = 200;
        _game.gameTable.visitors = 1000;
        _game.Buy("Jeep", (0, 0));

        int startMoney = _game.gameTable.money;
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(jeepPath.Count > 0);
        Assert.IsFalse(wasIncompleteRoad);

        Jeep jeep = _game.gameTable.vehicles.First();
        Assert.IsTrue(startMoney < _game.gameTable.money);
    }
    
    [TestMethod]
    public void Test_PoacherSpawn() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.NewGame(1);
        _game.GameProgress(null, EventArgs.Empty);

        int poacherCnt = _game.gameTable.poachers.Count;
        Assert.AreEqual(1, poacherCnt);

        for (int i = 0; i < 10; i++) {
            _game.Buy("Antilope", (500, 400));
        }
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(poacherCnt < _game.gameTable.poachers.Count);
    }
    
    [TestMethod]
    public void Test_WinEasy() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.NewGame(0);
        _game.gameTable.money = 7000;
        for (int i = 0; i < 20; i++) {
            _game.Buy("Antilope", (500, 400));
        }
        _game.gameTable.money = 6000;

        bool isGameFinishedTriggered = false;
        String message = "";
        _game.GameFinished += (sender, eventarg) => {
            isGameFinishedTriggered = true;
            message = eventarg;
        }; 
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(isGameFinishedTriggered);
        Assert.AreNotEqual("", message);
    }
    [TestMethod]
    public void Test_WinMeadium()
    {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.NewGame(1);
        _game.gameTable.money = 10500;
        for (int i = 0; i < 30; i++)
        {
            _game.Buy("Antilope", (500, 400));
        }
        _game.gameTable.money = 10000;

        bool isGameFinishedTriggered = false;
        String message = "";
        _game.GameFinished += (sender, eventarg) => {
            isGameFinishedTriggered = true;
            message = eventarg;
        };
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(isGameFinishedTriggered);
        Assert.AreNotEqual("", message);
    }
    [TestMethod]
    public void Test_WinHard()
    {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.NewGame(2);
        _game.gameTable.money = 17500;
        for (int i = 0; i < 50; i++)
        {
            _game.Buy("Antilope", (500, 400));
        }
        _game.gameTable.money = 20000;

        bool isGameFinishedTriggered = false;
        String message = "";
        _game.GameFinished += (sender, eventarg) => {
            isGameFinishedTriggered = true;
            message = eventarg;
        };
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(isGameFinishedTriggered);
        Assert.AreNotEqual("", message);
    }
    [TestMethod]
    public void Test_Lose() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.animals.Clear();
        _game.gameTable.money = 0;
        bool isGameFinishedTriggered = false;
        String message = "";
        _game.GameFinished += (sender, eventarg) => {
            isGameFinishedTriggered = true;
            message = eventarg;
        }; 
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(isGameFinishedTriggered);
        Assert.AreNotEqual("", message);
    }

    [TestMethod]
    public void Test_Carnivore_KillsHerbivore_WhenHungry() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal? oldest = _game.gameTable.animals.First(animal => animal is Lion).theOldest;
        Assert.IsNotNull(oldest);
        oldest.age = 3;
        Animal notTheOldest = _game.gameTable.animals.First(animal => animal is Lion && animal != oldest);
        notTheOldest.hunger = 30;

        Assert.IsNotNull(notTheOldest.foodSource);
        (int, int) herbivorePos = _game.gameTable.animals.Find(animal => animal is Herbivore)?.position ?? (0, 0);
        notTheOldest.foodSource.Add(herbivorePos);
        _game.GameProgress(null, EventArgs.Empty);
        int initAnimalCnt = _game.gameTable.animals.Count;

        Assert.IsTrue(notTheOldest.isHungry);
        Assert.AreEqual(2, oldest.someoneNeedsSomething);
        Assert.IsTrue(_game.gameTable.animals.Where(animal => animal.group == 5).All(animal => animal.destination == herbivorePos));

        oldest.position = oldest.destination;
        notTheOldest.position = notTheOldest.destination;

        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(_game.gameTable.animals.Any(animal => animal.isCaught));

        oldest.hunger = 10000;
        notTheOldest.hunger = 10000;

        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreEqual(initAnimalCnt - 1, _game.gameTable.animals.Count);
        Assert.IsFalse(oldest.isHungry);
        Assert.IsFalse(notTheOldest.isHungry);
    }

    [TestMethod]
    public void Test_InsideSight_Herbivore_WhenFoodSourceIsNearby() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal firstAnimal = _game.gameTable.animals[1];
        _game.gameTable.gameBoard[1, 1] = new Empty(_game.gameTable.gameBoard[1, 1].position);
        _game.InsideSight(firstAnimal);
        int initFoodSource = firstAnimal.foodSource?.Count ?? 0;

        _game.gameTable.gameBoard[1, 1] = new Bush(_game.gameTable.gameBoard[1, 1].position);
        _game.InsideSight(firstAnimal);
        Assert.AreEqual(initFoodSource + 1, firstAnimal.foodSource?.Count);
    }
    
    [TestMethod]
    public void Test_InsideSight_Carnivore_WhenFoodSourceIsNearby() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        Animal firstAnimal = _game.gameTable.animals.First(animal => animal is Carnivore);
        _game.InsideSight(firstAnimal);
        int initFoodSource = firstAnimal.foodSource?.Count ?? 0;

        _game.Buy("Antilope", firstAnimal.position);
        _game.InsideSight(firstAnimal);
        Assert.AreEqual(initFoodSource + 1, firstAnimal.foodSource?.Count);
    }

    [TestMethod]
    public void Test_Satisfaction_RemoveAnimals() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initSatisfaction = _game.gameTable.satisfaction;

        _game.gameTable.animals.RemoveAt(1);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreNotEqual(initSatisfaction, _game.gameTable.satisfaction);
    }
    
    [TestMethod]
    public void Test_InsideSight_Night_Jeep() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.Ticks = 40 * 2 * 8 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsFalse(_game.IsDay);
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(_game.gameTable.animals.Any(animal => !animal.canSee));
        _game.Buy("Jeep", (0, 0));
        _game.gameTable.vehicles.Last().passengers = 1;

        var startInd = _game.gameTable.startPos;
        var startPos = _game.gameTable.gameBoard[startInd.Item1, startInd.Item2].position;
        _game.Buy("Antilope", startPos);
        var lastAnilope = _game.gameTable.animals.Last(animal => animal is Antilope);
        lastAnilope.position = startPos;
        _game.InsideSight(lastAnilope);

        Assert.IsTrue(lastAnilope.canSee);
        int initSatisfaction = _game.gameTable.satisfaction;
    }
    
    [TestMethod]
    public void Test_InsideSight_NightDayChange_Animal() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.Ticks = 40 * 2 * 8 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsFalse(_game.IsDay);
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(_game.gameTable.animals.Any(animal => !animal.canSee));

        _game.Ticks = 40 * 3 * 8 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(_game.IsDay);
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(_game.gameTable.animals.Any(animal => animal.canSee));
    }

    [TestMethod]
    public void Test_OnEntityMurdered_CarnivoreKillsHerbivore() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        var carnivore = _game.gameTable.animals.First(animal => animal is Lion);
        var herbivore = _game.gameTable.animals[0];

        _game.gameTable.animals.Add(carnivore);
        _game.gameTable.animals.Add(herbivore);

        carnivore.hunger = 100;
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(herbivore.isCaught);

        carnivore.hunger = 10000;
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);
        
        Assert.IsFalse(_game.gameTable.animals.Contains(herbivore));
    }

    [TestMethod]
    public void Test_AnimalReproduction_Herbivore_AddsNewAnimalWhenConditionsMet() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialCount = _game.gameTable.animals.Count;

        var firstAnimal = _game.gameTable.animals[0].theOldest;
        Assert.IsNotNull(firstAnimal);
        firstAnimal.age = 4;

        _game.Ticks = 40 * 24 * 30 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(_game.gameTable.animals.Count > initialCount);
        Assert.IsTrue(_game.gameTable.animals.Any(a => a.transitionVector != (0, 0)));
    }
    
    [TestMethod]
    public void Test_AnimalReproduction_Carnivore_AddsNewAnimalWhenConditionsMet() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        int initialCount = _game.gameTable.animals.Count;

        var firstAnimal = _game.gameTable.animals.First(animal => animal is Carnivore).theOldest;
        Assert.IsNotNull(firstAnimal);
        firstAnimal.age = 4;

        _game.Ticks = 40 * 24 * 30 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.IsTrue(_game.gameTable.animals.Count > initialCount);
        Assert.IsTrue(_game.gameTable.animals.Any(a => a.transitionVector != (0, 0)));
    }

    [TestMethod]
    public void Test_GenMap_InitialTileDistribution() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        _game.NewGame(1);

        int hillCount = 0;
        int plantCount = 0;

        foreach (Tile tile in _game.gameTable.gameBoard) {
            if (tile is Hill) hillCount++;
            if (tile is Plant) plantCount++;
        }

        Assert.IsTrue(hillCount >= 0 && hillCount <= 15 * 30);
        Assert.IsTrue(plantCount >= 0 && plantCount <= 3 * 30);
    }

    [TestMethod]
    public void Test_SetNewStartEnd_PositionsOnEdges() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        _game.NewGame(1);

        (int sx, int sy) = _game.gameTable.startPos;
        (int ex, int ey) = _game.gameTable.endPos;

        Assert.IsTrue(
            (sx == 0 || sy == 0 || sx == 29 || sy == 59) &&
            (ex == 0 || ey == 0 || ex == 29 || ey == 59)
        );

        Assert.IsInstanceOfType(_game.gameTable.gameBoard[sx, sy], typeof(Road));
        Assert.IsInstanceOfType(_game.gameTable.gameBoard[ex, ey], typeof(Road));
    }

    [TestMethod]
    public void Test_GenMap_RowWiseTileLimits() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        _game.NewGame(1);

        for (int i = 0; i < _game.gameTable.gameBoard.GetLength(0); i++) {
            int hillCnt = 0;
            int plantCnt = 0;
            int lakeCnt = 0;

            for (int j = 0; j < _game.gameTable.gameBoard.GetLength(1); j++) {
                var tile = _game.gameTable.gameBoard[i, j];
                if (tile is Hill) hillCnt++;
                if (tile is Plant) plantCnt++;
                if (tile is Lake) lakeCnt++;
            }

            Assert.IsTrue(hillCnt <= 15, $"Row {i} has {hillCnt} hills (max 15 allowed)");

            Assert.IsTrue(plantCnt <= 3, $"Row {i} has {plantCnt} plants (max 3 allowed)");

            Assert.IsTrue(lakeCnt <= 3, $"Row {i} has {lakeCnt} lakes (max 3 allowed)");
        }
    }

    [TestMethod]
    public void Test_Poacher_AttacksAnimal_WhenEvery8thAnimal() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.GameProgress(null, EventArgs.Empty);
        for (int i = 0; i < 8; i++) {
            _game.gameTable.animals.Add(new Antilope(1, (100 + i, 100 + i), 5, 0, null));
        }

        _game.InsideSight(_game.gameTable.poachers[0]);

        var sziaLajos = _game.gameTable.poachers[0].entityToKill;
        Assert.IsNull(sziaLajos);
    }

    [TestMethod]
    public void Test_Poacher_StealsAnimal_WhenEvery5thAnimal() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.GameProgress(null, EventArgs.Empty);
        _game.gameTable.poachers.First().position = _game.gameTable.animals[5].position;
        _game.gameTable.poachers.First().destination = _game.gameTable.animals[5].destination;
        _game.gameTable.poachers.First().Move(1200, 600, _game.gameTable.tileWidth, _game.gameTable.tileHeight, 0);
        

        int initialAnimalCount = _game.gameTable.animals.Count;
        _game.GameProgress(null, EventArgs.Empty);

        _game.InsideSight(_game.gameTable.poachers[0]);

        Assert.AreEqual(initialAnimalCount - 1, _game.gameTable.animals.Count);
        Assert.AreEqual(1, _game.gameTable.poachers[0].stolenAnimals.Count);
    }
    
    [TestMethod]
    public void Test_Poacher_StopsAnimal_WhenEvery5thAnimal() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.GameProgress(null, EventArgs.Empty);
        _game.gameTable.poachers.First().position = _game.gameTable.animals[5].position;        

        int initialAnimalCount = _game.gameTable.animals.Count;
        _game.GameProgress(null, EventArgs.Empty);
        Assert.IsTrue(_game.gameTable.animals.Any(animal => animal.isCaught));
        Assert.IsFalse(_game.gameTable.poachers[0].stoleOrKilledToday);

        _game.InsideSight(_game.gameTable.poachers[0]);

        Assert.IsTrue(_game.gameTable.poachers[0].stoleOrKilledToday);
        Assert.AreEqual(initialAnimalCount - 1, _game.gameTable.animals.Count);
        Assert.AreEqual(1, _game.gameTable.poachers[0].stolenAnimals.Count);
    }
    
    [TestMethod]
    public void Test_Poacher_KillsAnimal_WhenEvery8thAnimal() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.GameProgress(null, EventArgs.Empty);
        _game.gameTable.animals[8].position = _game.gameTable.poachers.First().position;

        int initialAnimalCount = _game.gameTable.animals.Count;
        _game.GameProgress(null, EventArgs.Empty);

        _game.InsideSight(_game.gameTable.poachers[0]);

        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreEqual(initialAnimalCount - 1, _game.gameTable.animals.Count);
        Assert.AreEqual(0, _game.gameTable.poachers[0].stolenAnimals.Count);
    }

    [TestMethod]
    public void PayRangers_WhenInsufficientFunds_KeepsPartialRangers() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.Buy("Ranger", (100, 100));
        _game.Buy("Ranger", (200, 200));
        _game.Buy("Ranger", (200, 200));
        _game.Buy("Ranger", (200, 200));
        _game.Buy("Ranger", (200, 200));

        _game.gameTable.money = 340;

        _game.Ticks = 40 * 24 * 30 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreEqual(2, _game.gameTable.rangers.Count);
        Assert.AreEqual(340 - (2 * 140), _game.gameTable.money);
        Assert.AreEqual("Ranger", _game.gameTable.rangers[0].GetType().Name);
    }
    
    [TestMethod]
    public void Ranger_Poacher_KillEachOther() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);
        _game.Buy("Antilope", (200, 200));
        _game.Buy("Antilope", (200, 200));
        _game.Buy("Lion", (200, 200));
        _game.Buy("Lion", (200, 200));

        for (int i = 0; i < 30; i++) {
            _game.GameProgress(null, EventArgs.Empty);

            int initRangerCount = _game.gameTable.rangers.Count;
            int initPoacherCount = _game.gameTable.poachers.Count;
            Assert.AreNotEqual(0, initPoacherCount);
            _game.Buy("Ranger", (0, 0));
            initRangerCount++;
            _game.gameTable.rangers[0].position = _game.gameTable.poachers[0].position;

            _game.InsideSight(_game.gameTable.rangers[0]);
            _game.GameProgress(null, EventArgs.Empty);
            Assert.IsTrue((initRangerCount == _game.gameTable.rangers.Count && initPoacherCount >= _game.gameTable.poachers.Count) ||
                (initRangerCount > _game.gameTable.rangers.Count && initPoacherCount <= _game.gameTable.poachers.Count));
        }
    }

    [TestMethod]
    public void PayRangers_WhenZeroFunds_RemovesAllRangers() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 0;
        _game.Buy("Ranger", (0, 0));
        _game.Buy("Ranger", (0, 0));
        _game.Buy("Ranger", (0, 0));
        _game.Buy("Ranger", (0, 0));
        _game.Buy("Ranger", (0, 0));

        _game.Ticks = 40 * 24 * 30 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreEqual(0, _game.gameTable.rangers.Count);
        Assert.AreEqual(0, _game.gameTable.money);
    }

    [TestMethod]
    public void PayRangers_WhenExactMultiple_KeepsFullRangers() {
        Assert.IsNotNull(_game);
        Assert.IsNotNull(_game.gameTable);

        _game.gameTable.money = 1320;
        _game.Buy("Ranger", (200, 100));
        _game.Buy("Ranger", (200, 100));
        _game.Buy("Ranger", (200, 100));

        _game.Ticks = 40 * 24 * 30 - 1;
        _game.GameProgress(null, EventArgs.Empty);
        _game.GameProgress(null, EventArgs.Empty);

        Assert.AreEqual(3, _game.gameTable.rangers.Count);
        Assert.AreEqual(0, _game.gameTable.money);
    }

    //[TestMethod]
    //public void Test_LoadGame_Success() {
    //    Assert.IsNotNull(_game);
    //    // Várni kell, mivel ugyanazta fájlt írja, olvassa a többi teszt is
    //    try {
    //        _game.SaveGame();
    //    } catch (Exception) {
    //        Thread.Sleep(1500);
    //        _game.SaveGame();
    //
    //    }
    //    try {
    //        _game.LoadGame();
    //    } catch (Exception) {
    //        Thread.Sleep(1500);
    //        _game.LoadGame();
    //
    //    }
    //
    //    Assert.IsNotNull(_game.gameTable);
    //    Assert.IsNotNull(_game.gameTable.animals);
    //    Assert.IsNotNull(_game.gameTable.gameBoard);
    //    Assert.IsNotNull(_game.gameTable.poachers);
    //    Assert.IsNotNull(_game.gameTable.rangers);
    //    Assert.IsNotNull(_game.gameTable.vehicles);
    //    Assert.AreNotEqual(_game.gameTable.startPos, _game.gameTable.endPos);
    //    Assert.AreNotEqual(0, _game.gameTable.tileHeight);
    //    Assert.AreNotEqual(0, _game.gameTable.tileWidth);
    //    Assert.IsTrue(_game.gameTable.animals.All(animal => animal.theOldest != null));
    //}
    //
    //public void Test_LoadGame_Fail() {
    //    Assert.IsNotNull(_game);
    //    String path = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "";
    //    Assert.IsNotNull(path);
    //    File.Delete(path);
    //    try {
    //        _game.LoadGame();
    //    } catch (Exception) {
    //        Thread.Sleep(300);
    //        Assert.ThrowsException<IOException>(() => _game.LoadGame());
    //    }
    //}
    //
    //[TestMethod]
    //public void Test_SaveGame_Success() {
    //    Assert.IsNotNull(_game);
    //    try {
    //        _game.SaveGame();
    //    } catch (Exception) {
    //        Thread.Sleep(200);
    //        _game.SaveGame();
    //    }
    //
    //    String path = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "";
    //    Assert.IsNotNull(path);
    //    Assert.IsTrue(File.Exists(Path.Combine(path, "SavedGame", "Save.json")));
    //}

    private List<(int row, int col)> FindPath((int, int) start, (int, int) goal, String type) {
        if (_game == null) throw new NullReferenceException("game was null");
        if (_game.gameTable == null) throw new NullReferenceException("gameTable was null");
        int row = _game.gameTable.gameBoard.GetLength(0), col = _game.gameTable.gameBoard.GetLength(1);
        var dist = new int[row, col];
        var prev = new (int, int)?[row, col];
        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                dist[i, j] = int.MaxValue;
        dist[start.Item1, start.Item2] = 0;
        var pq = new PriorityQueue<(int, int), int>();
        pq.Enqueue(start, 0);
        int[][] dirs = [[1, 0], [-1, 0], [0, 1], [0, -1]];
        while (pq.Count > 0) {
            var cur = pq.Dequeue();
            if (cur.Equals(goal))
                break;
            foreach (var dir in dirs) {
                int ni = cur.Item1 + dir[0], nj = cur.Item2 + dir[1];
                if (!(ni < 0 || ni >= row || nj < 0 || nj >= col || _game.gameTable.gameBoard[ni, nj].GetType().Name != type)) {
                    int alt = dist[cur.Item1, cur.Item2] + 1;
                    if (alt < dist[ni, nj]) {
                        dist[ni, nj] = alt;
                        prev[ni, nj] = cur;
                        pq.Enqueue((ni, nj), alt);
                    }
                }
            }
        }
        var path = new List<(int, int)>();
        if (dist[goal.Item1, goal.Item2] == int.MaxValue) {
            return path;
        }
        for (var indexes = goal; !indexes.Equals(start); indexes = prev[indexes.Item1, indexes.Item2] ?? (0, 0)) {
            path.Add(indexes);
        }
        path.Add(start);
        path.Reverse();
        return path;
    }
}
