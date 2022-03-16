using System;
using Emgu.CV;
using Emgu.CV.CvEnum;


namespace Mechmind_CameraAPI_Csharp
{
    class sample  
    {
        static void printDeviceInfo(in string cameraID, in string cameraVersion)
        {
            Console.WriteLine("............................");
            Console.WriteLine("Camera ID:         " + cameraID);
            Console.WriteLine("Firmware Version:  " + cameraVersion);
            Console.WriteLine("............................");
            Console.WriteLine("");
        }
        static void Main()
        {
            CameraClient camera = new CameraClient();
            Console.WriteLine("Enter Camera IP: ");
            string ip = Console.ReadLine();

            if (Status.Error == camera.connect(ip)) return;

            string cameraID = camera.getCameraId();
            string cameraVersion = camera.getCameraVersion();
            printDeviceInfo(cameraID, cameraVersion);

            //Get some camera information like intrincis, IP, ID and version.
            double[] intri = camera.getCameraIntri(); //[fx,fy,u,v]

            Console.WriteLine("Camera ID: " + camera.getCameraId());
            Console.WriteLine("Version: " + camera.getCameraVersion());
            int[] colorImgSize = camera.getColorImgSize();
            int[] depthImgSize = camera.getDepthImgSize();
            Console.WriteLine("Color Image Size: {0} * {1}", colorImgSize[0], colorImgSize[1]);
            Console.WriteLine("Depth Image Size: {0} * {1}", depthImgSize[0], depthImgSize[1]);

            string save_path = "D:\\";
            //Capture the color image and depth image and save them.
            Mat color = camera.captureColorImg();
            Mat depth = camera.captureDepthImg();

            if (color == null || depth == null)
            {
                Console.WriteLine("Empty images");
                return;
            }
            else
            {
                CvInvoke.Imwrite(save_path + "color.jpg", color);
                CvInvoke.Imwrite(save_path + "depth.tif", depth);
            }

            captureResultToPLY.capture(camera, save_path);

            //Set some parameters of camera which you can refer to parameters' names in Mech_Eye Viewer.
            Console.WriteLine(camera.setParameter("scan2dExposureMode", 0)); //Set exposure mode to timed.
            Console.WriteLine(camera.getParameter("scan2dExposureMode"));
            Console.WriteLine(camera.setParameter("scan2dExposureTime", 20)); //Set expsosure time to 20ms.
            Console.WriteLine(camera.getParameter("scan2dExposureTime"));

            int[] roi = { 500, 500, 100, 100 }; // roi: height, width, X, Y
            Console.WriteLine(camera.setParameter("roi", roi));
            Console.WriteLine(camera.getParameter("roi"));
        }
    }
}
