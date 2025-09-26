using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public class CollectEnergyStrategy : IStrategy
    {
        public RobotCommand TryExecute(AnalysisResult analysisResult)
        {
            // TODO: Logic here
            // if (analysisResult.CanCollectEnergy) {
            //     return new CollectEnergyCommand();
            // }
            return null;
        }
    }
}
