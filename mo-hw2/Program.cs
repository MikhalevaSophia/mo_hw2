using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    const int POPULATION_SIZE = 4;
    const int NUM_GENERATIONS = 80000;
    static double MUTATION_RATE = 0.25;
    const double MIN_X = -2.0;
    const double MAX_X = 2.0;
    const double MIN_Y = -2.0;
    const double MAX_Y = 2.0;
    const int MAX_EXECUTION_TIME_SECONDS = 600;
    static DateTime startTime = DateTime.Now;

    class Individual
    {
        public double x;
        public double y;
        public double fitness;

        public Individual(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.fitness = 0.0;
        }

        public Individual() { }
    }

    static List<Individual> CreateInitialPopulation()
    {
        List<Individual> population = new List<Individual>(POPULATION_SIZE);

        Random random = new Random();
        for (int i = 0; i < POPULATION_SIZE; ++i)
        {
            double x = random.NextDouble() * (MAX_X - MIN_X) + MIN_X;
            double y = random.NextDouble() * (MAX_Y - MIN_Y) + MIN_Y;
            population.Add(new Individual(x, y));
        }

        return population;
    }

    static double FitnessFunction(double x, double y)
    {
        return Math.Exp(-x * x - y * y) / (1 + x*x + y*y);
    }

    static List<Individual> EvaluatePopulation(List<Individual> population)
    {
        foreach (var individual in population)
        {
            double fitness = FitnessFunction(individual.x, individual.y);
            individual.fitness = fitness;
        }
        return population;
    }

    static List<Individual> SelectParents(List<Individual> population)
    {
        List<Individual> parents = new List<Individual>();
        const int TOURNAMENT_SIZE = 3;

        Random random = new Random();
        for (int i = 0; i < population.Count; ++i)
        {
            Individual bestParent = new Individual();
            double bestFitness = double.NegativeInfinity;

            for (int j = 0; j < TOURNAMENT_SIZE; ++j)
            {
                int randomIndex = random.Next(0, population.Count);
                Individual candidate = population[randomIndex];

                double distance = Math.Sqrt(Math.Pow(candidate.x - 0.653297871, 2) + Math.Pow(candidate.y + 0.00000000564618584, 2));

                double distanceWeight = Math.Exp(-0.1 * distance);

                double weightedFitness = candidate.fitness * distanceWeight;

                if (weightedFitness > bestFitness)
                {
                    bestParent = candidate;
                    bestFitness = weightedFitness;
                }
            }

            parents.Add(bestParent);
        }

        return parents;
    }

    static Individual Mutate(Individual individual)
    {
        Random random = new Random();

        double x = individual.x;
        double y = individual.y;

        if (random.NextDouble() < MUTATION_RATE)
        {
            x += (random.NextDouble() * 2 - 1) * MUTATION_RATE;
            x = Math.Max(MIN_X, Math.Min(x, MAX_X));
        }

        if (random.NextDouble() < MUTATION_RATE)
        {
            y += (random.NextDouble() * 2 - 1) * MUTATION_RATE;
            y = Math.Max(MIN_Y, Math.Min(y, MAX_Y));
        }

        Individual mutatedIndividual = new Individual(x, y);
        mutatedIndividual.fitness = FitnessFunction(x, y);

        return mutatedIndividual;
    }

    static Individual Crossover(Individual parent1, Individual parent2)
    {
        Random random = new Random();

        double x = (random.NextDouble() < 0.5) ? parent1.x : parent2.x;
        double y = (random.NextDouble() < 0.5) ? parent1.y : parent2.y;

        Individual child = new Individual(x, y);
        child.fitness = FitnessFunction(x, y);

        return child;
    }

    static bool CheckConvergence(List<Individual> population)
    {
        double sum = 0;
        foreach (var p in population)
        {
            sum += p.fitness;
        }
        double average = sum / population.Count;

        int numConverged = 0;
        double tolerance = 0.0001;

        foreach (var p in population)
        {
            if (Math.Abs(p.fitness - average) < tolerance)
            {
                numConverged++;
            }
        }

        double convergenceRatio = (double)numConverged / population.Count;
        return (convergenceRatio >= 0.7);
    }

    static void PrintToTxt(List<Individual> population, int i)
    {
        string filePath = @"C:\Users\mi\source\repos\mo-hw2\result.txt";
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            double sum = 0;
            List<int> iterationsToPrint = new List<int>
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 50, 100, 200, 500, 1000, 5000, 10000, 20000, 50000, 80000
            };

            if (iterationsToPrint.Contains(i))
            {
                writer.WriteLine(i);
                writer.WriteLine("X:");
                foreach (var ind in population)
                {
                    writer.WriteLine(ind.x);
                }
                writer.WriteLine("Y:");
                foreach (var ind in population)
                {
                    writer.WriteLine(ind.y);
                }
                writer.WriteLine("F:");
                foreach (var ind in population)
                {
                    sum += ind.fitness;
                    writer.WriteLine(ind.fitness);
                }
                Individual bestIndividual = population.OrderByDescending(p => p.fitness).First();
                double maxF = bestIndividual.fitness;
                writer.WriteLine("MAX:" + maxF);
                writer.WriteLine("AVG:" + sum / population.Count);
                writer.WriteLine();
            }
        }
    }

    static void Main()
    {
        List<Individual> population = CreateInitialPopulation();
        double initialMutationRate = MUTATION_RATE;

        for (int generation = 0; generation < NUM_GENERATIONS; generation++)
        {
            population = EvaluatePopulation(population);
            List<Individual> parents = SelectParents(population);
            PrintToTxt(population, generation);

            double currentConvergence = (double)generation / NUM_GENERATIONS;
            MUTATION_RATE = initialMutationRate * (1.0 - currentConvergence);

            for (int j = 0; j < POPULATION_SIZE - 1; j += 2)
            {
                Individual child1 = Crossover(parents[j], parents[j + 1]);
                child1 = Mutate(child1);

                population[j] = child1;

                population[j].x = child1.x;
                population[j].y = child1.y;
                population[j].fitness = FitnessFunction(population[j].x, population[j].y);
            }
            var currentTime = DateTime.Now;
            var executionTime = (int)(currentTime - startTime).TotalSeconds;
            if (executionTime >= MAX_EXECUTION_TIME_SECONDS)
            {
                Console.WriteLine("RunTime Limit");
                break;
            }

            if (CheckConvergence(population))
            {
                Console.WriteLine($"Fitness Convergence {generation}");
                break;
            }
        }
        Individual bestIndividual = population.OrderByDescending(p => p.fitness).First();
        Console.WriteLine($"Best solution: x = {bestIndividual.x}, y = {bestIndividual.y}, f(x, y) = {bestIndividual.fitness}");
    }
}
