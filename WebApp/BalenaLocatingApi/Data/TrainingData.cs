using System;
namespace BalenaLocatingApi.Data
{
    public static class TrainingData
    {
        public static double[][] GetTrainingData()
        {
            var data = new double[21][];
            data[0] = new double[] { 1, -69, -80, -25, 0 };
            data[1] = new double[] { 2, -83, -74, -35, 0 };
            data[2] = new double[] { 3, -96, -50, -79, 2 };
            data[3] = new double[] { 4, -85, -63, -1001, 2 };
            data[4] = new double[] { 5, -54, -77, -84, 1 };
            data[5] = new double[] { 6, -80, -47, -92, 2 };
            data[6] = new double[] { 7, -62, -65, -1001, 2 };
            data[7] = new double[] { 8, -1001, -79, -82, 1 };
            data[8] = new double[] { 9, -1001, -48, -91, 2 };
            data[9] = new double[] { 10, -82, -88, -36, 0 };
            data[10] = new double[] { 11, -65, -88, -34, 0 };
            data[11] = new double[] { 12, -54, -1001, -66, 1 };
            data[12] = new double[] { 13, -79, -49, -1001, 2 };
            data[13] = new double[] { 14, -70, -89, -81, 1 };
            data[14] = new double[] { 15, -75, -79, -34, 0 };
            data[15] = new double[] { 16, -53, -1001, -69, 1 };
            data[16] = new double[] { 17, -54, -82, -80, 1 };
            data[17] = new double[] { 18, -58, -81, -69, 1 };
            data[18] = new double[] { 19, -82, -1001, -34, 0 };
            data[19] = new double[] { 20, -63, -49, -1001, 2 };
            data[20] = new double[] { 21, -69, -1001, -25, 0 };

            return data;
        }
    }
}
