using System;
using System.Collections.Generic;
using Braichenko.Ihor.RobotChallange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallenge.Test
{
    [TestClass]
    public class TestBraichenkoIhorAlgorythm
    {
        private BraichenkoIhorAlgorythm _algorithm;

        [TestInitialize]
        public void Setup()
        {
            _algorithm = new BraichenkoIhorAlgorythm();
        }

        [TestMethod]
        public void DoStep_CheckPriority1Decision()
        {
            // Arrange: Create an "ideal" situation where ALL actions are possible:
            // 1. Enough energy to create (302)
            // 2. Robot stands near a station
            // 3. There is a nearest free station
            var myRobot = new Robot.Common.Robot() { Energy = 302, Position = new Position(0, 0) };
            var map = new Map()
            {
                Stations = new List<EnergyStation>()
                {
                    new EnergyStation() { Position = new Position(1, 1), Energy = 100 },
                    new EnergyStation() { Position = new Position(10, 10), Energy = 100 }
                }
            };
            var robots = new List<Robot.Common.Robot> { myRobot };

            // Act
            var command = _algorithm.DoStep(robots, 0, map);

            // Assert
            Assert.IsInstanceOfType(command, typeof(CreateNewRobotCommand), "Creating a robot has the highest priority.");
        }

        [TestMethod]
        public void DoStep_CheckPriority2Decision()
        {
            // Arrange: Not enough energy to create, but the robot stands near a station.
            var myRobot = new Robot.Common.Robot() { Energy = 150, Position = new Position(0, 0) };
            var map = new Map()
            {
                Stations = new List<EnergyStation>() { new EnergyStation() { Position = new Position(1, 1), Energy = 100 } }
            };
            var robots = new List<Robot.Common.Robot> { myRobot };

            // Act
            var command = _algorithm.DoStep(robots, 0, map);

            // Assert
            Assert.IsInstanceOfType(command, typeof(CollectEnergyCommand), "If creation is not possible, collecting energy is the next priority.");
        }

        [TestMethod]
        public void DoStep_CheckPriority3Decision()
        {
            // Arrange: Енергії мало (150), станцій поруч немає.
            var myRobot = new Robot.Common.Robot() { Energy = 150, Position = new Position(0, 0) };
            var nearestStation = new Position(10, 10); // Вартість стрибка = 200
            var map = new Map()
            {
                Stations = new List<EnergyStation>() { new EnergyStation() { Position = nearestStation, Energy = 100 } }
            };
            var robots = new List<Robot.Common.Robot> { myRobot };

            // Act
            var command = _algorithm.DoStep(robots, 0, map);

            // Assert
            Assert.IsInstanceOfType(command, typeof(MoveCommand), "Коли нічого іншого робити, робот повинен рухатись.");

            // ОНОВЛЕНА ПЕРЕВІРКА:
            // Алгоритм розрахує: 200/150 = 1.33 -> 2 ходи.
            // Перший крок буде на 1/2 шляху до (10,10), тобто (5,5).
            var expectedNextStep = new Position(5, 5);
            var moveCommand = (MoveCommand)command;
            Assert.AreEqual(expectedNextStep, moveCommand.NewPosition, "Робот має зробити перший крок багатоходового плану.");
        }

        [TestMethod]
        public void DoStep_CheckPriority3Decision_Attack()
        {
            // Arrange:
            // 1. Cannot create (not enough energy)
            // 2. Nothing to collect (no stations nearby)
            // 3. There is a profitable enemy!
            // 4. There is also a free station (but attack has higher priority)
            var myRobot = new Robot.Common.Robot() { Energy = 200, Position = new Position(50, 50) };
            var enemyRobot = new Robot.Common.Robot() { Energy = 1500, Position = new Position(51, 51), OwnerName = "Enemy" };
            var freeStation = new EnergyStation() { Position = new Position(0, 0), Energy = 100 };

            var map = new Map() { Stations = new List<EnergyStation>() { freeStation } };
            var robots = new List<Robot.Common.Robot> { myRobot, enemyRobot };

            // Act
            var command = _algorithm.DoStep(robots, 0, map);

            // Assert
            Assert.IsInstanceOfType(command, typeof(MoveCommand), "There should be a move command.");
            var moveCommand = (MoveCommand)command;
            Assert.AreEqual(enemyRobot.Position, moveCommand.NewPosition, "Movement should be towards the enemy, since attack has priority over moving to a station.");
        }
    }
}