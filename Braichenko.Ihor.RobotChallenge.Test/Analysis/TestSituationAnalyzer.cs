using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analyzer;
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
            _analyzer = new SituationAnalyzer();
            _myRobot = new Robot.Common.Robot() { Energy = 500, Position = new Position(50, 50), OwnerName = "Braichenko Ihor" };
            _allRobots = new List<Robot.Common.Robot>() { _myRobot };
            _map = new Map();
        }

        [DataTestMethod]
        [DataRow(10, 200, false, "Рання гра: рівно 200, недостатньо.")]
        [DataRow(10, 201, true, "Рання гра: 201, рівно достатньо для агресивного створення.")]
        [DataRow(30, 351, true, "Середня гра: 351, є запас в 1 енергію.")]
        [DataRow(42, 651, true, "Пізня гра: 651, є запас.")]
        [DataRow(48, 10000, false, "Кінець гри: створення заборонено.")]
        public void Analyze_ShouldCreateRobot_CorrectlyAppliesPhaseBasedLogic(int round, int energy, bool expected, string message)
        {
            _myRobot.Energy = energy;
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, round);
            Assert.AreEqual(expected, result.CanCreateRobot, message);
        }

        
        [TestMethod]
        public void Analyze_BestAction_ShouldBeToStayAndCollectWhenOnARichStation()
        {
            // Arrange
            var richCurrentStation = new EnergyStation() { Position = _myRobot.Position, Energy = 500 };
            var alternativeStation = new EnergyStation() { Position = new Position(60, 50), Energy = 300 };
            _map.Stations = new List<EnergyStation> { richCurrentStation, alternativeStation };
            _myRobot.Energy = 200; 

            // Act
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            // Assert
            Assert.IsTrue(result.CanCollectEnergy, "Найкраща дія - збирати енергію, оскільки робот на багатій станції.");
            Assert.IsNull(result.NextStepToTarget, "Робот не повинен планувати рух, оскільки залишатися вигідніше.");
        }

        
        [TestMethod]
        public void Analyze_BestAction_ShouldBeToMoveWhenABetterOpportunityExists()
        {
            // Arrange
            var poorCurrentStation = new EnergyStation() { Position = _myRobot.Position, Energy = 50 };
            var richAlternativeStation = new EnergyStation() { Position = new Position(40, 50), Energy = 800 };
            _map.Stations = new List<EnergyStation> { poorCurrentStation, richAlternativeStation };
            _myRobot.Energy = 200;

            // Act
            var result = _analyzer.Analyze(_myRobot, _map, _allRobots, 0);

            // Assert
            Assert.IsFalse(result.CanCollectEnergy, "Робот не повинен збирати енергію, оскільки є значно краща ціль.");
            Assert.IsNotNull(result.NextStepToTarget, "Робот повинен планувати рух до багатшої станції.");
            var finalTarget = richAlternativeStation.Position;
            var step = result.NextStepToTarget;
            Assert.AreEqual(Math.Sign(finalTarget.X - _myRobot.Position.X), Math.Sign(step.X - _myRobot.Position.X));
        }
    }
}
