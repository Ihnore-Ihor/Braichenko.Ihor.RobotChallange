using Braichenko.Ihor.RobotChallange.Analyzer;
using Robot.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Braichenko.Ihor.RobotChallange.Analyzer
{
    public class SituationAnalyzer
    {
        private const int CREATION_COST = 200;
        private const int SAFETY_BUFFER = 150;

        private const int EXPANSION_PHASE_ROBOT_COUNT = 80; // Threshold for peaceful "Expansion" phase

        // Coefficients for Phase 1 ("Expansion")
        private const double EXPANSION_ATTACK_PENALTY_MULTIPLIER = 0.1; // Strongly penalize attack (multiplier)
        private const double EXPANSION_FREE_STATION_BONUS_MULTIPLIER = 1.5; // Encourage capturing free stations

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
            var bestAction = FindBestAction(myRobot, map, allRobots);
            result.CanCollectEnergy = bestAction.ShouldCollect;

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
            if (myRobotsCount < EXPANSION_PHASE_ROBOT_COUNT) // 'Expansion' phase
            {
                requiredEnergy = CREATION_COST + 1;
            }
            else // 'Optimization' phase
            {
                requiredEnergy = CREATION_COST + SAFETY_BUFFER;
            }

            if (round > 40 && myRobot.Energy < (CREATION_COST + SAFETY_BUFFER * 3))
            {
                return false;
            }

            return myRobot.Energy >= requiredEnergy;
        }


        private BestAction FindBestAction(Robot.Common.Robot myRobot, Map map, IList<Robot.Common.Robot> allRobots)
        {
            // 1. Determine current game phase
            int myRobotsCount = allRobots.Count(r => r.OwnerName == myRobot.OwnerName);
            bool isExpansionPhase = myRobotsCount < EXPANSION_PHASE_ROBOT_COUNT;
            var friendlyRobots = allRobots.Where(r => r.OwnerName == myRobot.OwnerName && r != myRobot).ToList();

            // 2. Evaluate benefit of STAYING PUT.
            double stayPutScore = 0;
            bool canCollectHere = false;
            var stationUnderneath = map.GetResource(myRobot.Position);
            if (stationUnderneath != null && stationUnderneath.Energy > 0)
            {
                stayPutScore = stationUnderneath.Energy * 1.1; // Multiplier 1.1 - advantage for holding position
                canCollectHere = true;
            }

            var bestAction = new BestAction { Score = stayPutScore, TargetPosition = myRobot.Position, ShouldCollect = canCollectHere };

            // 3. Iterate over all stations as ALTERNATIVES.
            foreach (var station in map.Stations)
            {
                if (station.Position == myRobot.Position) continue;

                var robotOnStation = allRobots.FirstOrDefault(r => r.Position == station.Position);
                if (robotOnStation != null && robotOnStation.OwnerName == myRobot.OwnerName) continue;

                // --- Calculate Benefits ---
                double potentialGain = station.Energy;
                if (robotOnStation != null) // If station is occupied by an enemy
                {
                    potentialGain += robotOnStation.Energy * 0.05; // Account for stolen energy

                    // INTEGRATION: Bonuses/penalties for defender's strength
                    if (robotOnStation.Energy < 100) potentialGain += 150; // Bonus for a weak enemy
                    else if (robotOnStation.Energy > myRobot.Energy) potentialGain -= 100; // Penalty for a stronger one
                }

                // --- Calculate Costs ---
                int moveCost = CalculateToroidalDistanceSquare(myRobot.Position, station.Position);
                bool isAttack = robotOnStation != null;
                if (isAttack) moveCost += 50;

                // --- Calculate Base Score (ROI) ---
                double moveScore = potentialGain / (moveCost + 1);

                // --- Apply Dynamic Multipliers ---
                if (isExpansionPhase)
                {
                    if (isAttack) moveScore *= EXPANSION_ATTACK_PENALTY_MULTIPLIER; // Penalty for attack
                    else moveScore *= EXPANSION_FREE_STATION_BONUS_MULTIPLIER; // Bonus for peaceful capture
                }

                // --- INTEGRATION: 'Territorial Control' ---
                int competitorsCount = friendlyRobots.Count(friendly =>
                    CalculateToroidalDistanceSquare(friendly.Position, station.Position) < moveCost);
                moveScore -= competitorsCount * 50; // Penalty for each ally closer to the target

                // --- Final Comparison ---
                if (moveScore > bestAction.Score)
                {
                    bestAction = new BestAction { Score = moveScore, TargetPosition = station.Position, ShouldCollect = false };
                }
            }
            return bestAction;
        }

        // --- Helper methods and movement methods ---
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

            var nextPosition = new Position(nextX, nextY);

            if (IsPositionOccupied(nextPosition, myRobot, allRobots))
            {
                return from;
            }

            return nextPosition;
        }

        private bool IsPositionOccupied(Position pos, Robot.Common.Robot myRobot, IList<Robot.Common.Robot> allRobots)
        {
            return allRobots.Any(r => r != myRobot && r.Position == pos);
        }
    }
}