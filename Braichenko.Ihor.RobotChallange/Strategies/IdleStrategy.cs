using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analyzer;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public class IdleStrategy : IStrategy
    {
        public RobotCommand TryExecute(AnalysisResult analysisResult)
        {
            return new CollectEnergyCommand();
            //return new MoveCommand() { NewPosition = analysisResult.MyRobot.Position };
        }
    }
}
