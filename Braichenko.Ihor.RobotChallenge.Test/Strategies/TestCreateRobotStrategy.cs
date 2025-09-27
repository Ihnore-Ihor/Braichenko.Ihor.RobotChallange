using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Braichenko.Ihor.RobotChallange.Strategies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallenge.Test.Strategies
{
    [TestClass]
    public class TestCreateRobotStrategy
    {
        private readonly CreateRobotStrategy _strategy = new CreateRobotStrategy();

        [TestMethod]
        public void TryExecute_ReturnsCreateCommand_WhenCanCreateIsTrue()
        {
            var analysisResult = new AnalysisResult { CanCreateRobot = true };
            var command = _strategy.TryExecute(analysisResult);
            Assert.IsInstanceOfType(command, typeof(CreateNewRobotCommand));
        }

        [TestMethod]
        public void TryExecute_ReturnsNull_WhenCanCreateIsFalse()
        {
            var analysisResult = new AnalysisResult { CanCreateRobot = false };
            var command = _strategy.TryExecute(analysisResult);
            Assert.IsNull(command);
        }
    }
}
