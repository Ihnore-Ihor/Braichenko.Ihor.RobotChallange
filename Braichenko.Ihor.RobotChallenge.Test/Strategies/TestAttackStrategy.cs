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
    public class TestAttackStrategy
    {
        private readonly AttackStrategy _strategy = new AttackStrategy();

        [TestMethod]
        public void TryExecute_ReturnsMoveCommandToEnemy_WhenProfitableEnemyExists()
        {
            var enemyPosition = new Position(10, 10);
            var enemy = new Robot.Common.Robot() { Position = enemyPosition };
            var analysisResult = new AnalysisResult { ProfitableEnemyToAttack = enemy };

            var command = _strategy.TryExecute(analysisResult);

            Assert.IsInstanceOfType(command, typeof(MoveCommand));
            Assert.AreEqual(enemyPosition, ((MoveCommand)command).NewPosition);
        }

        [TestMethod]
        public void TryExecute_ReturnsNull_WhenNoProfitableEnemy()
        {
            var analysisResult = new AnalysisResult { ProfitableEnemyToAttack = null };
            var command = _strategy.TryExecute(analysisResult);
            Assert.IsNull(command);
        }
    }
}
