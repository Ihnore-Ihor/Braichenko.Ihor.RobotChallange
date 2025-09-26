using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public class MoveToStationStrategy : IStrategy
    {
        public RobotCommand TryExecute(AnalysisResult analysisResult)
        {
            // TODO: Logic here
            // if (analysisResult.NearestFreeStation != null) {
            //     return new MoveCommand();
            // }
            return null;
        }
    }
}
