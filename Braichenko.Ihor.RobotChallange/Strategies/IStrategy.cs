using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public interface IStrategy
    {
        // Returns command to execute, or null if strategy is not applicable
        RobotCommand TryExecute(AnalysisResult analysisResult);
    }
}
