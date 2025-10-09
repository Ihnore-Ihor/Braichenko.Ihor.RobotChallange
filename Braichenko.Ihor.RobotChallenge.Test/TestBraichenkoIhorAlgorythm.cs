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
        public void DoStep_ShouldPrioritizeCreatingRobotOverCollectingEnergy()
        {
            // Arrange
            _algorithm.RoundCount = 10; 
            var myRobot = new Robot.Common.Robot() { Energy = 202, Position = new Position(10, 10), OwnerName = "Braichenko Ihor" };
            var map = new Map()
            {
                Stations = new List<EnergyStation>() { new EnergyStation() { Position = new Position(10, 10), Energy = 500 } }
            };
            var robots = new List<Robot.Common.Robot> { myRobot };

            // Act
            var command = _algorithm.DoStep(robots, 0, map);

            // Assert
            Assert.IsInstanceOfType(command, typeof(CreateNewRobotCommand), "Створення робота має найвищий пріоритет, навіть якщо можна збирати енергію.");
        }
    }
}