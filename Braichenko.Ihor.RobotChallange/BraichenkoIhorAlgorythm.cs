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
        public string Author => "Braichenko Ihor";

        private readonly SituationAnalyzer _analyzer = new SituationAnalyzer();
        private readonly List<IStrategy> _strategies;

        public BraichenkoIhorAlgorythm()
        {
            // Priorities of strategies
            _strategies = new List<IStrategy>
            {
                new CreateRobotStrategy(),
                new CollectEnergyStrategy(),
                new AttackStrategy(),
                new MoveToStationStrategy(),
                new IdleStrategy() // Backup strategy
            };
        }

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            var analysisResult = _analyzer.Analyze(robots[robotToMoveIndex], map, robots);

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
