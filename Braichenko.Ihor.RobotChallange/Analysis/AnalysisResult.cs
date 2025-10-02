using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Analysis
{
    public class AnalysisResult
    {
        public Robot.Common.Robot MyRobot { get; set; }
        public IList<Robot.Common.Robot> AllRobots { get; set; }
        public Map Map { get; set; }

        public bool CanCreateRobot { get; set; }
        public bool CanCollectEnergy { get; set; }
        public EnergyStation OptimalStation { get; set; }
        public Robot.Common.Robot ProfitableEnemyToAttack { get; set; }
        public Position NextStepToTarget { get; set; }
    }
}
