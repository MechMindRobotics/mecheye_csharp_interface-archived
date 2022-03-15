using System;
using OpenCvSharp;
using PclSharp;
using PclSharp.IO;

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

        static void savePLY(in Mat pointXYZMap, in string path)
        {
            PointCloudOfXYZ pointCloud(pointXYZMap.Width, pointXYZMap.Height);

            for (int i = 0; i < pointXYZMap.Height; ++i)
            {
                for (int j = 0; j < pointXYZMap.Width; ++j)
                {
                    pointCloud.At(i, j).X = pointXYZMap.Get<Vec3f>(i, j).Item0;
                    pointCloud.At(i, j).Y = pointXYZMap.Get<Vec3f>(i, j).Item1;
                    pointCloud.At(i, j).Z = pointXYZMap.Get<Vec3f>(i, j).Item2;
                }
            }

            PCDWriter writer;
            writer.write(path, pointCloud);
            Console.WriteLine("PointCloudXYZ has : {0} data points.", pointCloud.width * pointCloud.height);
        }

        static void savePLY(in Mat pointXYZMap, in Mat colorMap, in string path)
        {
            PointCloudOfXYZRGBA pointCloud(pointXYZMap.Width, pointXYZMap.Height);

            for (int i = 0; i < pointXYZMap.Height; ++i)
            {
                for (int j = 0; j < pointXYZMap.Width; ++j)
                {
                    pointCloud.At(i, j).X = pointXYZMap.Get<Vec3f>(i, j).Item0;
                    pointCloud.At(i, j).Y = pointXYZMap.Get<Vec3f>(i, j).Item1;
                    pointCloud.At(i, j).Z = pointXYZMap.Get<Vec3f>(i, j).Item2;
                    int r = pointXYZMap.Get<Vec3b>(i, j).Item0;
                    int g = pointXYZMap.Get<Vec3b>(i, j).Item1;
                    int b = pointXYZMap.Get<Vec3b>(i, j).Item2;
                    pointCloud.At(i, j).rgba = (r << 16 | g << 8 | b);
                }
            }

            PCDWriter writer;
            writer.write(path, pointCloud);
            Console.WriteLine("PointCloudXYZRGB has : {0} data points.", pointCloud.width * pointCloud.height);
        }

        static void Main()
        {
            CameraClient camera = new CameraClient();
            string ip = Console.ReadLine();

            if (Status.Error == camera.connect(ip)) return;

            string cameraID = camera.getCameraId();
            string cameraVersion = camera.getCameraVersion();
            printDeviceInfo(cameraID, cameraVersion);

            string pointCloudPath = "pointCloudXYZ.pcd";
            Mat pointXYZMap = camera.captureCloud();
            savePLY(pointXYZMap, pointCloudPath);

            string pointCloudColorPath = "pointCloudXYZRGB.pcd";
            Mat colorMap = camera.captureColorImg();
            savePLY(pointXYZMap, colorMap, pointCloudColorPath);
        }
    }
}