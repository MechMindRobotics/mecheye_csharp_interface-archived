using System;
using OpenCvSharp;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Mechmind_CameraAPI_Csharp
{

    class CameraIntri
    { 
        double __fx = 0.0;
        double __fy = 0.0;
        double __u = 0.0;
        double __v = 0.0;

        public bool isZero()
        {
            return __fx == 0.0 && __fy == 0.0 && __u == 0.0 && __v == 0.0;
        }
        public void setValue(double fx, double fy, double u, double v)
        {
            __fx = fx;
            __fy = fy;
            __u = u;
            __v = v;
        }
        public double[] getValue()
        {
            double[] a = { __fx, __fy, __u, __v };
            return a;
        }
        
    }
    static class Service
    {
        public const string cmd = "cmd";
        public const string property_name = "property_name";
        public const string property_value = "property_value";
        public const string image_type = "image_type";
        public const string persistent = "persistent";
        public const string camera_config = "camera_config";
        public const string image_format = "image_format";
        public const string size2d = "imageSize2D";
        public const string size3d = "imageSize3D";
    }
    static class Command
    {
        public const string CaptureImage = "CaptureImage";
        public const string GetCameraIntri = "GetCameraIntri";
        public const string GetCameraId = "GetCameraId";
        public const string GetCameraInfo = "GetCameraInfo";
        public const string GetCamera2dInfo = "GetCamera2dInfo";
        public const string GetServerInfo = "GetServerInfo";
        public const string SetCameraParams = "SetCameraConfig";
        public const string GetCameraParams = "GetCameraConfig";
        public const string GetImageFormat = "GetImageFormat";
    }
    class CameraClient:ZmqClient
    {
        const int DEPTH = 1;
        const int COLOR = 2;
        const int MatXYZ = 16;
        const int Encode32FBias = 32768;
        const int SIZE_OF_JSON = 4;
        const int SIZE_OF_SCALE = 8;
        public CameraClient() : base()
        { }
        public int connect(string ip)
        {
            return setAddr(ip);
        }
        byte[] sendRequest(string command, double value = 0, string propertyName = "", int image_type = 0)
        {
            JObject request = new JObject();
            request.Add(Service.cmd, command);
            request.Add(Service.property_name, propertyName);
            request.Add(Service.property_value, value);
            request.Add(Service.image_type, image_type);
            byte[] reply = sendReq(request.ToString());
            return reply;
        }
        public JToken getCameraInfo()
        {
            byte[] reply = sendRequest(Command.GetCameraInfo);
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            return info["camera_info"];
        }
        public string getCameraId()
        {
            return getCameraInfo()["eyeId"].ToString();
        }
        public string getCameraVersion()
        {
            return getCameraInfo()["version"].ToString();
        }
        public double getParameter(string paraname)
        {
            JObject request = new JObject();
            request.Add(Service.cmd, Command.GetCameraParams);
            request.Add(Service.property_name, paraname);
            byte[] reply = sendReq(request.ToString());
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            JToken allConfigs = info["camera_config"]["configs"][0];
            if (allConfigs[paraname] == null) {
                Console.WriteLine("Property " + paraname +" not exist!");
                return -1;
            }
            return double.Parse(allConfigs[paraname].ToString());
        }
        public string setParameter(string paraname, double value)
        {
            JObject request = new JObject();
            request.Add(Service.cmd, Command.SetCameraParams);
            JObject tmp = new JObject();
            tmp.Add(paraname, value);
            request.Add(Service.camera_config,tmp);
            request.Add(Service.persistent, "false");
            byte[] reply = sendReq(request.ToString());
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            if (info["err_msg"] != null)
                Console.Write(info["err_msg"]);
            return "";
        }
        public double[] getCameraIntri()
        {
            byte[] reply = sendRequest(Command.GetCameraIntri);
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            string intri_original = info["camera_intri"]["intrinsic"].ToString();
            int start = intri_original.LastIndexOf('[');
            int end = intri_original.LastIndexOf(']');
            int length = intri_original.Length;
            if (start == -1 || end == -1 || end < start)
            {
                Console.WriteLine("Wrong camera intrinsics");
                return null;
            }
            string intri_str = intri_original.Remove(0, start + 1).Substring(0, end - start - 1);
            string[] intrivalue_str = intri_str.Split(',');
            if (intrivalue_str.Length != 4)
            {
                Console.WriteLine("Wrong intrinscis value");
                return null;
            }
            CameraIntri intri = new CameraIntri();
            intri.setValue(double.Parse(intrivalue_str[0]),
                double.Parse(intrivalue_str[1]),
                double.Parse(intrivalue_str[2]),
                double.Parse(intrivalue_str[3])
                );
            double[] rel = intri.getValue();
            return rel;
        }
        public Mat captureColorImg()
        {
            byte[] reply = sendRequest(Command.CaptureImage, 0, "", COLOR);
            int jsonSize = readInt(reply, 0);
            int imageSize = readInt(reply, SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE);
            int imageBegin = SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE + sizeof(Int32);
            byte[] imageRGB = reply.Skip(imageBegin).Take(imageSize).ToArray();
            if (imageRGB.Length == 0)
            {
                Console.WriteLine("Client depth image is empty!");
                return null;
            }
            Console.WriteLine("Color image captured!");
            Mat img = asMat(imageRGB);
            return Cv2.ImDecode(img, ImreadModes.Color);

        }
        public Mat captureDepthImg()
        {
            byte[] response = sendRequest(Command.CaptureImage, 0, "", DEPTH);
            int jsonSize = readInt(response, 0);
            double scale = readDouble(response, jsonSize + SIZE_OF_JSON);
            int imageSize = readInt(response, SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE);
            int imageBegin = SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE + sizeof(Int32);
            byte[] imageDepth = response.Skip(imageBegin).Take(imageSize).ToArray();
            if (imageDepth.Length == 0)
            {
                Console.WriteLine("Client depth image is empty!");
                return null;
            }
            Console.WriteLine("Depth image captured!");
            return read32FC1Mat(imageDepth,scale);
        }
        Mat read32FC1Mat(byte[] data, double scale)
        {
            if (data.Length == 0) return null;
            Mat bias16U = Cv2.ImDecode(asMat(data), ImreadModes.AnyDepth);
            Mat bias32F = Mat.Zeros(bias16U.Size(), MatType.CV_32FC1);
            bias16U.ConvertTo(bias32F, MatType.CV_32FC1);
            Mat mat32F = bias32F + new Mat(bias32F.Size(), bias32F.Type(), Scalar.All(-Encode32FBias));

            if (scale == 0)
                return new Mat();
            else
                return mat32F / scale;
        }
        Mat asMat(byte[] imgRGB,int offset = 0)
        {
            int i = offset;
            Mat img = new Mat();
            for (; i < imgRGB.Length; i++)
            {
                img.PushBack((byte)imgRGB[i]);
            }
            return img;
        }
        double readDouble(byte[] data_bs, int pos)
        {   
            if (pos + sizeof(double) > data_bs.Length)
            {
                return 0;
            }
            byte[] str = new byte[sizeof(double)];
            int j = 0;
            for (int i = sizeof(double)+pos-1; i >= pos; i--)
            {
                str[j] = data_bs[i];
                j++;
            }
            str.Reverse();
            double v = BitConverter.ToDouble(str, 0);
            return v;
        }
        int readInt(byte[] data_bs, int pos)
        {
            if (pos + sizeof(Int32) > data_bs.Length)
            {
                return 0;
            }
            byte[] str = new byte[sizeof(Int32)];
            int j = 0;
            for (int i = sizeof(Int32) + pos - 1; i >= pos; i--)
            {
                str[j] = data_bs[i];
                j++;
            }
            str.Reverse();
            int v = BitConverter.ToInt32(str, 0);
            return v;
        }

        Mat read32FC3Mat(byte[] data, double scale)
        {
            if (data.Length == 0) return null;
            Mat matC1 = Cv2.ImDecode(asMat(data), ImreadModes.AnyDepth);
            Mat bias16UC3 = matC1ToC3(matC1);
            Mat bias32F = Mat.Zeros(bias16UC3.Size(), MatType.CV_32FC3);
            bias16UC3.ConvertTo(bias32F, MatType.CV_32FC3);
            Mat mat32F = bias32F + new Mat(bias32F.Size(), bias32F.Type(), Scalar.All(-Encode32FBias));
            Mat depth32F = mat32F / scale;
            return depth32F;
        }
        Mat matC1ToC3(Mat matC1)
        {
            if (matC1.Empty()) return new Mat();
            if (matC1.Channels() != 1 || (matC1.Rows % 3) != 0)
                return new Mat();
            Mat[] channels = new Mat[3];
            int rows = matC1.Rows;
            int cols = matC1.Cols;
            channels[0] = (matC1[0, (int)rows / 3, 0, cols]);
            channels[1] = (matC1[(int)rows / 3, (int)(2 * rows / 3), 0, cols]);
            channels[2] = (matC1[(int)(2 * rows / 3), rows, 0, cols]);
            Mat rel = new Mat();
            Cv2.Merge(channels, rel);
            return rel;
        }
        public double[,] captureRGBCloud()
        {
            byte[] response = sendRequest(Command.CaptureImage, 0, "", MatXYZ);
            int jsonSize = readInt(response, 0);
            double scale = readDouble(response, jsonSize + SIZE_OF_JSON);
            int imageSize = readInt(response, SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE);
            int imageBegin = SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE + sizeof(Int32);
            byte[] imageDepth = response.Skip(imageBegin).Take(imageSize).ToArray();
            Mat depthC3 = read32FC3Mat(imageDepth, scale);
            Mat color = captureColorImg();
            int nums = color.Rows * color.Cols;
            double[,] xyzbgr = new double[nums, 6];
            int count = 0;
            for (int i = 0; i < depthC3.Rows; i++)
                for (int j = 0; j < depthC3.Cols; j++)
                {
                    xyzbgr[count, 0] = (double)depthC3.At<Vec3f>(i, j)[0] * 0.001;
                    xyzbgr[count, 1] = (double)depthC3.At<Vec3f>(i, j)[1] * 0.001;
                    xyzbgr[count, 2] = (double)depthC3.At<Vec3f>(i, j)[2] * 0.001;
                    xyzbgr[count, 3] = color.At<Vec3b>(i, j)[0];
                    xyzbgr[count, 4] = color.At<Vec3b>(i, j)[1];
                    xyzbgr[count, 5] = color.At<Vec3b>(i, j)[2];
                    count++;

                }

            return xyzbgr;
        }
        JObject getImgSize()
        {
            byte[] reply = sendRequest(Command.GetImageFormat);
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            return (JObject)info[Service.image_format];
        }
        public int[] getColorImgSize()
        {
            JArray size2d = (JArray)getImgSize()[Service.size2d];
            int[] res = { (int)size2d[0], (int)size2d[1] };
            return res;
        }
        public int[] getDepthImgSize()
        {
            JArray size3d = (JArray)getImgSize()[Service.size3d];
            int[] res = { (int)size3d[0], (int)size3d[1] };
            return res;
        }
    }
}
