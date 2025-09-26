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
        [TestMethod]
        public void TestDoStep_ShouldMoveRobotDownAndRight()
        {
            // Arrange
            var algorithm = new BraichenkoIhorAlgorythm();
            var robots = new List<Robot.Common.Robot>
            {
                new Robot.Common.Robot() { Energy = 100, Position = new Position(5, 5) }
            };
            var map = new Map() { MaxPozition = new Position(100, 100) };
            int robotToMoveIndex = 0;

            // Act
            RobotCommand command = ((IRobotAlgorithm)algorithm).DoStep(robots, robotToMoveIndex, map);

            // Assert
            Assert.IsInstanceOfType(command, typeof(MoveCommand));
            var moveCommand = (MoveCommand)command;
            Assert.AreEqual(6, moveCommand.NewPosition.X);
            Assert.AreEqual(6, moveCommand.NewPosition.Y);
        }
    }
}