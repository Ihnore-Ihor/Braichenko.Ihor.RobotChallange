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
    public class TestCollectEnergyStrategy
    {
        private readonly CollectEnergyStrategy _strategy = new CollectEnergyStrategy();

        [TestMethod]
        public void TryExecute_ReturnsCollectCommand_WhenCanCollectIsTrue()
        {
            var analysisResult = new AnalysisResult { CanCollectEnergy = true };
            var command = _strategy.TryExecute(analysisResult);
            Assert.IsInstanceOfType(command, typeof(CollectEnergyCommand));
        }

        [TestMethod]
        public void TryExecute_ReturnsNull_WhenCanCollectIsFalse()
        {
            var analysisResult = new AnalysisResult { CanCollectEnergy = false };
            var command = _strategy.TryExecute(analysisResult);
            Assert.IsNull(command);
        }
    }
}
