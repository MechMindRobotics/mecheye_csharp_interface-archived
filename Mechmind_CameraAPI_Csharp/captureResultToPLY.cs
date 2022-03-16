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

        static void Main()
        {
            CameraClient camera = new CameraClient();
            string ip = Console.ReadLine();

            if (Status.Error == camera.connect(ip)) return;

            string cameraID = camera.getCameraId();
            string cameraVersion = camera.getCameraVersion();
            printDeviceInfo(cameraID, cameraVersion);

            string pointCloudPath = "pointCloudXYZ.ply";
            Mat pointXYZMap = camera.captureCloud();
            CvInvoke.WriteCloud(pointCloudPath, pointXYZMap);

            string pointCloudColorPath = "pointCloudXYZRGB.ply";
            Mat colorMap = camera.captureColorImg();
            CvInvoke.WriteCloud(pointCloudColorPath, pointXYZMap, colorMap);
        }
    }
}