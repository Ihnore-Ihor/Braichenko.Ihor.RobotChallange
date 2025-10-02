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

        /// <summary>
        /// Перевіряє ключовий пріоритет: створення нового робота є важливішим,
        /// ніж збір енергії, навіть якщо робот стоїть на багатій станції.
        /// </summary>
        [TestMethod]
        public void DoStep_ShouldPrioritizeCreatingRobotOverCollectingEnergy()
        {
            // Arrange: Робот може і створювати (достатньо енергії), і збирати (стоїть на станції).
            _algorithm.RoundCount = 10; // Рання гра для агресивного створення
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