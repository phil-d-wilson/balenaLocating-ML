using System;
using System.Collections.Generic;
using System.Linq;

namespace BalenaLocatingApi.Services
{
    public class ClassificationService
    {
        private readonly double[][] _trainingData;
        private StorageService _storageService;

        public ClassificationService()
        {
            _storageService = new StorageService();
            _trainingData = _storageService.GetDataAsync();
        }

        /// <summary>
        /// Classifies the unknown item using the
        /// training data as predictor points
        /// </summary>
        /// <param name="unknownItem">the item being classified</param>
        /// <returns></returns>
        public int Classify(IReadOnlyList<double> unknownItem)
        {
            const int k = 3;
            const int numberOfClasses = 3;

            var numberOfTrainingDataPoints = _trainingData.Length;

            var distances = GetDistancesToTrainingDataPoints(unknownItem, numberOfTrainingDataPoints);

            var orderedDistances = GetOrderedDistances(numberOfTrainingDataPoints, distances);

            var kNearestDistances = FindKNearestTrainingPoints(k, orderedDistances, distances);

            var votes = GetNearestPointVotes(numberOfClasses, k, kNearestDistances, orderedDistances);

            return votes.ToList().IndexOf(votes.Max());
        }

        /// <summary>
        /// Finds the distances from the unknown item
        /// to all of the training data items
        /// </summary>
        /// <param name="unknownItem">the item being classified</param>
        /// <param name="numberOfTrainingDataPoints">number of training tuples</param>
        /// <returns></returns>
        private double[] GetDistancesToTrainingDataPoints(IReadOnlyList<double> unknownItem, int numberOfTrainingDataPoints)
        {
            var distances = new double[numberOfTrainingDataPoints];
            for (var i = 0; i < numberOfTrainingDataPoints; ++i)
                distances[i] = FindDistances(unknownItem, _trainingData[i]);
            return distances;
        }

        /// <summary>
        /// Orders the distances
        /// </summary>
        /// <param name="numberOfTrainingDataPoints"></param>
        /// <param name="distances"></param>
        /// <returns></returns>
        private static int[] GetOrderedDistances(int numberOfTrainingDataPoints, double[] distances)
        {
            var orderedDistances = new int[numberOfTrainingDataPoints];
            for (var i = 0; i < numberOfTrainingDataPoints; ++i)
                orderedDistances[i] = i;
            var distancesCopy = new double[numberOfTrainingDataPoints];
            Array.Copy(distances, distancesCopy, distances.Length);
            Array.Sort(distancesCopy, orderedDistances);
            return orderedDistances;
        }

        /// <summary>
        /// A simple implementation of a voting mechanism
        /// for  the K nearest neighbour points to
        /// vote on the class of the unknown item
        /// </summary>
        /// <param name="numberOfClasses"></param>
        /// <param name="k"></param>
        /// <param name="kNearestDistance"></param>
        /// <param name="orderedDistances"></param>
        /// <returns></returns>
        private double[] GetNearestPointVotes(int numberOfClasses, int k, double[] kNearestDistance, int[] orderedDistances)
        {
            var votes = new double[numberOfClasses];
            var weights = CalculateWeightings(k, kNearestDistance);
            for (var i = 0; i < k; ++i)
            {
                var distance = orderedDistances[i];
                var predictedClass = (int)_trainingData[distance][4];
                votes[predictedClass] += weights[i] * 1.0;
            }

            return votes;
        }

        /// <summary>
        /// Finds the K nearest training data points
        /// </summary>
        /// <param name="k"></param>
        /// <param name="orderedDistances"></param>
        /// <param name="distances"></param>
        /// <returns></returns>
        private static double[] FindKNearestTrainingPoints(int k, IReadOnlyList<int> orderedDistances, IReadOnlyList<double> distances)
        {
            var kNearestDistance = new double[k];
            for (var i = 0; i < k; ++i)
            {
                var distance = orderedDistances[i];
                kNearestDistance[i] = distances[distance];
            }

            return kNearestDistance;
        }


        /// <summary>
        /// Calculates the weighting for each of the
        /// determined 
        /// </summary>
        /// <param name="k"></param>
        /// <param name="distances"></param>
        /// <returns></returns>
        private static double[] CalculateWeightings(int k, IReadOnlyList<double> distances)
        {
            var result = new double[k];
            var sum = 0.0;
            for (var i = 0; i < k; ++i)
            {
                result[i] = 1.0 / distances[i];
                sum += result[i];
            }
            for (var i = 0; i < k; ++i)
                result[i] /= sum;
            return result;
        }

        /// <summary>
        /// Finds the Euclidean distance between the unknown
        /// item and the training data points
        /// </summary>
        /// <param name="unknownItem"></param>
        /// <param name="trainingDataPoints"></param>
        /// <returns></returns>
        private static double FindDistances(IReadOnlyList<double> unknownItem, IReadOnlyList<double> trainingDataPoints)
        {
            var sum = 0.0;
            for (var i = 0; i < 3; ++i)
            {
                var difference = unknownItem[i] - trainingDataPoints[i + 1];
                sum += difference * difference;
            }
            return Math.Sqrt(sum);
        }
    }
}
