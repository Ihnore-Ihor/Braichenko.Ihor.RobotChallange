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
            // Ця стратегія тепер універсальна. Вона виконує будь-яку розраховану дію з переміщення.
            // Якщо "мозок" вирішив, що найкраща дія - це атака, NextStepToTarget буде вести до ворога.
            // Якщо найкраща дія - рух до станції, NextStepToTarget буде вести до станції.
            if (analysisResult.NextStepToTarget != null)
            {
                return new MoveCommand() { NewPosition = analysisResult.NextStepToTarget };
            }
            return null;
        }
    }
}
