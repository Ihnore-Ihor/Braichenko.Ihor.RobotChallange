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
        // --- Константи для нової логіки ---
        private const int CREATION_COST = 200; // Вартість створення (100 втрата + 100 новому)
        private const int SAFETY_BUFFER = 150; // Запас енергії "на життя" після створення

        public AnalysisResult Analyze(Robot.Common.Robot myRobot, Map map, IList<Robot.Common.Robot> allRobots, int round)
        {
            var result = new AnalysisResult
            {
                MyRobot = myRobot,
                AllRobots = allRobots,
                Map = map
            };

            // --- 1. НОВИЙ АЛГОРИТМ: "Найкращий Момент для Розмноження" ---
            result.CanCreateRobot = ShouldCreateRobot(myRobot, allRobots, round);

            // --- 2. Логіка Збору Енергії ---
            var stationUnderneath = map.GetResource(myRobot.Position);
            result.CanCollectEnergy = stationUnderneath != null && stationUnderneath.Energy > 0;

            // --- 3. Економічна логіка (без змін) ---
            var bestOpportunity = FindBestEconomicOpportunity(myRobot, map, allRobots);
            result.OptimalStation = bestOpportunity?.TargetStation;
            result.ProfitableEnemyToAttack = bestOpportunity?.TargetEnemy;
            if (bestOpportunity != null)
            {
                result.NextStepToTarget = CalculateSprintStep(myRobot, allRobots, myRobot.Position, bestOpportunity.TargetPosition);
            }

            return result;
        }

        /// <summary>
        /// КЛЮЧОВИЙ НОВИЙ МЕТОД: Вирішує, чи є зараз найкращий момент для створення робота.
        /// </summary>
        private bool ShouldCreateRobot(Robot.Common.Robot myRobot, IList<Robot.Common.Robot> allRobots, int round)
        {
            // Умова 1: Чи не досягли ми ліміту?
            int myRobotsCount = allRobots.Count(r => r.OwnerName == myRobot.OwnerName);
            if (myRobotsCount >= 100) return false;

            // Умова 2: Чи не занадто пізно в грі?
            if (round > 47) return false;

            // Умова 3: Динамічний поріг енергії залежно від фази гри
            int requiredEnergy;
            if (round <= 20) // Фаза 1: Гіпер-Експансія
            {
                // Ризикуємо: створюємо, як тільки з'являється мінімальна можливість
                requiredEnergy = CREATION_COST + 1;
            }
            else if (round <= 40) // Фаза 2: Безпечна Експансія
            {
                // Створюємо, тільки якщо залишається запас "на життя"
                requiredEnergy = CREATION_COST + SAFETY_BUFFER;
            }
            else // Фаза 3: Пізня Гра
            {
                // Створюємо, тільки якщо є величезний надлишок енергії
                requiredEnergy = CREATION_COST + SAFETY_BUFFER * 3; // ~650
            }

            return myRobot.Energy >= requiredEnergy;
        }

        // --- Клас для опису економічної можливості ---
        private class EconomicOpportunity
        {
            public double Score { get; set; } // ROI
            public Position TargetPosition { get; set; }
            public EnergyStation TargetStation { get; set; } // Може бути null
            public Robot.Common.Robot TargetEnemy { get; set; } // Може бути null
        }

        // --- КЛЮЧОВИЙ НОВИЙ МЕТОД: Оцінка ROI ---
        private EconomicOpportunity FindBestEconomicOpportunity(Robot.Common.Robot myRobot, Map map, IList<Robot.Common.Robot> allRobots)
        {
            var opportunities = new List<EconomicOpportunity>();
            var enemies = allRobots.Where(r => r.OwnerName != myRobot.OwnerName).ToList();

            // 1. Оцінюємо всі станції як потенційні цілі
            foreach (var station in map.Stations)
            {
                var robotOnStation = allRobots.FirstOrDefault(r => r.Position == station.Position);
                int cost;
                double potentialGain = station.Energy;

                if (robotOnStation == null) // Станція вільна
                {
                    cost = CalculateToroidalDistanceSquare(myRobot.Position, station.Position);
                    opportunities.Add(new EconomicOpportunity
                    {
                        Score = potentialGain / (cost + 1),
                        TargetPosition = station.Position,
                        TargetStation = station
                    });
                }
                else if (robotOnStation.OwnerName != myRobot.OwnerName) // Станція зайнята ворогом ("Station Jacking")
                {
                    cost = CalculateToroidalDistanceSquare(myRobot.Position, station.Position) + 50; // Вартість руху + атаки
                    potentialGain += robotOnStation.Energy * 0.05; // Вигода = енергія станції + вкрадена енергія
                    opportunities.Add(new EconomicOpportunity
                    {
                        Score = potentialGain / (cost + 1),
                        TargetPosition = station.Position,
                        TargetStation = station,
                        TargetEnemy = robotOnStation
                    });
                }
                // Станції, зайняті союзниками, ігноруємо
            }

            // 2. Оцінюємо всіх ворогів у полі як цілі для "Пограбування"
            foreach (var enemy in enemies)
            {
                // Не розглядаємо ворогів на станціях, бо ми їх вже оцінили вище
                if (map.GetResource(enemy.Position) != null) continue;

                double potentialGain = enemy.Energy * 0.05;
                int cost = CalculateToroidalDistanceSquare(myRobot.Position, enemy.Position) + 50;

                // "Пограбування" вигідне, тільки якщо прибуток значно перевищує витрати
                if (potentialGain > cost)
                {
                    opportunities.Add(new EconomicOpportunity
                    {
                        Score = potentialGain / (cost + 1),
                        TargetPosition = enemy.Position,
                        TargetEnemy = enemy
                    });
                }
            }

            if (!opportunities.Any()) return null;

            // Повертаємо найкращу можливість з найвищим ROI
            return opportunities.OrderByDescending(o => o.Score).First();
        }

        // --- Допоміжні методи та методи руху (з правильним розрахунком відстані) ---
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

            // Спрощений спринт: робимо крок, щоб дістатися за 2-3 ходи
            int neededSteps = Math.Max(2, (int)Math.Ceiling((double)totalDistanceSquare / myRobot.Energy));

            var stepX = (int)Math.Round((double)dx / neededSteps);
            var stepY = (int)Math.Round((double)dy / neededSteps);

            var nextX = (from.X + stepX + 100) % 100;
            var nextY = (from.Y + stepY + 100) % 100;

            return new Position(nextX, nextY);
        }
    }
}
