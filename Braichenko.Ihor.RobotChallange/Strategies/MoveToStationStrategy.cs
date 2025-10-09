using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analyzer;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public class MoveToStationStrategy : IStrategy
    {
        public RobotCommand TryExecute(AnalysisResult analysisResult)
        {
            if (analysisResult.NextStepToTarget != null)
            {
                return new MoveCommand() { NewPosition = analysisResult.NextStepToTarget };
            }
            return null;
        }
    }
}
