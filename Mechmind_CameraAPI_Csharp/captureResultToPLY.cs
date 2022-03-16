using System;
using Emgu.CV;

namespace Mechmind_CameraAPI_Csharp
{
    class captureResultToPLY
    {
        static void printDeviceInfo(in string cameraID, in string cameraVersion)
        {
            Console.WriteLine("............................");
            Console.WriteLine("Camera ID:         "+cameraID);
            Console.WriteLine("Firmware Version:  "+cameraVersion);
            Console.WriteLine("............................");
            Console.WriteLine("");
        }

        public static void capture(CameraClient camera, string save_path)
        {
            Mat pointXYZMap = camera.captureCloud();
            CvInvoke.WriteCloud(save_path + "pointCloudXYZ.ply", pointXYZMap);
            Console.WriteLine("PointCloudXYZ has : {0} data points.", pointXYZMap.Width * pointXYZMap.Height);

            Mat colorMap = camera.captureColorImg();
            CvInvoke.WriteCloud(save_path + "pointCloudXYZRGB.ply", pointXYZMap, colorMap);
            Console.WriteLine("PointCloudXYZRGB has : {0} data points.", pointXYZMap.Width, pointXYZMap.Height);
        }
    }
}