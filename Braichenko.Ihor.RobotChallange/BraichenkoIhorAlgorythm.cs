using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange
{
    public class BraichenkoIhorAlgorythm : IRobotAlgorithm
    {
        string IRobotAlgorithm.Author
        {
            get { return "Braichenko Ihor"; }
        }

        RobotCommand IRobotAlgorithm.DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            var myRobot = robots[robotToMoveIndex];

            var newPosition = myRobot.Position;
            newPosition.X = newPosition.X + 1;
            newPosition.Y = newPosition.Y + 1;
            return new MoveCommand() { NewPosition = newPosition };
        }
    }
}
