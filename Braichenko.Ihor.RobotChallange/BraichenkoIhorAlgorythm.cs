using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Braichenko.Ihor.RobotChallange.Analysis;
using Braichenko.Ihor.RobotChallange.Strategies;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange
{
    public class BraichenkoIhorAlgorythm : IRobotAlgorithm
    {

        public int RoundCount { get; set; }
        public string Author => "Braichenko Ihor";

        private readonly SituationAnalyzer _analyzer = new SituationAnalyzer();
        private readonly List<IStrategy> _strategies;

        public BraichenkoIhorAlgorythm()
        {
            // Priorities of strategies
            _strategies = new List<IStrategy>
            {
                new CreateRobotStrategy(),      // 1. Експансія - головний пріоритет.
                new CollectEnergyStrategy(),    // 2. Якщо не створюємо, то збираємо (якщо стоїмо на станції).
                new MoveToStationStrategy(),    // 3. Якщо перші два неможливі - виконуємо найкращу економічну дію (рух/атака).
                new IdleStrategy()
            };

            Logger.OnLogRound += Logger_OnLogRound;
        }

        private void Logger_OnLogRound(object sender, LogRoundEventArgs e)
        {
            RoundCount++;
        }

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            int round = RoundCount;
            var analysisResult = _analyzer.Analyze(robots[robotToMoveIndex], map, robots, round);

            foreach (var strategy in _strategies)
            {
                var command = strategy.TryExecute(analysisResult);
                if (command != null)
                {
                    return command; 
                }
            }

            // This should never happen because of the IdleStrategy
            return new MoveCommand() { NewPosition = robots[robotToMoveIndex].Position };
        }
    }
}
