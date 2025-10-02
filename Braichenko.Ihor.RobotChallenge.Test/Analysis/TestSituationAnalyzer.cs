using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallenge.Test.Analysis
{
    [TestClass]
    public class TestSituationAnalyzer
    {
        private SituationAnalyzer _analyzer;
        private Robot.Common.Robot _myRobot;
        private List<Robot.Common.Robot> _allRobots;
        private Map _map;

        [TestInitialize]
        public void Setup()
        {
            // Set up default data for tests
            _analyzer = new SituationAnalyzer();
            _myRobot = new Robot.Common.Robot() { Energy = 300, Position = new Position(50, 50), OwnerName = "Braichenko Ihor" };
            _allRobots = new List<Robot.Common.Robot>() { _myRobot };
            _map = new Map();
        }

        #region CanCreateRobot Tests (based on game phases)

        [DataTestMethod]
        [DataRow(10, 270, false, "Early game: Not enough energy")]
        [DataRow(10, 271, true, "Early game: Exactly enough energy")]
        [DataRow(26, 300, false, "Mid game: Threshold increased, not enough")]
        [DataRow(26, 301, true, "Mid game: Exactly enough energy")]
        [DataRow(41, 600, false, "Late game: Threshold increased, not enough")]
        [DataRow(41, 601, true, "Late game: Exactly enough energy")]
        [DataRow(48, 10000, false, "End game: Creation is forbidden regardless of energy")]
        public void Analyze_CanCreateRobot_ShouldVaryByGamePhaseAndEnergy(int round, int energy, bool expected, string message)
        {
            _myRobot.Energy = energy;
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, round);
            Assert.AreEqual(expected, result.CanCreateRobot, message);
        }

        #endregion

        #region NearestFreeStation Tests

        [TestMethod]
        public void Analyze_NearestFreeStation_ShouldFindClosestUnoccupiedStation()
        {
            var stationA = new EnergyStation() { Position = new Position(55, 55) }; // Distance ~50
            var stationB = new EnergyStation() { Position = new Position(60, 60) }; // Distance ~200
            _map.Stations = new List<EnergyStation>() { stationA, stationB };

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsNotNull(result.OptimalStation);
            Assert.AreEqual(stationA.Position, result.OptimalStation.Position, "The closest station should be selected.");
        }

        [TestMethod]
        public void Analyze_NearestFreeStation_ShouldIgnoreOccupiedStations()
        {
            var occupiedStation = new EnergyStation() { Position = new Position(51, 51) }; // Nearest
            var freeStation = new EnergyStation() { Position = new Position(40, 40) };     // Next, but free
            var enemyRobot = new Robot.Common.Robot() { Position = occupiedStation.Position, OwnerName = "Enemy" };
            _allRobots.Add(enemyRobot);
            _map.Stations = new List<EnergyStation>() { occupiedStation, freeStation };

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsNotNull(result.OptimalStation);
            Assert.AreEqual(freeStation.Position, result.OptimalStation.Position, "Should ignore stations occupied by other robots.");
        }

        [TestMethod]
        public void Analyze_NearestFreeStation_ShouldReturnNullIfAllStationsAreOccupied()
        {
            var occupiedStation = new EnergyStation() { Position = new Position(51, 51) };
            var enemyRobot = new Robot.Common.Robot() { Position = occupiedStation.Position, OwnerName = "Enemy" };
            _allRobots.Add(enemyRobot);
            _map.Stations = new List<EnergyStation>() { occupiedStation };

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsNull(result.OptimalStation, "Should return null if there are no free stations.");
        }

        #endregion

        #region CanCollectEnergy Tests

        [TestMethod]
        public void Analyze_CanCollectEnergy_ShouldBeTrueWhenStationIs1CellAway()
        {
            _map.Stations = new List<EnergyStation>() { new EnergyStation() { Position = new Position(51, 51) } };

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsTrue(result.CanCollectEnergy, "Collection is possible if the station is 1 cell away.");
        }

        [TestMethod]
        public void Analyze_CanCollectEnergy_ShouldBeTrueWhenOnTheStation()
        {
            _map.Stations = new List<EnergyStation>() { new EnergyStation() { Position = _myRobot.Position } };

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsTrue(result.CanCollectEnergy, "Collection is possible if the robot is standing directly on the station.");
        }


        [TestMethod]
        public void Analyze_CanCollectEnergy_ShouldBeFalseWhenStationIsTooFar()
        {
            _map.Stations = new List<EnergyStation>() { new EnergyStation() { Position = new Position(52, 52) } };

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsFalse(result.CanCollectEnergy, "Collection is not possible if the distance to the station is greater than 1 cell.");
        }

        #endregion

        #region ProfitableEnemyToAttack Tests

        // According to option 5: (EnemyEnergy * 0.05) > 50 (AttackEnergyCost)
        // So, EnemyEnergy > 1000 for profitability
        [TestMethod]
        public void Analyze_ProfitableEnemyToAttack_ShouldReturnNullForLowEnergyEnemy()
        {
            var enemyRobot = new Robot.Common.Robot() { Position = new Position(51, 51), Energy = 1000, OwnerName = "Enemy" };
            _allRobots.Add(enemyRobot);

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsNull(result.ProfitableEnemyToAttack, "Attacking an enemy with 1000 energy is break-even, not profitable.");
        }

        [TestMethod]
        public void Analyze_ProfitableEnemyToAttack_ShouldFindEnemyWithSufficientEnergy()
        {
            var enemyRobot = new Robot.Common.Robot() { Position = new Position(51, 51), Energy = 1001, OwnerName = "Enemy" };
            _allRobots.Add(enemyRobot);

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsNotNull(result.ProfitableEnemyToAttack);
            Assert.AreEqual(enemyRobot, result.ProfitableEnemyToAttack, "Should find a profitable enemy.");
        }

        [TestMethod]
        public void Analyze_ProfitableEnemyToAttack_ShouldChooseClosestProfitableEnemy()
        {
            var enemyClose = new Robot.Common.Robot() { Position = new Position(52, 52), Energy = 1100, OwnerName = "EnemyClose" };
            var enemyFar = new Robot.Common.Robot() { Position = new Position(60, 60), Energy = 2000, OwnerName = "EnemyFar" };
            _allRobots.Add(enemyClose);
            _allRobots.Add(enemyFar);

            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            Assert.IsNotNull(result.ProfitableEnemyToAttack);
            Assert.AreEqual(enemyClose, result.ProfitableEnemyToAttack, "Should choose the closest profitable enemy.");
        }

        #endregion

        #region Friendly Fire Prevention Tests

        
        [TestMethod]
        public void Analyze_NearestFreeStation_ShouldIgnoreStationsOccupiedByFriendlyRobots()
        {
            // Arrange
            var friendlyRobot = new Robot.Common.Robot() { OwnerName = _myRobot.OwnerName, Position = new Position(51, 51) };
            _allRobots.Add(friendlyRobot);

            var friendlyOccupiedStation = new EnergyStation() { Position = friendlyRobot.Position }; // Nearest station
            var trulyFreeStation = new EnergyStation() { Position = new Position(40, 40) };     // Farther, but free

            _map.Stations = new List<EnergyStation>() { friendlyOccupiedStation, trulyFreeStation };

            // Act
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            // Assert
            Assert.IsNotNull(result.OptimalStation, "A free station should be found.");
            Assert.AreEqual(trulyFreeStation.Position, result.OptimalStation.Position, "The analyzer should ignore stations occupied by its own robots and select the next closest free one.");
        }

        
        [TestMethod]
        public void Analyze_ProfitableEnemyToAttack_ShouldIgnoreFriendlyRobots()
        {
            // Arrange
            var veryRichFriendlyRobot = new Robot.Common.Robot()
            {
                OwnerName = _myRobot.OwnerName,
                Position = new Position(51, 51),
                Energy = 2000 // More than the threshold 1001
            };
            _allRobots.Add(veryRichFriendlyRobot);

            // Act
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            // Assert
            Assert.IsNull(result.ProfitableEnemyToAttack, "Friendly robots should never be considered as attack targets, regardless of their energy.");
        }

        #endregion

        #region Quality of Decision Tests

       
        [TestMethod]
        public void Analyze_OptimalStation_ShouldPreferFartherStationWithVastlyMoreEnergy()
        {
            // Arrange
            // This station is very close, but almost empty.
            var closePoorStation = new EnergyStation() { Position = new Position(52, 50), Energy = 30 };

            // This station is a bit farther, but has a huge energy reserve.
            // The cost of the extra path (a few moves) is negligible compared to the benefit.
            var farRichStation = new EnergyStation() { Position = new Position(55, 50), Energy = 1000 };

            _map.Stations = new List<EnergyStation>() { closePoorStation, farRichStation };

            // Act
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            // Assert
            Assert.IsNotNull(result.OptimalStation, "An optimal station should be found.");
            Assert.AreEqual(farRichStation.Position, result.OptimalStation.Position, "The algorithm should select the farther but much richer station.");
        }

        #endregion

        #region Multi-Step Path Planning

        [DataTestMethod]
        [DataRow(300, 0, 0, 10, 10, 10, 10, "Scenario 1: Energy (300) is enough for a jump costing 200. Move directly.")]
        [DataRow(100, 0, 0, 11, 4, 6, 2, "Scenario 2: Cost is 137, energy is 100. Two moves needed (137/100=1.37 -> 2). Take the first step halfway.")]
        [DataRow(150, 10, 10, 30, 25, 14, 13, "Scenario 3: Cost is 625, energy is 150. Five moves needed (625/150=4.16 -> 5). Take the first step 1/5 of the way.")]
        [DataRow(49, 50, 50, 40, 40, 48, 48, "Scenario 4: Cost is 200, energy is 49. Five moves needed (200/49=4.08 -> 5). Take the first step 1/5 of the way.")]
        [DataRow(0, 10, 10, 20, 20, 10, 10, "Scenario 5: Edge case. Zero energy. Robot should stay in place.")]
        public void Analyze_NextStep_ShouldPlanMultiStepJourneyWhenDirectJumpIsTooExpensive(
           int energy, int startX, int startY, int targetX, int targetY, int expectedX, int expectedY, string message)
        {
            // Arrange
            _myRobot.Energy = energy;
            _myRobot.Position = new Position(startX, startY);

            var targetStation = new EnergyStation() { Position = new Position(targetX, targetY) };
            _map.Stations = new List<EnergyStation>() { targetStation };

            var expectedPosition = new Position(expectedX, expectedY);

            // Act
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            // Assert
            Assert.IsNotNull(result.NextStepToTarget, message + " - Аналізатор не повинен повертати null.");
            Assert.AreEqual(expectedPosition, result.NextStepToTarget, message);
        }

        #endregion

    }
}
