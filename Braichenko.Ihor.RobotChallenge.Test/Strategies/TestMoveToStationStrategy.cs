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
    public class TestMoveToStationStrategy
    {
        private readonly MoveToStationStrategy _strategy = new MoveToStationStrategy();

        [TestMethod]
        public void TryExecute_WhenTargetStepExists_ReturnsMoveCommandToThatStep()
        {
            var nextStep = new Position(5, 6);
            var analysisResult = new AnalysisResult { NextStepToTarget = nextStep };
            var command = _strategy.TryExecute(analysisResult);

            Assert.IsInstanceOfType(command, typeof(MoveCommand));
            Assert.AreEqual(nextStep, ((MoveCommand)command).NewPosition);
        }

        [TestMethod]
        public void TryExecute_WhenNoTargetStep_ReturnsNull()
        {
            var analysisResult = new AnalysisResult { NextStepToTarget = null };
            var command = _strategy.TryExecute(analysisResult);
            Assert.IsNull(command);
        }
    }
}