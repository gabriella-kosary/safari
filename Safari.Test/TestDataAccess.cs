using Safari.Model;
using Safari.Persistence;
using Safari.Persistence.DataAccess;
using Safari.Persistence.Entities;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Safari.Test;

[TestClass]
public class TestDataAccess
{
    DataAccess? dataAcess;
    Game? game;

    [TestInitialize]
    public void InitDataAccess() {
        dataAcess = new DataAccess();
        game = new Game();
        game.NewGame(1);
        //DataAccess_SaveGame();
    }

    //[TestCleanup]
    //public void Cleanup() {
    //    string filePath = "./Assets/SavedGame.json";
    //    if (File.Exists(filePath)) {
    //        try {
    //            FileHelper.WaitForFileRelease(filePath);
    //            File.Delete(filePath);
    //        } catch (IOException ex) {
    //            Console.WriteLine($"Cleanup failed: {ex.Message}");
    //        }
    //    }
    //}

    [TestMethod]
    public void DataAccess_SaveGame_CustomPath()
    {
        //const string filePath = "./Assets/SavedGame.json";
        //
        //FileHelper.WaitForFileRelease(filePath);
        //
        //FileHelper.WaitForFileRelease(filePath);

        //Még nem jók az (int, int) tupple-ök miatt
        Assert.IsNotNull(dataAcess);
        Assert.IsNotNull(game);

        Assert.IsNotNull(game.gameTable);
        game.gameTable.animals[0].foodSource?.Add((0,0));

        dataAcess.Save(game.gameTable, "SaveGameTester.json");
        Assert.IsTrue(File.Exists("SaveGameTester.json"));
    }
    
    [TestMethod]
    public void DataAccess_Actual_LoadGameTest()
    {
        //const string filePath = "./Assets/SavedGame.json";
        //
        //FileHelper.WaitForFileRelease(filePath);
        Assert.IsNotNull(dataAcess);
        Assert.IsNotNull(game);
        Assert.IsNotNull(game.gameTable);

        dataAcess.Save(game.gameTable, "ActualTest.json");
        //Még nem jók az (int, int) tupple-ök miatt
        Assert.IsNotNull(dataAcess);

        GameTable? loadedGameTable = null;
        Thread.Sleep(100);
        loadedGameTable = dataAcess.Load("ActualTest.json");

        Assert.IsNotNull(loadedGameTable);
        Assert.IsNotNull(loadedGameTable.animals);
        Assert.IsNotNull(loadedGameTable.vehicles);
        Assert.IsNotNull(loadedGameTable.rangers);
        Assert.IsNotNull(loadedGameTable.poachers);
    }

    [TestMethod]
    public void DataAccess_Simple_LoadGameTest() {
        //const string filePath = "./Assets/SavedGame.json";
        //
        //FileHelper.WaitForFileRelease(filePath);

        Assert.IsNotNull(dataAcess);
        
        var simpleTable = new GameTable();
        simpleTable.animals.Add(new Antilope());

        dataAcess.Save(simpleTable, "SimpleSave.json");

        var loaded = dataAcess.Load("SimpleSave.json");
        Assert.IsNotNull(loaded);
    }

    [TestMethod]
    public void DataAccess_LoadGameTest_ThrowsException()
    {
        Assert.IsNotNull(dataAcess);

        GameTable? loadedGameTable = null;
        Assert.ThrowsException<Exception>(() => loadedGameTable = dataAcess.Load("ExceptionTest.json"));
    }
}
