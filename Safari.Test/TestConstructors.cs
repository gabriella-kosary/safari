using Microsoft.VisualStudio.TestTools.UnitTesting;
using Safari.Model;
using Safari.Persistence.DataAccess;
using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;

namespace Safari.Test {
    [TestClass]
    public sealed class TestConstructors {
        private Game? _game = null;

        [TestInitialize]
        public void InitGameMechanicsTest() {
            _game = new Game();
        }

        [TestMethod]
        public void TestMethod1() {
            Assert.IsTrue(true);
            Assert.IsTrue(true);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestGameConstructor() {
            Assert.IsNotNull(_game);
            Assert.IsNull(_game.gameTable);
        }

        [TestMethod]
        public void TestAnimalsConstructor() {
            Assert.IsNotNull(_game);

            Antilope antika = new Antilope(0, (0, 0), 5, 200, null);
            Antilope antikaFia = new Antilope(0, (0, 0), 5, 200, antika);
            Assert.IsNotNull(antika);
            Assert.IsNotNull(antika.foodSource);
            Assert.IsNotNull(antika.waterSource);
            Assert.IsNotNull(antika.obstacles);
            Assert.AreEqual(antika.group, 0);
            Assert.AreEqual(antika.position, (0,0));
            Assert.AreEqual(antika.fov, 5);
            Assert.AreEqual(antika.birthDay, 200);
            Assert.AreEqual(antika.age, 0);
            Assert.AreEqual(antika.hunger, 10000);
            Assert.AreEqual(antika.thirst, 10000);
            Assert.AreEqual(antika.destination, antika.position);
            Assert.IsFalse(antika.isDead);
            Assert.IsFalse(antika.chipped);
            Assert.IsFalse(antika.isDrinking);
            Assert.IsFalse(antika.isEating);
            Assert.IsFalse(antika.isResting);
            Assert.AreEqual(antikaFia.group, 0);
            Assert.AreEqual(antika.theOldest, antika);
            Assert.AreEqual(antikaFia.theOldest, antika);

            Elephant todor = new Elephant(1, (1, 1), 9, 600, null);
            Elephant todorFia = new Elephant(1, (1, 1), 9, 600, todor);
            Assert.IsNotNull(todor);
            Assert.IsNotNull(todor.foodSource);
            Assert.IsNotNull(todor.waterSource);
            Assert.IsNotNull(todor.obstacles);
            Assert.AreEqual(todor.group, 1);
            Assert.AreEqual(todor.position, (1,1));
            Assert.AreEqual(todor.fov, 9);
            Assert.AreEqual(todor.birthDay, 600);
            Assert.AreEqual(todor.age, 0);
            Assert.AreEqual(todor.hunger, 10000);
            Assert.AreEqual(todor.thirst, 10000);
            Assert.AreEqual(todor.destination, todor.position);
            Assert.IsFalse(todor.isDead);
            Assert.IsFalse(todor.chipped);
            Assert.IsFalse(todor.isDrinking);
            Assert.IsFalse(todor.isEating);
            Assert.IsFalse(todor.isResting);
            Assert.AreEqual(todorFia.group, 1);
            Assert.AreEqual(todor.theOldest, todor);
            Assert.AreEqual(todorFia.theOldest, todor);

            Lion tihamer = new Lion(6, (6, 6), 9, 900, null);
            Lion tihamerFia = new Lion(6, (6, 6), 9, 900, tihamer);
            Assert.IsNotNull(tihamer);
            Assert.IsNotNull(tihamer.foodSource);
            Assert.IsNotNull(tihamer.waterSource);
            Assert.IsNotNull(tihamer.obstacles);
            Assert.AreEqual(tihamer.group, 6);
            Assert.AreEqual(tihamer.position, (6,6));
            Assert.AreEqual(tihamer.fov, 9);
            Assert.AreEqual(tihamer.birthDay, 900);
            Assert.AreEqual(tihamer.age, 0);
            Assert.AreEqual(tihamer.hunger, 10000);
            Assert.AreEqual(tihamer.thirst, 10000);
            Assert.AreEqual(tihamer.destination, tihamer.position);
            Assert.IsFalse(tihamer.isDead);
            Assert.IsFalse(tihamer.chipped);
            Assert.IsFalse(tihamer.isDrinking);
            Assert.IsFalse(tihamer.isEating);
            Assert.IsFalse(tihamer.isResting);
            Assert.AreEqual(tihamerFia.group, 6);
            Assert.AreEqual(tihamer.theOldest, tihamer);
            Assert.AreEqual(tihamerFia.theOldest, tihamer);

            Hyena kazmer = new Hyena(4, (4, 4), 2, 600000000, null);
            Hyena kazmerFia = new Hyena(4, (4, 4), 2, 600000000, kazmer);
            Assert.IsNotNull(kazmer);
            Assert.IsNotNull(kazmer.foodSource);
            Assert.IsNotNull(kazmer.waterSource);
            Assert.IsNotNull(kazmer.obstacles);
            Assert.AreEqual(kazmer.group, 4);
            Assert.AreEqual(kazmer.position, (4,4));
            Assert.AreEqual(kazmer.fov, 2);
            Assert.AreEqual(kazmer.birthDay, 600000000);
            Assert.AreEqual(kazmer.age, 0);
            Assert.AreEqual(kazmer.hunger, 10000);
            Assert.AreEqual(kazmer.thirst, 10000);
            Assert.AreEqual(kazmer.destination, kazmer.position);
            Assert.IsFalse(kazmer.isDead);
            Assert.IsFalse(kazmer.chipped);
            Assert.IsFalse(kazmer.isDrinking);
            Assert.IsFalse(kazmer.isEating);
            Assert.IsFalse(kazmer.isResting);
            Assert.AreEqual(kazmerFia.group, 4);
            Assert.AreEqual(kazmer.theOldest, kazmer);
            Assert.AreEqual(kazmerFia.theOldest, kazmer);
        }

        [TestMethod]
        public void TestHuntersConstructor() {
            Poacher rezso = new Poacher(5);

            Assert.IsFalse(rezso.canSee);
            Assert.AreEqual(rezso.position, (0, 0));
            Assert.AreEqual(rezso.fov, 5);
            Assert.IsNull(rezso.entityToKill);
            Assert.AreEqual(rezso.destination, rezso.position);
            Assert.IsFalse(rezso.stoleOrKilledToday);

            Ranger elemer = new Ranger((150, 200), 150);
            Assert.IsTrue(elemer.canSee);
            Assert.AreEqual(elemer.position, (150, 200));
            Assert.AreEqual(elemer.fov, 150);
            Assert.IsNull(elemer.entityToKill);
            Assert.AreEqual(elemer.destination, elemer.position);
        }

        [DataTestMethod]
        [DataRow(typeof(Hill), 0, 0)]
        [DataRow(typeof(Bush), 1, 1)]
        [DataRow(typeof(Empty), 2, 2)]
        [DataRow(typeof(Grass), 3, 3)]
        [DataRow(typeof(Lake), 4, 4)]
        [DataRow(typeof(River), 5, 5)]
        [DataRow(typeof(Road), 6, 6)]
        [DataRow(typeof(Tree), 7, 7)]
        public void TestTilesConstructors(Type tileType, int x, int y) {
            if(Activator.CreateInstance(tileType, (x, y)) is Tile tile) {
                Assert.IsNotNull(tile);
                Assert.IsTrue(tile.canSee);
                Assert.AreEqual((x, y), tile.position);
            }
        }

        [DataTestMethod]
        [DataRow(0, 0, 5)]
        [DataRow(10, -3, 3)]
        [DataRow(-1, -1, 1)]
        public void TestJeepConstructor(int x, int y, int fov) {
            var position = (x, y);
            Jeep jeep = new Jeep(position, fov);

            Assert.IsNotNull(jeep);
            Assert.IsTrue(jeep.canSee);
            Assert.AreEqual(position, jeep.position);
            Assert.AreEqual(fov, jeep.fov);
            Assert.AreEqual(0, jeep.passengers);
            Assert.AreEqual(-1, jeep.pathIndex);
            Assert.IsNotNull(jeep.path);
            Assert.AreEqual(0, jeep.path.Count);
            Assert.IsFalse(jeep.onThePath);
        }
    }
}
