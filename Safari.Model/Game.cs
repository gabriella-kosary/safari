
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using Safari.Persistence;
using Safari.Persistence.DataAccess;
using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;

namespace Safari.Model
{
    public class Game
    {
        #region Fields
        //Ez így elég lassú, de így accurate
        private readonly double fast = 1;
        private readonly double mid = 24;
        private readonly double slow = 24*7;

        private readonly int _canvasHeight = 600;
        private readonly int _canvasWidth = 1200;
        private readonly int _matrixHeight = 30;
        private readonly int _matrixWidth = 60;
        // kezdő és végpontok ha nem generáljuk
        private (int, int) _startPosition;
        private (int, int) _endPosition;
        private bool _isDay;
        private bool _incompleteRoadWasTriggered;
        private int _herbivores => gameTable?.animals.Where(animal => animal is Herbivore).Count() ?? 0;
        private int _carnivores => gameTable?.animals.Where(animal => animal is Carnivore).Count() ?? 0;
        private int _animalGroupCount = 1;
        private DataAccess dataAccess = new();
        #endregion

        #region Properties
        public GameTable? gameTable { get; set; }
        public int Ticks { get; set; }
        public int Hours  { get; set; }
        public int Days  { get; set; }
        public int Weeks  { get; set; }
        public bool IsDay {
            get => _isDay;
            set {
                _isDay = value;
                OnDayNightChanged();
            }
        }
        #endregion

        #region GameBalanceParameters
        //gamebalance szempontjából változtatható értékek
        private readonly int startingMoneyEasy = 6500;
        private readonly int startingMoneyMedium = 5500;
        private readonly int startingMoneyHard = 4700;
        private readonly int roadCost = 50;
        private readonly int grassCost = 50;
        private readonly int bushCost = 70;
        private readonly int treeCost = 90;
        private readonly int lakeCost = 180;
        private readonly int rangerCost = 300;
        private readonly int lionCost = 600;
        private readonly int hyenaCost = 500;
        private readonly int elephantCost = 450;
        private readonly int antilopeCost = 350;
        private readonly int jeepCost = 200;
        private readonly int chipCost = 300;
        private readonly int fov = 5;
        private readonly int rangerSalary = 140;
        private readonly int ticketPrice = 150;
        // win conditions
        private readonly int winConAnimalEasy = 20;
        private readonly int winConAnimalMedium = 30;
        private readonly int winConAnimalHard = 50;
        private readonly int winConMoneyEasy = 6000;
        private readonly int winConMoneyMedium = 10000;
        private readonly int winConMoneyHard = 20000;
        #endregion

        #region Events
        public event EventHandler<int>? MoneyChanged;
        public event EventHandler<EntityMovedEventArgs>? EntityMoved;
        public event EventHandler<PlacedEventargs>? EntityPlaced;
        public event EventHandler<PlacedEventargs>? EntityDied;
        public event EventHandler<PlacedEventargs>? TilePlaced;
        public event EventHandler<AnimalStateEventargs>? AnimalStateChanged;
        public event EventHandler<string>? GameFinished;
        public event EventHandler<bool>? IncompleteRoad;
        public event EventHandler<TilesCreatedEventArgs>? TilesCreated;
        public event EventHandler? NotEnoughMoney;
        public event EventHandler<int>? DaysChanged;
        public event EventHandler<bool>? DayNightChanged;
        public event EventHandler<PlacedEventargs>? CanSeeChanged;
        public event EventHandler<StatEventArgs>? StatsChanged;
        #endregion

        public Game()
        {
            gameTable = null;    
        }

