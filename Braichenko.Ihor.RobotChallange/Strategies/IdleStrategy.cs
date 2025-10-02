using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Strategies
{
    public class IdleStrategy : IStrategy
    {
        public RobotCommand TryExecute(AnalysisResult analysisResult)
        {
            // Якщо всі інші стратегії не спрацювали, пропускаємо хід.
            // Ми повертаємо MoveCommand на поточну позицію, щоб явно 
            // повідомити рушій про дію (пропуск ходу).
            return new MoveCommand() { NewPosition = analysisResult.MyRobot.Position };
        }
    }
}
