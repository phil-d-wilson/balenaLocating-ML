using System;
using System.Collections.Generic;
using System.Linq;
using BalenaLocatingApi.Data;

namespace BalenaLocatingApi.Services
{
    public class ClassificationService
    {
        private readonly double[][] _trainingData;

        public ClassificationService()
        {
            _trainingData = TrainingData.GetTrainingData();
        }

        public int Analyze(IReadOnlyList<double> item)
        {
            const int k = 3;
            const int numberOfClasses = 3;

            var dataLength = _trainingData.Length;
            var distances = new double[dataLength];
            for (var i = 0; i < dataLength; ++i)
                distances[i] = FindDistances(item, _trainingData[i]);

            var orderedDistances = new int[dataLength];
            for (var i = 0; i < dataLength; ++i)
                orderedDistances[i] = i;
            var distancesCopy = new double[dataLength];
            Array.Copy(distances, distancesCopy, distances.Length);
            Array.Sort(distancesCopy, orderedDistances);

            var kNearestDistance = new double[k];
            for (var i = 0; i < k; ++i)
            {
                var index = orderedDistances[i];
                kNearestDistance[i] = distances[index];
            }

            var votes = new double[numberOfClasses];
            var wts = MakeWeights(k, kNearestDistance);
            for (var i = 0; i < k; ++i)
            {
                var idx = orderedDistances[i];
                var predictedClass = (int)_trainingData[idx][4];
                votes[predictedClass] += wts[i] * 1.0;
            }

            return votes.ToList().IndexOf(votes.Max());
        }

        private static double[] MakeWeights(int k, IReadOnlyList<double> distances)
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

        private static double FindDistances(IReadOnlyList<double> item, IReadOnlyList<double> dataPoint)
        {
            var sum = 0.0;
            for (var i = 0; i < 3; ++i)
            {
                var diff = item[i] - dataPoint[i + 1];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }
    }
}
