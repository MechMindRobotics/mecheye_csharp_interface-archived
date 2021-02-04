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
            camera.connect("192.168.3.76");
 
            //get some camera info like intrincis, ip, id and version
            double[] intri = camera.getCameraIntri(); //[fx,fy,u,v]
       
            Console.WriteLine("Camera IP: " + camera.getCameraIp());
            Console.WriteLine("Camera ID: " + camera.getCameraId());
            Console.WriteLine("Version: " + camera.getCameraVersion());

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
            Console.WriteLine(camera.setParameter("camera2DExpTime", 40));
            Console.WriteLine(camera.getParameter("camera2DExpTime"));
            //The following is all parameters you can set.
            Console.WriteLine(camera.setParameter("period", 20));
            Console.WriteLine(camera.setParameter("isNanoType", 0));
            Console.WriteLine(camera.setParameter("lightPower", 300));
            Console.WriteLine(camera.setParameter("syncExposure", 0));
            Console.WriteLine(camera.setParameter("exposure1", 0.3));
            Console.WriteLine(camera.setParameter("exposure2", 6));
            Console.WriteLine(camera.setParameter("exposure3", 6));
            Console.WriteLine(camera.setParameter("gain", 0));
            Console.WriteLine(camera.setParameter("useBinning", 0));
            Console.WriteLine(camera.setParameter("useColorHdr", 0));
            Console.WriteLine(camera.setParameter("camera2DExpTime", 40));
            Console.WriteLine(camera.setParameter("expectedGrayValue", 120));
            Console.WriteLine(camera.setParameter("sharpenFactor", 0));
            Console.WriteLine(camera.setParameter("contrastThres", 10));
            Console.WriteLine(camera.setParameter("strength", 5));
            Console.WriteLine(camera.setParameter("useMedianBlur", 1));
            Console.WriteLine(camera.setParameter("hasThinObject", 0));
            Console.WriteLine(camera.setParameter("lowerLimit", 800));
            Console.WriteLine(camera.setParameter("upperLimit", 1100));

            //get point cloud in a 2-dim array,each element is [x,y,z,b,g,r]
            
        }
        
    }
}
