using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot.Common;

namespace Braichenko.Ihor.RobotChallange.Analysis
{
    public class SituationAnalyzer
    {
        private const int CREATION_COST = 200;
        private const int SAFETY_BUFFER = 150;

        // Class describing the best action
        private class BestAction
        {
            public double Score { get; set; }
            public Position TargetPosition { get; set; } 
            public bool ShouldCollect { get; set; } 
        }

        public AnalysisResult Analyze(Robot.Common.Robot myRobot, Map map, IList<Robot.Common.Robot> allRobots, int round)
        {
            var result = new AnalysisResult
            {
                MyRobot = myRobot,
                AllRobots = allRobots,
                Map = map
            };

            result.CanCreateRobot = ShouldCreateRobot(myRobot, allRobots, round);

            // Main method: find the best possible action
            var bestAction = FindBestAction(myRobot, map, allRobots);

            result.CanCollectEnergy = bestAction.ShouldCollect;

            // If the best action is to move, set the target
            if (bestAction.TargetPosition != myRobot.Position)
            {
                result.NextStepToTarget = CalculateSprintStep(myRobot, allRobots, myRobot.Position, bestAction.TargetPosition);
            }
            return result;
        }

        private bool ShouldCreateRobot(Robot.Common.Robot myRobot, IList<Robot.Common.Robot> allRobots, int round)
        {
            int myRobotsCount = allRobots.Count(r => r.OwnerName == myRobot.OwnerName);
            if (myRobotsCount >= 100 || round > 47) return false;

            int requiredEnergy;
            if (round <= 20) requiredEnergy = CREATION_COST + 1;
            else if (round <= 40) requiredEnergy = CREATION_COST + SAFETY_BUFFER;
            else requiredEnergy = CREATION_COST + SAFETY_BUFFER * 3;
            return myRobot.Energy >= requiredEnergy;
        }

        private BestAction FindBestAction(Robot.Common.Robot myRobot, Map map, IList<Robot.Common.Robot> allRobots)
        {
            // 1. Evaluate the benefit of staying in place.
            double stayPutScore = 0;
            bool canCollectHere = false;
            var stationUnderneath = map.GetResource(myRobot.Position);
            if (stationUnderneath != null && stationUnderneath.Energy > 0)
            {
                // Benefit of staying = energy we collect.
                // Multiply by 1.1 to give a slight advantage to the current position and avoid unnecessary moves.
                stayPutScore = stationUnderneath.Energy * 1.1;
                canCollectHere = true;
            }

            // Initially, the best action is to stay here.
            var bestAction = new BestAction { Score = stayPutScore, TargetPosition = myRobot.Position, ShouldCollect = canCollectHere };

            // 2. Iterate through all other stations as alternatives.
            foreach (var station in map.Stations)
            {
                if (station.Position == myRobot.Position) continue;

                var robotOnStation = allRobots.FirstOrDefault(r => r.Position == station.Position);
                // Ignore stations occupied by allies
                if (robotOnStation != null && robotOnStation.OwnerName == myRobot.OwnerName) continue;

                // Calculate net profit from moving
                int moveCost = CalculateToroidalDistanceSquare(myRobot.Position, station.Position);
                double potentialGain = station.Energy;

                if (robotOnStation != null) // If station is occupied by an enemy
                {
                    moveCost += 50; // Add attack cost
                    potentialGain += robotOnStation.Energy * 0.05;
                }

                double moveScore = potentialGain - moveCost;

                // If net profit from moving is higher than staying...
                if (moveScore > bestAction.Score)
                {
                    // ...then the new best action is to move to this station.
                    bestAction = new BestAction { Score = moveScore, TargetPosition = station.Position, ShouldCollect = false };
                }
            }
            return bestAction;
        }

        // Helper methods and movement methods
        private int Min2D(int v1, int v2, int mapSize) { int d = Math.Abs(v1 - v2); return Math.Min(d, mapSize - d); }
        private int CalculateToroidalDistanceSquare(Position p1, Position p2)
        {
            int dx = Min2D(p1.X, p2.X, 100);
            int dy = Min2D(p1.Y, p2.Y, 100);
            return dx * dx + dy * dy;
        }

        private Position CalculateSprintStep(Robot.Common.Robot myRobot, IList<Robot.Common.Robot> allRobots, Position from, Position to)
        {
            var totalDistanceSquare = CalculateToroidalDistanceSquare(from, to);
            if (myRobot.Energy >= totalDistanceSquare) return to;
            if (myRobot.Energy == 0) return from;

            var dx = to.X - from.X; if (Math.Abs(dx) > 50) dx = dx > 0 ? dx - 100 : dx + 100;
            var dy = to.Y - from.Y; if (Math.Abs(dy) > 50) dy = dy > 0 ? dy - 100 : dy + 100;

            int neededSteps = Math.Max(2, (int)Math.Ceiling((double)totalDistanceSquare / myRobot.Energy));
            var stepX = (int)Math.Round((double)dx / neededSteps);
            var stepY = (int)Math.Round((double)dy / neededSteps);

            var nextX = (from.X + stepX + 100) % 100;
            var nextY = (from.Y + stepY + 100) % 100;

            return new Position(nextX, nextY);
        }
    }
}
