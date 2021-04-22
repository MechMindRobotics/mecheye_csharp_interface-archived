using System;
using OpenCvSharp;


namespace Mechmind_CameraAPI_Csharp
{
    class sample  
    {
        static void Main()
        {
            CameraClient camera = new CameraClient();
            //camera ip should be modified to actual ip address
            //always set ip before do anything else
            camera.connect("192.168.3.168");

            //get some camera info like intrincis, ip, id and version
            double[] intri = camera.getCameraIntri(); //[fx,fy,u,v]

            Console.WriteLine("Camera ID: " + camera.getCameraId());
            Console.WriteLine("Version: " + camera.getCameraVersion());
            int[] colorImgSize = camera.getColorImgSize();
            int[] depthImgSize = camera.getDepthImgSize();
            Console.WriteLine("Color Image Size: {0} * {1}", colorImgSize[0], colorImgSize[1]);
            Console.WriteLine("Depth Image Size: {0} * {1}", depthImgSize[0], depthImgSize[1]);

            string save_path = "D:\\";
            //capture color image and depth image and save them
            Mat color = camera.captureColorImg();
            Mat depth = camera.captureDepthImg();

            if (color.Empty() || depth.Empty())
            {
                Console.WriteLine("Empty images");
            }
            else
            {
                Cv2.ImWrite(save_path + "color.jpg", color);
                Cv2.ImWrite(save_path + "depth.png", depth);
            }
            double[,] rel = camera.captureRGBCloud();//point cloud data in xyzrgb3
            Console.WriteLine("Cloud has " + rel.Length.ToString() + " points");
            //set some parameters of camera, you can refer to parameters' names in Mech_eye
            Console.WriteLine(camera.setParameter("scan2dExposureMode",0)); //set exposure mode to timed
            Console.WriteLine(camera.getParameter("scan2dExposureMode"));
            Console.WriteLine(camera.setParameter("scan2dExposureTime",20)); //set expsosure time to 20ms
            Console.WriteLine(camera.getParameter("scan2dExposureTime"));
        }
        
    }
}
