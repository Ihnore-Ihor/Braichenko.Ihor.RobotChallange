using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public class AttackStrategy : IStrategy
    {
        public RobotCommand TryExecute(AnalysisResult analysisResult)
        {
            if (analysisResult.ProfitableEnemyToAttack != null)
            {
                // when stepping towards enemy we attack automatically
                return new MoveCommand() { NewPosition = analysisResult.ProfitableEnemyToAttack.Position };
            }
            return null;
        }
    } 
}