        #region PublicFunctions
        public void NewGame(int nehezseg)
        {
            IsDay = true;
            Hours = 0;
            Days = 0;
            Weeks = 0;
            Ticks = 0;
            gameTable = new GameTable();
            gameTable.days = 0;
            gameTable.difficulty = nehezseg;
            if(nehezseg == 0)
            {
                gameTable.money = startingMoneyEasy;
            }
            else if (nehezseg == 1)
            {
                gameTable.money = startingMoneyMedium;
            }
            else if (nehezseg == 2)
            {
                gameTable.money = startingMoneyHard;
            }
            gameTable.speed = mid;
            gameTable.satisfaction = 50;
            gameTable.tileHeight = _canvasHeight / _matrixHeight;
            gameTable.tileWidth = _canvasWidth / _matrixWidth;
            GenMap();

            OnMoneyChanged();
        }
        public void Buy(string whatToBuy, (int, int) coords)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            (int, int) matrixCoords = CanvasToMatrix(coords);
            if (matrixCoords == _startPosition || matrixCoords == _endPosition) return;
            switch (whatToBuy)
            {
                case "Road":
                    if(roadCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        var pos = gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2].position;
                        gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2] = new Road(pos);
                        gameTable.money = gameTable.money - roadCost;
                        OnMoneyChanged();
                        OnTilePlaced(whatToBuy, pos.Item1, pos.Item2);
                    }
                    break;
                case "Grass":
                    if (grassCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        var pos = gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2].position;
                        gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2] = new Grass(pos);
                        gameTable.money = gameTable.money - grassCost;
                        OnMoneyChanged();
                        OnTilePlaced(whatToBuy, pos.Item1, pos.Item2);
                    }
                    break;
                case "Bush":
                    if (bushCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        var pos = gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2].position;
                        gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2] = new Bush(pos);
                        gameTable.money = gameTable.money - bushCost;
                        OnMoneyChanged();
                        OnTilePlaced(whatToBuy, pos.Item1, pos.Item2);
                    }
                    break;
                case "Tree":
                    if (treeCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        var pos = gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2].position;
                        gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2] = new Tree(pos);
                        gameTable.money = gameTable.money - treeCost;
                        OnMoneyChanged();
                        OnTilePlaced(whatToBuy, pos.Item1, pos.Item2);
                    }
                    break;
                case "Lake":
                    if (lakeCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        var pos = gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2].position;
                        gameTable.gameBoard[matrixCoords.Item1, matrixCoords.Item2] = new Lake(pos);
                        gameTable.money = gameTable.money - lakeCost;
                        OnMoneyChanged();
                        OnTilePlaced(whatToBuy, pos.Item1, pos.Item2);
                    }
                    break;
                case "Ranger":
                    if (rangerCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        var ranger = new Ranger(coords, fov);
                        GiveEntityEvents(ranger);
                        gameTable.rangers.Add(ranger);
                        gameTable.money = gameTable.money - rangerCost;
                        OnMoneyChanged();
                        OnEntityPlaced(ranger);
                    }
                    break;
                case "Lion":
                    if (lionCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        if (gameTable == null) throw new NullReferenceException("gameTable was null");
                        Animal? oldest = SearchTheOldest(coords, "Lion");
                        if (oldest is null) _animalGroupCount++;
                        Lion lion = new Lion(_animalGroupCount, coords, fov, Days, oldest);
                        GiveEntityEvents(lion);
                        gameTable.animals.Add(lion);
                        OnStatsChanged();
                        gameTable.money = gameTable.money - lionCost;
                        OnMoneyChanged();
                        OnEntityPlaced(lion);
                    }
                    break;
                case "Hyena":
                    if (hyenaCost > gameTable?.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        if (gameTable == null) throw new NullReferenceException("gameTable was null");
                        Animal? oldest = SearchTheOldest(coords, "Hyena");
                        if (oldest is null) _animalGroupCount++;
                        Hyena hyena = new Hyena(_animalGroupCount, coords, fov, Days, oldest);
                        GiveEntityEvents(hyena);
                        gameTable.animals.Add(hyena);
                        OnStatsChanged();
                        gameTable.money = gameTable.money - hyenaCost;
                        OnMoneyChanged();
                        OnEntityPlaced(hyena);
                    }
                    break;
                case "Elephant":
                    if (elephantCost > gameTable?.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        if (gameTable == null) throw new NullReferenceException("gameTable was null");
                        Animal? oldest = SearchTheOldest(coords, "Elephant");
                        if (oldest is null) _animalGroupCount++;
                        Elephant elephant = new Elephant(_animalGroupCount, coords, fov, Days, oldest);
                        GiveEntityEvents(elephant);
                        gameTable.animals.Add(elephant);
                        OnStatsChanged();
                        gameTable.money = gameTable.money - elephantCost;
                        OnMoneyChanged();
                        OnEntityPlaced(elephant);
                    }
                    break;
                case "Antilope":
                    if (antilopeCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        if (gameTable == null) throw new NullReferenceException("gameTable was null");
                        Animal? oldest = SearchTheOldest(coords, "Antilope");
                        if (oldest is null) _animalGroupCount++;
                        Antilope antilope = new Antilope(_animalGroupCount, coords, fov, Days, oldest);
                        GiveEntityEvents(antilope);
                        gameTable.animals.Add(antilope);
                        OnStatsChanged();
                        gameTable.money = gameTable.money - antilopeCost;
                        OnMoneyChanged();
                        OnEntityPlaced(antilope);
                    }
                    break;
                case "Jeep":
                    if (jeepCost > gameTable.money)
                    {
                        OnNotEnoughMoney();
                    }
                    else
                    {
                        coords = MatrixToCanvas(_startPosition);
                        var jeep = new Jeep(coords, fov);
                        GiveEntityEvents(jeep);
                        gameTable.vehicles.Add(jeep);
                        gameTable.money = gameTable.money - jeepCost;
                        OnMoneyChanged();
                        OnEntityPlaced(jeep);
                    }
                    break;
                case "Chip":
                    if (chipCost > gameTable.money) {
                        OnNotEnoughMoney();
                    } else {
                        Animal? animal = gameTable.animals.Find(animal => animal.position == coords);
                        if (animal is null) throw new Exception("Animal was not found");
                        animal.chipped = true;
                        gameTable.money = gameTable.money - chipCost;
                        OnMoneyChanged();
                    }
                    break;
                default:
                    throw new Exception("Nem implementált opció");
            }
        }
        public void KillWithRanger((int, int) rangerCoords, (int, int) whatToKill) {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            Ranger? selectedRanger = gameTable?.rangers.Where(ranger => ranger.position == rangerCoords).FirstOrDefault();
            if (selectedRanger == null) throw new Exception($"Could not find the selected ranger. It's position is {rangerCoords.ToString()}");
            //this makes the code undreadable...
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            Entity? entityToKill = gameTable.animals.Find(animal => animal.position == whatToKill);
            if (entityToKill == null) 
            {
                entityToKill = gameTable.poachers.Find(poacher => poacher.position == whatToKill);
            }
            if (entityToKill == null) throw new Exception($"Could not find what to kill. The position is: {whatToKill}");

            selectedRanger.destination = selectedRanger.position;
            selectedRanger.entityToKill = entityToKill;
            selectedRanger.isUserDestination = true;
        }
        public double SpeedChanged(int changedSpeed)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            if (changedSpeed == 1) {
                gameTable.speed = slow;
                return slow;
            } else if (changedSpeed == 2) {
                gameTable.speed = mid;
                return mid;
            } else if (changedSpeed == 3) {
                gameTable.speed = fast;
                return fast;
            } else {
                throw new Exception("Invalid speed value!");
            }
        }
        public void GameProgress(object? sender, EventArgs e) {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            PoacherSpawn();
            TheGroupNeedsSomething();
            MoveTheMovables();
            CheckGameEnd();
            SatisfactionCheck();
            if(Ticks % (40 * 24 * 30) == 0)
            {
                AnimalReproduction();
                PayRangers();
            }
            if (Ticks % 100 == 0) {
                VisitorsToJeeps();
            }
            Ticks++;
            if (Ticks % 40 == 0) {
                Hours++;
                if (IsDay) VisitorSpawn();
            }
            if (Ticks % (40 * 24) == 0) {
                Days++;
                gameTable.days = Days;
                foreach (Animal animal in gameTable.animals) {
                    animal.GetOlder(Days);
                }
                foreach (Poacher poacher in gameTable.poachers) {
                    poacher.stoleOrKilledToday = false;
                }
                OnDaysChanged();
            }
            if (Ticks % (40 * 24 * 7) == 0) {
                Weeks++;
            }
            if (!IsDay && Ticks % (40 * 8) == 0) {
                IsDay = true;
            }
            if (IsDay && Ticks % (40 * 3 * 8) == 16 * 40) {
                IsDay = false;
                if (gameTable == null) throw new NullReferenceException("gameTable was null");
                foreach (var jeep in gameTable.vehicles)
                {
                    if (!jeep.onThePath)
                    {
                        jeep.passengers = 0;
                    }
                }
                gameTable.visitors = 0;
                OnStatsChanged();
            }
        }
        public void InsideSight(Entity entity)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int r;
            (int x, int y) = CanvasToMatrix(entity.position);
            if (gameTable.gameBoard[x, y] is Hill)
            {
                r = (entity.fov + 1) * (gameTable.tileHeight + gameTable.tileWidth) / 2;
            }
            else
            {
                r = entity.fov * (gameTable.tileHeight + gameTable.tileWidth) / 2;
            }

            if (entity is Carnivore carnivore)
            {
                if (carnivore.foodSource == null) throw new NullReferenceException("foodSource was null");
                if (carnivore == null) throw new NullReferenceException("carnivore was null");
                int biggerFov = (carnivore.isHungry || carnivore.theOldest?.someoneNeedsSomething == 2) ? r * 4 : r; 
                foreach (Animal animal in gameTable.animals)
                {
                    if (animal is Herbivore)
                    {
                        if(InsideSightCircle(biggerFov, animal.position.Item1 - carnivore.position.Item1, animal.position.Item2 - carnivore.position.Item2))
                        {
                            if (!carnivore.foodSource.Contains(animal.position)) carnivore.foodSource.Add(animal.position);
                        }
                        if(carnivore.isHungry &&
                            InsideSightCircle(r / 2, animal.position.Item1 - carnivore.position.Item1, animal.position.Item2 - carnivore.position.Item2))
                        {
                            if(!animal.isCaught && !carnivore.isEating) {
                                animal.GetCaught();
                                carnivore.isEating = true;
                            }
                        }
                    }
                }
                List<(int, int)> notThere = new List<(int, int)>();
                foreach ((int, int) food in carnivore.foodSource)
                {
                    if (InsideSightCircle(biggerFov, food.Item1 - carnivore.position.Item1, food.Item2 - carnivore.position.Item2))
                    {
                        bool wasFound = false;
                        foreach (Animal potencialFood in gameTable.animals)
                        {
                            if (potencialFood is Herbivore && potencialFood.position == food) wasFound = true;
                        }
                        if (!wasFound) notThere.Add(food);
                    }
                }
                foreach ((int, int) pos in notThere)
                {
                    carnivore.foodSource.Remove(pos);
                }
                notThere.Clear();
            }
            else if (entity is Herbivore herbivore)
            {
                SearchForAnimalResource(herbivore, r, "Food");

                //if (herbivore.foodSource == null) throw new NullReferenceException("foodSource was null");
                //ezt lehetne optimalizálni, hogy i,j-vel megyünk radius-ba kifelé, ahol már kint van, azt már nem nézzük
                //foreach (Tile tile in gameTable.gameBoard)
                //{
                //    if (tile is Plant && InsideSightCircle(r, tile.position.Item1 - herbivore.position.Item1, tile.position.Item2 - herbivore.position.Item2))
                //    {
                //        if (!herbivore.foodSource.Contains(tile.position)) herbivore.foodSource.Add(tile.position);
                //    }
                //}
            }
            if (entity is Animal animal1) {
                SearchForAnimalResource(animal1, r, "Water");

                //if (animal1.waterSource == null) throw new NullReferenceException("waterSource was null");
                //ezt lehetne optimalizálni, hogy i,j-vel megyünk radius-ba kifelé, ahol már kint van, azt már nem nézzük
                //foreach (Tile tile in gameTable.gameBoard) {
                //    if (tile is Water && InsideSightCircle(r, tile.position.Item1 - animal1.position.Item1, tile.position.Item2 - animal1.position.Item2)) {
                //        if (!animal1.waterSource.Contains(tile.position)) animal1.waterSource.Add(tile.position);
                //    }
                //}
                if (animal1.chipped && animal1.canSee) {
                    return;
                } else {
                    if(!animal1.canSee) {
                        animal1.canSee = true;
                        OnCanSeeChanged(animal1);

                    }
                }
                if (!IsDay) {
                    bool iWasSeen = false;
                    foreach (Ranger ranger in gameTable.rangers) {
                        if (InsideSightCircle(r, ranger.position.Item1 - animal1.position.Item1, ranger.position.Item2 - animal1.position.Item2)) {
                            iWasSeen = true;
                            animal1.canSee = true;
                            OnCanSeeChanged(animal1);
                        }
                    }
                    foreach (Jeep jeep in gameTable.vehicles) {
                        if (jeep.passengers > 0 &&
                            InsideSightCircle(r, jeep.position.Item1 - animal1.position.Item1, jeep.position.Item2 - animal1.position.Item2)) {
                            iWasSeen = true;
                            animal1.canSee = true;
                            OnCanSeeChanged(animal1);
                        }
                    }
                    if (!iWasSeen) {
                        if (animal1.canSee) {
                            animal1.canSee = false;
                            OnCanSeeChanged(animal1);
                        }
                    }
                } else {
                    if (!animal1.canSee) {
                        animal1.canSee = true;
                        OnCanSeeChanged(animal1);
                    }
                }
            } 
            else if (entity is Poacher poacher)
            {
                bool poacherWasSeen = false;
                for (int i = 0; i < gameTable.animals.Count(); i++)
                {
                    Animal animal = gameTable.animals[i];
                    if (InsideSightCircle(r, animal.position.Item1 - poacher.position.Item1, animal.position.Item2 - poacher.position.Item2))
                    {
                        if (!poacher.stoleOrKilledToday && i != 0 && i % 8 == 0)
                        {
                            poacher.entityToKill = animal;
                            poacher.Attack();
                            poacher.stoleOrKilledToday = true;
                        } else if (!poacher.stoleOrKilledToday && i != 0 && i % 5 == 0) {
                            if(animal.position == poacher.position) {
                                poacher.Steal(animal);
                                poacher.stoleOrKilledToday = true;
                                i--;
                            } else {
                                animal.isCaught = true;
                                poacher.destination = animal.position;
                            }
                        }
                    }
                }
                foreach (Ranger ranger in gameTable.rangers)
                {
                    if (InsideSightCircle(r, ranger.position.Item1 - poacher.position.Item1, ranger.position.Item2 - poacher.position.Item2))
                    {
                        poacherWasSeen = true;
                        if (!poacher.canSee) {
                            poacher.canSee = true;
                            OnCanSeeChanged(poacher);
                        }
                    }
                }
                foreach (Jeep jeep in gameTable.vehicles)
                {
                    if (jeep.passengers > 0 && 
                        InsideSightCircle(r, jeep.position.Item1 - poacher.position.Item1, jeep.position.Item2 - poacher.position.Item2))
                    {
                        poacherWasSeen = true;
                        if (!poacher.canSee) {
                            poacher.canSee = true;
                            OnCanSeeChanged(poacher);
                        }
                    }
                }
                if (!poacherWasSeen) {
                    if(poacher.canSee) {
                        poacher.canSee = false;
                        OnCanSeeChanged(poacher);
                    }
                }
            }
            else if (entity is Ranger ranger)
            {
                if (ranger.isUserDestination) {
                    if (ranger.entityToKill != null && InsideSightCircle(r, ranger.entityToKill.position.Item1 - ranger.position.Item1, ranger.entityToKill.position.Item2 - ranger.position.Item2)) {
                        if (ranger.entityToKill is Animal) {
                            gameTable.money += 200;
                            OnMoneyChanged();
                        }
                        ranger.Attack();
                        ranger.isUserDestination = false;
                    }
                } else {
                    for (int i = 0; i < gameTable.poachers.Count(); i++)
                    {
                        Poacher poacher1 = gameTable.poachers[i];
                        if (InsideSightCircle(r, poacher1.position.Item1 - ranger.position.Item1, poacher1.position.Item2 - ranger.position.Item2))
                        {
                            ranger.entityToKill = poacher1;
                            ranger.Attack();
                            if (!gameTable.poachers.Contains(poacher1)) i--;
                        }
                    }
                }
            }

        }
        [ExcludeFromCodeCoverage]
        public void SaveGame() {
            if (gameTable == null) throw new Exception("Wanted to save null gametable");
            String path = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "";
            if (path == null || path == "") throw new Exception("Failed to determine path");
            dataAccess.Save(gameTable, Path.Combine(path, "SavedGame", "Save.json"));
        }
        [ExcludeFromCodeCoverage]
        public void LoadGame() {
            String path = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "";
            if (path == null || path == "") throw new Exception("Failed to determine path");
            gameTable = dataAccess.Load(Path.Combine(path, "SavedGame", "Save.json"));
            //This is only needed while debugging
            //if (gameTable == null) throw new Exception("Loaded null gametable");
            //if (gameTable.animals == null) throw new Exception("Loaded null animals");
            //if (gameTable.gameBoard == null) throw new Exception("Loaded null GameBoard");
            //if (gameTable.poachers == null) throw new Exception("Loaded null poachers");
            //if (gameTable.rangers == null) throw new Exception("Loaded null rangers");
            //if (gameTable.vehicles == null) throw new Exception("Loaded null vehicles");
            //if (gameTable.startPos == gameTable.endPos) throw new Exception("Loaded false startpos, endpos");
            //if (gameTable.startPos == gameTable.endPos) throw new Exception("Loaded false startpos, endpos");
            //if (gameTable.tileHeight == 0 || gameTable.tileWidth == 0) throw new Exception("Loaded false tile sizes vehicles");
            //Search for the oldest
            foreach (Animal animal in gameTable.animals) {
                animal.theOldest = gameTable.animals.Where(anim => anim.group == animal.group).Aggregate((a, b) => {
                    if (a.age > b.age) {
                        return a;
                    } else {
                        return b;
                    }
                });
            }
            Days = gameTable.days;

            GiveBackEvents(gameTable.animals);
            GiveBackEvents(gameTable.rangers);
            GiveBackEvents(gameTable.poachers);
            GiveBackEvents(gameTable.vehicles);

            OnTilesCreated();
            OnStatsChanged();
            OnDaysChanged();
            OnMoneyChanged();
        }
        #endregion

        #region PrivateFunctions
        private void GenMap()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int hillCnt = 0;
            int lakeCnt = 0;
            int plantCnt = 0;
            Tile[,] table = new Tile[30, 60];
            Random rnd = new Random();
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    (int, int) currentPos = (j * gameTable.tileWidth,
                                             i * gameTable.tileHeight);


                    //alap esetben general mindenbol 4-et
                    int rndNum = rnd.Next(0, 100);
                    if(rndNum > 0 && rndNum < 3 && hillCnt < 15) {
                        table[i, j] = new Hill(currentPos);
                        hillCnt++;
                    } else if(rndNum > 6 && rndNum < 9 && plantCnt < 3) {
                        plantCnt++;
                        table[i, j] = new Bush(currentPos);
                    } else if(rndNum > 10 && rndNum < 12 && plantCnt < 3) {
                        plantCnt ++;
                        table[i, j] = new Grass(currentPos);
                    } else if(rndNum > 12 && rndNum < 14 && plantCnt < 3) {
                        plantCnt ++;
                        table[i, j] = new Tree(currentPos);
                    } else if(rndNum > 14 && rndNum < 16 && lakeCnt < 3) {
                        lakeCnt++;
                        table[i, j] = new Lake(currentPos);
                    } else {
                        table[i, j] = new Empty(currentPos);
                    }
                    
                }
                hillCnt = 0;
                lakeCnt = 0;
                plantCnt = 0;
            }
            gameTable.gameBoard = table;
            foreach (var item in FindPath((rnd.Next(0, 29), rnd.Next(0, 59)), (rnd.Next(0, 29), rnd.Next(0, 59)), "Empty")) {
                (int, int) currentPos = (item.col * gameTable.tileWidth,
                                                item.row * gameTable.tileHeight);
                gameTable.gameBoard[item.row, item.col] = new River(currentPos);
            }
            Animal tih = new Antilope(1, (120, 65), 5, Days, null);
            GiveEntityEvents(tih);
            tih.age = 2;
            gameTable.animals.Add(tih);
            for (int i = 0; i < 10; i++) {
                Animal tihamer = new Antilope(1, (i*45, 45), fov, Days, tih);
                GiveEntityEvents(tihamer);
                tihamer.age = 2;
                gameTable.animals.Add(tihamer);
            }
            Animal tod = new Lion(2, (150, 100), fov, Days, null);
            GiveEntityEvents(tod);
            gameTable.animals.Add(tod);
            for (int i = 0; i < 2; i++) {
                Animal todor = new Lion(2, (i*100, 100), fov, Days, tod);
                GiveEntityEvents(todor);
                gameTable.animals.Add(todor);
            }
            SetNewStartEnd();
            OnTilesCreated();
            OnStatsChanged();
        }
        private void SetNewStartEnd() {
            //Újra felhasználhgatóságnál fontos, hogy nincs eventhez kapcsolva, csak a tilesCreated-hez
            //Ha a genmap-on kívűl váltoik, akkor nem fogja látni azt a view
            Random rnd = new Random();
            int firstRnd = rnd.Next(0, 29);
            //mindig a peremen helyezkedik el, ha i = 0, akkor a felső sorban, 
            //  ha nem, akkor bal oldalon
            if(firstRnd == 0) {
                _startPosition = (firstRnd, rnd.Next(0, 59));
                _endPosition = (29, rnd.Next(0, 59));
            } else {
                _startPosition = (firstRnd, 0);
                _endPosition = (rnd.Next(0, 29), 59);
            }
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            gameTable.gameBoard[_startPosition.Item1, _startPosition.Item2] = new Road(MatrixToCanvas(_startPosition));
            gameTable.gameBoard[_endPosition.Item1, _endPosition.Item2] = new Road(MatrixToCanvas(_endPosition));
            gameTable.startPos = _startPosition;
            gameTable.endPos = _endPosition;
        }
        /*public List<List<(int, int)>> GetAllPaths((int, int) start, (int, int) goal, string type) {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");

            int row = gameTable.gameBoard.GetLength(0), col = gameTable.gameBoard.GetLength(1);
            var allPaths = new List<List<(int, int)>>();
            var visited = new bool[row, col];
            var currentPath = new List<(int, int)>();

            int[][] dirs = [[1, 0], [-1, 0], [0, 1], [0, -1]];

            //Belső függvény. nem vmi szép, majd lehet kiveszem
            void DFS((int, int) current) {
                visited[current.Item1, current.Item2] = true;
                currentPath.Add(current);

                if (current.Equals(goal)) {
                    allPaths.Add([.. currentPath]);
                } else {
                    foreach (var dir in dirs) {
                        int ni = current.Item1 + dir[0], nj = current.Item2 + dir[1];
                        if (!(ni < 0 || ni >= row || nj < 0 || nj >= col ||
                              gameTable.gameBoard[ni, nj].GetType().Name != type ||
                              visited[ni, nj])) {
                            DFS((ni, nj));
                        }
                    }
                }

                currentPath.RemoveAt(currentPath.Count - 1);
                visited[current.Item1, current.Item2] = false;
            }

            DFS(start);

            if (allPaths.Count == 0) {
                if(!_incompleteRoadWasTriggered && type == "Road") {
                    OnIncompleteRoad(true);
                    _incompleteRoadWasTriggered = true;
                }
            } else {
                if(_incompleteRoadWasTriggered && type == "Road") {
                    OnIncompleteRoad(false);
                    _incompleteRoadWasTriggered = false;
                }
            }

                return allPaths;
        }*/
        private List<(int row, int col)> FindPath((int, int) start, (int, int) goal, String type) {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int row = gameTable.gameBoard.GetLength(0), col = gameTable.gameBoard.GetLength(1);
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
                    if (!(ni < 0 || ni >= row || nj < 0 || nj >= col || gameTable.gameBoard[ni, nj].GetType().Name != type)) {
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
        private List<(int, int)> GenJeepPath()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");

            var allPath = ParallelPathFinder.FindAllPaths(gameTable.gameBoard, new Position(_startPosition), new Position(_endPosition));
            int randomIndex = new Random().Next(0, allPath.Count == 0 ? 0 : allPath.Count - 1);
            if(allPath.Count == 0 ) {
                if (!_incompleteRoadWasTriggered) {
                    OnIncompleteRoad(true);
                    _incompleteRoadWasTriggered = true;
                }
                return new List<(int, int)>();
            } else {
                if (_incompleteRoadWasTriggered) {
                    OnIncompleteRoad(false);
                    _incompleteRoadWasTriggered = false;
                }
                return allPath[randomIndex].Select(pos => (pos.Row, pos.Col)).ToList();
            }
        }
        private void CheckGameEnd() {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int animalsCount = _carnivores + _herbivores;
            if (gameTable.difficulty == 0)
            {
                if (animalsCount >= winConAnimalEasy && gameTable.money >= winConMoneyEasy)
                {
                    Win();
                }
            }
            if (gameTable.difficulty == 1)
            {
                if (animalsCount >= winConAnimalMedium && gameTable.money >= winConMoneyMedium)
                {
                    Win();
                }
            }
            if (gameTable.difficulty == 2)
            {
                if (animalsCount >= winConAnimalHard && gameTable.money >= winConMoneyHard)
                {
                    Win();
                }
            }
            if((_carnivores == 0 || _herbivores == 0) && gameTable.money < 50) {
                Lose();
            }
        }
        private void MoveTheMovables() 
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            for (int i = 0; i < gameTable.animals.Count; i++)
            {
                Animal animal = gameTable.animals[i];
                animal.CheckDead();
                if (gameTable.animals.Contains(animal)) InsideSight(animal);
                if (gameTable.animals.Contains(animal))
                {
                    if (animal.isDrinking)
                    {
                        animal.Drink();
                    }
                    else if (animal.isEating)
                    {
                        animal.Eat(Days);
                    }
                    else if (animal.isResting)
                    {
                        animal.Rest(Days);
                    }
                    else if (!animal.isResting)
                    {
                        (int matrixX, int matrixY)  = CanvasToMatrix(animal.position);
                        Tile standingOn = gameTable.gameBoard[matrixX, matrixY];
                        if (standingOn is River || standingOn is Hill || standingOn is Lake)
                        {
                            if (Ticks % 2 == 0)
                            {
                                animal.Move(_canvasWidth, _canvasHeight, gameTable.tileWidth, gameTable.tileHeight, i);
                            }
                        }
                        else
                        {
                            animal.Move(_canvasWidth, _canvasHeight, gameTable.tileWidth, gameTable.tileHeight, i);
                        }
                    }
                    animal.GenerateHungerAndThirst();
                }
                else i--;
            }
            for (int i = 0; i < gameTable.vehicles.Count; i++) {
                Jeep jeep = gameTable.vehicles[i];
                InsideSight(jeep);
                if (gameTable.vehicles.Contains(jeep)) { 
                    jeep.Move(_canvasWidth, _canvasHeight, gameTable.tileWidth, gameTable.tileHeight, i);
                }
            }
            for (int i = 0; i < gameTable.rangers.Count; i++) {
                Ranger ranger = gameTable.rangers[i];
                InsideSight(ranger);
                if (gameTable.rangers.Contains(ranger)) {
                    ranger.Move(_canvasWidth, _canvasHeight, gameTable.tileWidth, gameTable.tileHeight, i);
                }
            }
            for (int i = 0; i < gameTable.poachers.Count; i++) {
                Poacher poacher = gameTable.poachers[i];
                InsideSight(poacher);
                if (gameTable.poachers.Contains(poacher)) {
                    poacher.Move(_canvasWidth, _canvasHeight, gameTable.tileWidth, gameTable.tileHeight, i);
                }
            }
        }
        private void GiveEntityEvents(Entity entity) {
            entity.EntityMoved += OnEntityMoved;
            entity.EntityDied += OnEntityDied;
            entity.EntityMurdered += OnEntityMurdered;
            entity.AnimalStateChanged += OnAnimalStateChanged;
        }
        private void VisitorsToJeeps()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            foreach (var jeep in gameTable.vehicles)
            {
                if (!jeep.onThePath && gameTable.visitors > 0)
                {
                    int szabadHely = 4 - jeep.passengers;
                    int betoltheto = Math.Min(szabadHely, gameTable.visitors);

                    jeep.passengers += betoltheto;
                    gameTable.visitors -= betoltheto;

                    var path = GenJeepPath();

                    if (jeep.passengers == 4 && path.Count != 0)
                    {
                        //Le kellett volna kezelni, hogy mi van akkor, ha nincs kész az útunk
                        jeep.onThePath = true;
                        PayJeep();
                        List<(int, int)> matrixPath = path;
                        List<(int, int)> canvasPath = matrixPath.Select(p => MatrixToCanvas(p)).ToList();
                        jeep.GiveJeepPath(canvasPath);
                    }

                    if (gameTable.visitors == 0)
                        return;
                }
            }
        }
        private void VisitorSpawn()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int startIngVisitort = gameTable.visitors;
            int maxvisitors = (int)(10 + (gameTable.satisfaction / 100.0) * (50 - 10));
            // max vistorszám 10 - 50 ig egyenesen arányosan
            if (maxvisitors > gameTable.visitors)
            { 
                // 1-4 ig érkezenek elégedettség alapján
                gameTable.visitors += (int)(1 + (gameTable.satisfaction / 100.0) * (4 - 1));
            }
            if(startIngVisitort != gameTable.visitors) {
                OnStatsChanged();
            }
        }
        private void PoacherSpawn()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int neededPoachers = gameTable.animals.Count / 10;
            if (neededPoachers > 0 && neededPoachers > gameTable.poachers.Count )
            {
                for (int i = gameTable.poachers.Count; i < neededPoachers; i++)
                {
                    Poacher poacher = new Poacher(fov);
                    gameTable.poachers.Add(poacher);
                    GiveEntityEvents(poacher);
                    OnEntityPlaced(poacher);
                }
            }
        }
        private void PayRangers()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int osszBer = gameTable.rangers.Count * rangerSalary;
            if (osszBer <= gameTable.money)
            {
                gameTable.money = gameTable.money - osszBer;
                OnMoneyChanged();
            }
            else //ha elfogy a pénze, akkor azok a vadőrök akik nem kaptak meghalnak
            {
                int maradDB = (int)(gameTable.money / rangerSalary); //mennyi őr marad
                List<Ranger> akikMaradnak = gameTable.rangers.GetRange(0, maradDB);

                gameTable.money = gameTable.money - (maradDB * rangerSalary);
                OnMoneyChanged();

                List<Ranger> akikMennek = gameTable.rangers.GetRange(maradDB, gameTable.rangers.Count - maradDB);
                foreach (Ranger act in akikMennek)
                {
                    if (act != null)
                    {
                        act.Leave();
                    }
                }
                gameTable.rangers = akikMaradnak;
            }
        }
        private void PayJeep()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            // ha a elégedettség 0% -> jegyár, ha 50 akkor másfélszerese, 100 -> 2 szerese
            gameTable.money = (int)(gameTable.money + 4 * ticketPrice * ((gameTable.satisfaction / 100.0) + 1));
            OnMoneyChanged();
        }
        private void Win()
        {
            OnGameFinished("Szép volt tesó!");
        }
        private void Lose()
        {
            OnGameFinished("Hát Bástya, nem vagyok rád büszke");
        }
        private Animal? SearchTheOldest((int x, int y) pos, string type)
        {
            //return gameTable?.animals.First(animal => 
            //    animal.GetType().Name == type && animal.theOldest == animal &&
            //    InsideSightCircle(fov * (gameTable.tileHeight + gameTable.tileWidth) / 2,
            //    pos.x - animal.position.Item1, pos.y - animal.position.Item2));
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int index = 0;
            while (index < gameTable.animals.Count())
            {
                Animal animal = gameTable.animals[index];
                if (animal.GetType().Name == type && animal.theOldest == animal &&
                    InsideSightCircle(fov * (gameTable.tileHeight + gameTable.tileWidth) / 2,
                    pos.x - animal.position.Item1, pos.y - animal.position.Item2))
                {
                    return animal;
                }
                index++;
            }
            return null;
        }
        private void SetOldestInGroup(Animal oldest)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            foreach (Animal animal in gameTable.animals)
            {
                if (animal.group == oldest.group)
                {
                    animal.theOldest = oldest;
                    animal.transitionVector = (animal.position.Item1 - oldest.position.Item1,
                                                animal.position.Item2 - oldest.position.Item2);
                }
            }
        }
        private void TheGroupNeedsSomething()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            //Valamiért az alap beállítás nem volt jó, ez így biztosabb
            var animalsInGroups = gameTable.animals.GroupBy(animal => animal.group);
            _animalGroupCount = animalsInGroups.Count();
            foreach (var animals in animalsInGroups) {
                Animal? oldest = animals.First().theOldest;
                if (oldest is null) throw new Exception("Oldest was null");
                bool needWater = false;
                bool needFood = false;
                foreach (Animal animal in animals) {
                    animal.NeedsSomething();
                    if(animal.isHungry) {
                        needFood = true;
                    } else if(animal.isThirsty) {
                        needWater = true;
                    }
                }
                if (needWater) {
                    oldest.someoneNeedsSomething = 1;
                    if(oldest?.waterSource?.Count == 0 && animals.Any(animal => animal?.waterSource?.Count > 0)) {
                        oldest.waterSource = animals.Select(animal => animal.waterSource).Aggregate((acc, waterSource) => {
                            if(waterSource is not null && waterSource.Count > 0) {
                                acc?.AddRange(waterSource);
                            }
                            return acc;
                        });
                    }
                }
                else if (needFood) { 
                    oldest.someoneNeedsSomething = 2;
                    if (oldest?.foodSource?.Count == 0 && animals.Any(animal => animal?.foodSource?.Count > 0)) {
                        oldest.foodSource = animals.Select(animal => animal.foodSource).Aggregate((acc, foodsource) => {
                            if (foodsource is not null && foodsource.Count > 0) {
                                acc?.AddRange(foodsource);
                            }
                            return acc;
                        });
                    }
                }
                else oldest.someoneNeedsSomething = 0;
            }
        }
        private bool InsideSightCircle(int r, int x, int y)
        {
            return Math.Pow(r, 2) >= Math.Pow(x, 2) + Math.Pow(y, 2);
        }
        private (int, int) MatrixToCanvas((int, int) matrixTile)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            return gameTable.gameBoard[matrixTile.Item1, matrixTile.Item2].position;
        }
        private (int, int) CanvasToMatrix((int, int) canvasPoint)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int row = (int) Math.Round(canvasPoint.Item1*1.0 / gameTable.tileWidth);
            int col = (int) Math.Round(canvasPoint.Item2*1.0 / gameTable.tileHeight);
            return (col, row);      
        }
        private int GetEntityIndex(Entity entity) {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            if (entity is Animal animal) {
                return gameTable.animals.IndexOf(animal);
            }
            if(entity is Poacher poacher) {
                return gameTable.poachers.IndexOf(poacher);
            }
            if(entity is Ranger ranger) {
                return gameTable.rangers.IndexOf(ranger);
            }
            if (entity is Jeep jeep) {
                return gameTable.vehicles.IndexOf(jeep);
            } 
            else return 0;
        }
        private void SatisfactionCheck()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int satisfaction = 0;
            foreach (Animal animal in gameTable.animals)
            {
                if(animal is Lion || animal is Elephant)
                {
                    satisfaction += 2;
                }
                else
                {
                    satisfaction += 1;

                }
            }
            satisfaction -= gameTable.poachers.Count * 10;
            int roadCnt = 0;
            for (int i = 0; i < gameTable.gameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < gameTable.gameBoard.GetLength(1); j++)
                {
                    if (gameTable.gameBoard[i,j] is Road)
                    {
                        roadCnt++;
                    }
                }
            }
            satisfaction += roadCnt / 5;
            if(satisfaction > 100)
            {
                satisfaction = 100;
            } else if (satisfaction < 0)
            {
                satisfaction = 0;
            }
            gameTable.satisfaction = satisfaction;
        }
        private void AnimalReproduction()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");

            var groupedAnimals = gameTable.animals.GroupBy(animal => animal.group).ToList();
            foreach (var animals in groupedAnimals)
            {
                if (animals.Where(animal => animal.age >= 2).Count() > 1)
                {
                    Animal firstAnimal = animals.First();
                    (int x, int y) = firstAnimal.transitionVector;
                    (int posX, int posY) = firstAnimal.position;
                    switch (firstAnimal.GetType().Name)
                    {
                        case "Antilope":
                            Animal antilope = new Antilope(firstAnimal.group, firstAnimal.position, firstAnimal.fov,
                                Days, firstAnimal.theOldest);
                            antilope.transitionVector = (x - Math.Sign(x) * 5, y - Math.Sign(y) * 5);
                            GiveEntityEvents(antilope);
                            gameTable.animals.Add(antilope);
                            OnStatsChanged();
                            antilope.position = (posX + Math.Sign(posX) * 5, posY + Math.Sign(posY) * 5);
                            OnEntityPlaced(antilope);
                            break;
                        case "Elephant":
                            Animal elephant = new Elephant(firstAnimal.group, firstAnimal.position, firstAnimal.fov,
                                Days, firstAnimal.theOldest);
                            elephant.transitionVector = (x - Math.Sign(x) * 5, y - Math.Sign(y) * 5);
                            GiveEntityEvents(elephant);
                            gameTable.animals.Add(elephant);
                            OnStatsChanged();
                            elephant.position = (posX + Math.Sign(posX) * 5, posY + Math.Sign(posY) * 5);
                            OnEntityPlaced(elephant);
                            break;
                        case "Hyena":
                            Animal hyena = new Hyena(firstAnimal.group, firstAnimal.position, firstAnimal.fov,
                                Days, firstAnimal.theOldest);
                            hyena.transitionVector = (x - Math.Sign(x) * 5, y - Math.Sign(y) * 5);
                            GiveEntityEvents(hyena);
                            gameTable.animals.Add(hyena);
                            OnStatsChanged();
                            hyena.position = (posX + Math.Sign(posX) * 5, posY + Math.Sign(posY) * 5);
                            OnEntityPlaced(hyena);
                            break;
                        case "Lion":
                            Animal lion = new Lion(firstAnimal.group, firstAnimal.position, firstAnimal.fov,
                                Days, firstAnimal.theOldest);
                            lion.transitionVector = (x - Math.Sign(x) * 5, y - Math.Sign(y) * 5);
                            GiveEntityEvents(lion);
                            gameTable.animals.Add(lion);
                            OnStatsChanged();
                            lion.position = (posX + Math.Sign(posX) * 5, posY + Math.Sign(posY) * 5);
                            OnEntityPlaced(lion);
                            break;
                    }
                }
            }
        }
        private void GiveBackEvents<T>(List<T> entities) where T : Entity {
            foreach (Entity entity in entities) {
                GiveEntityEvents(entity);
            }
        }
        private void SearchForAnimalResource(Animal animal, int radius, String type) {
            var animalPos = CanvasToMatrix(animal.position);

            (int row, int col) radiusInTiles = animalPos;
            Tile tile = gameTable?.gameBoard[animalPos.Item1, animalPos.Item2] ?? new Empty((0, 0));
            while(radiusInTiles.row < gameTable?.gameBoard.GetLength(0) && radiusInTiles.row >= 0
                && InsideSightCircle(radius, tile.position.Item1 - animal.position.Item1, tile.position.Item2 - animal.position.Item2)) {
                tile = gameTable?.gameBoard[radiusInTiles.row, animalPos.Item2] ?? new Empty((0, 0));
                radiusInTiles.row++;
            }
            tile = gameTable?.gameBoard[animalPos.Item1, radiusInTiles.col] ?? new Empty((0, 0));
            while(radiusInTiles.col < gameTable?.gameBoard.GetLength(1) && radiusInTiles.col >= 0
                && InsideSightCircle(radius, tile.position.Item1 - animal.position.Item1, tile.position.Item2 - animal.position.Item2)) {
                tile = gameTable?.gameBoard[animalPos.Item1, radiusInTiles.col] ?? new Empty((0, 0));
                radiusInTiles.col++;
            }

            radiusInTiles = (Math.Abs(animalPos.Item1 - radiusInTiles.row), Math.Abs(animalPos.Item2 - radiusInTiles.col));
            int minI = Math.Max(0, animalPos.Item1 - radiusInTiles.row);
            int maxI = Math.Min(gameTable?.gameBoard.GetLength(0) ?? 0, animalPos.Item1 + radiusInTiles.row);
            int minJ = Math.Max(0, animalPos.Item2 - radiusInTiles.col);
            int maxJ = Math.Min(gameTable?.gameBoard.GetLength(1) ?? 0, animalPos.Item2 + radiusInTiles.col);
            for (int i = minI; i < maxI; i++) {
                for (int j = minJ; j < maxJ; j++) {
                    Tile actTile = gameTable?.gameBoard[i, j] ?? new Empty((0, 0));
                    if (type == "Food") {
                        if(actTile is Plant && (!animal.foodSource?.Contains(actTile.position) ?? true)) {
                            animal.foodSource?.Add(actTile.position);
                        }
                    } else if(type == "Water") {
                        if (actTile is Water && (!animal.waterSource?.Contains(actTile.position) ?? true)) {
                            animal.waterSource?.Add(actTile.position);
                        }
                    }
                }
            }
        }
        #endregion

        #region EventHandlerFunctions
        private void OnStatsChanged() {
            StatsChanged?.Invoke(this, new StatEventArgs {
                Carnivores = _carnivores,
                Herbivores = _herbivores,
                Visitors = gameTable?.visitors ?? 0
            });
        }
        private void OnDayNightChanged() {
            DayNightChanged?.Invoke(this, _isDay);
        }
        private void OnMoneyChanged()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            MoneyChanged?.Invoke(this, gameTable.money);
        }
        private void OnEntityMoved(object? sender, EntityMovedEventArgs e)
        {
            EntityMoved?.Invoke(this, e);
        }
        private void OnEntityPlaced(Entity entity)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            int entityIndex = GetEntityIndex(entity);
            if (entity is Animal) entityIndex = gameTable.animals.Count(animal => animal.Type == entity.GetType().Name) - 1;
            EntityPlaced?.Invoke(this, new PlacedEventargs
            {
                X = entity.position.Item1,
                Y = entity.position.Item2,
                IsAnimal = entity is Animal,
                Index = entityIndex,
                Cansee = entity.canSee,
                Type = entity.GetType().Name,
                IsCarnivore = entity is Carnivore,
                IsHerbivore = entity is Herbivore
            });
        }
        private void OnEntityDied(object? sender, PlacedEventargs e)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            if (sender is Animal animal)
            {
                e.Index = GetEntityIndex(animal);
                gameTable.animals.Remove(animal);
                OnStatsChanged();
                if (animal.theOldest == animal)
                {
                    if(gameTable.animals.Any(anim => anim.group == animal.group)) {
                        Animal? oldest = gameTable.animals.Where(anim => anim.group == animal.group).Aggregate((a, b) => {
                            if (a.age > b.age) {
                                return a;
                            } else {
                                return b;
                            }
                        });
                        if(oldest is not null) {
                            SetOldestInGroup(oldest);
                        }
                    }
                }
                EntityDied?.Invoke(this, e);
            }
            else if (sender is Poacher poacher)
            {
                e.Index = GetEntityIndex(poacher);
                gameTable.poachers.Remove(poacher);
                foreach (Animal stolen in poacher.stolenAnimals)
                {
                    stolen.position = (e.X, e.Y);
                    stolen.theOldest = gameTable.animals.Find(anim => anim.theOldest == stolen.theOldest)?.theOldest ?? stolen;
                    stolen.destination = stolen.position;
                    gameTable.animals.Add(stolen);
                    OnStatsChanged();
                    //végig ez volt a hiba
                    //GiveEntityEvents(stolen);
                    OnEntityPlaced(stolen);
                }
                EntityDied?.Invoke(this, e);
            }
            else if (sender is Ranger ranger)
            {
                e.Index = GetEntityIndex(ranger);
                gameTable.rangers.Remove(ranger);
                EntityDied?.Invoke(this, e);
            }
            else if (sender is Jeep jeep)
            {
                e.Index = GetEntityIndex(jeep);
                gameTable.vehicles.Remove(jeep);
                EntityDied?.Invoke(this, e);
            }
        }
        private void OnAnimalStateChanged(object? sender, AnimalStateEventargs e)
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            if (sender is Animal animal)
            {
                e.Index = GetEntityIndex(animal);
                AnimalStateChanged?.Invoke(this, e);
            }
        }
        private void OnTilePlaced(String type, int x, int y)
        {
            TilePlaced?.Invoke(this, new PlacedEventargs
            {
                Type = type,
                X = x,
                Y = y
            });
        }
        private void OnGameFinished(string winner)
        {
            GameFinished?.Invoke(this, winner);
        }
        private void OnIncompleteRoad(bool isThereIncompleteRoad)
        {
            IncompleteRoad?.Invoke(this, isThereIncompleteRoad);
        }
        private void OnTilesCreated()
        {
            if (gameTable == null) throw new NullReferenceException("gameTable was null");
            List<Entity> entities = new List<Entity>();
            entities.AddRange(gameTable.animals);
            entities.AddRange(gameTable.vehicles);
            entities.AddRange(gameTable.poachers);
            entities.AddRange(gameTable.rangers);
            TilesCreated?.Invoke(this, new TilesCreatedEventArgs {
                CanvasHeight = _canvasHeight,
                CanvasWidth = _canvasWidth,
                Tiles = gameTable.gameBoard,
                Enitites = entities,
                StartPosition = MatrixToCanvas(_startPosition),
                EndPosition = MatrixToCanvas(_endPosition)
            });
        }
        private void OnNotEnoughMoney()
        {
            NotEnoughMoney?.Invoke(this, EventArgs.Empty);
        }
        private void OnDaysChanged() {
            DaysChanged?.Invoke(this, Days);
        }
        private void OnCanSeeChanged(Entity entity) {
            CanSeeChanged?.Invoke(this, new PlacedEventargs {
                X = entity.position.Item1,
                Y = entity.position.Item2,
                IsAnimal = entity is Animal,
                Index = GetEntityIndex(entity),
                Cansee = entity.canSee,
                Type = entity.GetType().Name,
                IsCarnivore = entity is Carnivore,
                IsHerbivore = entity is Herbivore
            });
        }
        private void OnEntityMurdered(object? sender, EventArgs e) {
            if(sender is Carnivore carnivore) {
                if (gameTable == null) throw new NullReferenceException("gameTable was null");
                if(gameTable.animals.Any(animal => animal.isCaught)) {
                    (int minx, int miny) = gameTable.animals.Where(animal => animal is Herbivore && animal.isCaught)
                        .Select(herbivore => (
                            Math.Abs(herbivore.position.Item1 - carnivore.position.Item1),
                            Math.Abs(herbivore.position.Item2 - carnivore.position.Item2)))
                        .Aggregate((a, b) => {
                            if (a.Item1 < b.Item1 && a.Item2 < b.Item2) {
                                return a;
                            } else {
                                return b;
                            }
                        });
                    foreach (var animal in gameTable.animals) {
                        if(animal.isCaught &&
                           Math.Abs(animal.position.Item1 - carnivore.position.Item1) == minx &&
                           Math.Abs(animal.position.Item2 - carnivore.position.Item2) == miny) {
                            animal.isDead = true;
                            animal.isCaught = false;
                            return;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
