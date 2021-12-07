using System;
using OpenCvSharp;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Mechmind_CameraAPI_Csharp
{
    static class Constant
    {
        public const int TIMEOUTMS = 10000;
        public const string UNSUPPORTEDCOMMENDMSG = "Unsupported command.";
    }
    enum Status
    {
        Error,
        Success
    }
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
        public Status connect(string ip)
        {
            if(Status.Success == setAddr(ip) && getCameraIntri() != null)
            {               
                Console.WriteLine("Successfully connected to this camera!");
                return Status.Success;
            }
            Console.WriteLine("Failed to connect the camera! Please confirm whether the IP address to be connected is correct!");     
            return Status.Error;
        }
        byte[] sendRequest(string command, double value = 0, string propertyName = "", int image_type = 0)
        {
            JObject request = new JObject();
            request.Add(Service.cmd, command);
            request.Add(Service.property_name, propertyName);
            request.Add(Service.property_value, value);
            request.Add(Service.image_type, image_type);
            byte[] reply = sendReq(request.ToString());
            if (null == reply)
            {
                System.Threading.Thread.Sleep(Constant.TIMEOUTMS);
            }
            return reply;
        }
        private bool isResponseValid(string info)
        {
            if (info.Contains(Constant.UNSUPPORTEDCOMMENDMSG))
            {
                Console.WriteLine("{0}. Please check the command.", Constant.UNSUPPORTEDCOMMENDMSG);
                return false;
            }
            return true;
        }
        public JToken getCameraInfo()
        {
            byte[] reply = sendRequest(Command.GetCameraInfo);
            if (null == reply)
            {
                Console.WriteLine("Failed to get camera infomation!");
                return null;
            }
            string info = System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray());
            return isResponseValid(info) ? JObject.Parse(info)["camera_info"] : null;
        }
        public string getCameraId()
        {
            JToken info = getCameraInfo();
            return info != null ? info["eyeId"].ToString() : null;
        }
        public string getCameraVersion()
        {
            JToken info = getCameraInfo();
            return info != null ? info["version"].ToString() : null;
        }
        public double getParameter(string paraname)
        {
            JObject request = new JObject();
            request.Add(Service.cmd, Command.GetCameraParams);
            request.Add(Service.property_name, paraname);
            byte[] reply = sendReq(request.ToString());
            if (null == reply)
            {
                Console.WriteLine("Failed to get {0} parameter!", paraname);
                return -1;
            }
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            JToken allConfigs = info["camera_config"]["configs"][0];
            if (allConfigs[paraname] == null) {
                Console.WriteLine("Property {0} not exist!", paraname);
                return -1;
            }
            return double.Parse(allConfigs[paraname].ToString());
        }
        public string setParameter(string paraname, double value)
        {
            JObject request = new JObject();
            request.Add(Service.cmd, Command.SetCameraParams);
            JObject objectToSet = new JObject();
            objectToSet.Add(paraname, value);
            request.Add(Service.camera_config, objectToSet);
            request.Add(Service.persistent, true);
            byte[] reply = sendReq(request.ToString());
            string errString = "Failed to set paraname " + paraname + ".";
            if (null == reply)
            {
                return errString;
            }
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            if(info.ContainsKey("err_msg") && info.Value<string>("err_msg") != "")
            {
                return errString + info["err_msg"]; 
            }
            return "Set " + paraname + " successfully!";
        }
        public double[] getCameraIntri()
        {
            byte[] reply = sendRequest(Command.GetCameraIntri);
            if (null == reply) 
            {
                Console.WriteLine("Failed to get camera intrinsics!");
                return null;
            }
            string info = System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray());
            if (!isResponseValid(info)) return null;
            string intriOriginal = JObject.Parse(info)["camera_intri"]["intrinsic"].ToString();
            int start = intriOriginal.LastIndexOf('[');
            int end = intriOriginal.LastIndexOf(']');
            int length = intriOriginal.Length;
            if (start == -1 || end == -1 || end < start)
            {
                Console.WriteLine("Wrong camera intrinsics!");
                return null;
            }
            string intriStr = intriOriginal.Remove(0, start + 1).Substring(0, end - start - 1);
            string[] intriStrVec = intriStr.Split(',');
            if (intriStrVec.Length != 4)
            {
                Console.WriteLine("Wrong intrinscis value");
                return null;
            }
            CameraIntri intri = new CameraIntri();
            intri.setValue(double.Parse(intriStrVec[0]),
                double.Parse(intriStrVec[1]),
                double.Parse(intriStrVec[2]),
                double.Parse(intriStrVec[3])
                );
            double[] rel = intri.getValue();
            return rel;
        }
        public Mat captureColorImg()
        {
            byte[] reply = sendRequest(Command.CaptureImage, 0, "", COLOR);
            if (null == reply)
            {
                Console.WriteLine("Client depth image is empty!");
                return null;
            }
            int jsonSize = readInt(reply, 0);
            int imageSize = readInt(reply, SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE);
            int imageBegin = SIZE_OF_JSON + jsonSize + SIZE_OF_SCALE + sizeof(Int32);
            byte[] imageRGB = reply.Skip(imageBegin).Take(imageSize).ToArray();
            if (imageRGB.Length == 0)
            {
                Console.WriteLine("Client color image is empty!");
                return null;
            }
            Console.WriteLine("Color image captured!");
            Mat img = asMat(imageRGB);
            return Cv2.ImDecode(img, ImreadModes.Color);
        }
        public Mat captureDepthImg()
        {
            byte[] response = sendRequest(Command.CaptureImage, 0, "", DEPTH);
            if (null == response) 
            {
                Console.WriteLine("Client depth image is empty!"); 
                return null; 
            }
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
            if (null == data_bs) return 0;
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
            if (null == data_bs) return 0;
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
            if (null == response)
            {
                Console.WriteLine("Client MatXYZ image is empty!");
                return null;
            }
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
            if (null == reply)
            {
                Console.WriteLine("Failed to get image size!");
                return null;
            }
            JObject info = JObject.Parse(System.Text.Encoding.Default.GetString(reply.Skip(SIZE_OF_JSON).ToArray()));
            return (JObject)info[Service.image_format];
        }
        public int[] getColorImgSize()
        {
            JObject imageSizeObject = getImgSize();
            if (null == imageSizeObject) return null;
            JArray size2d = (JArray)(imageSizeObject[Service.size2d]);
            int[] res = { (int)size2d[0], (int)size2d[1] };
            return res;
        }
        public int[] getDepthImgSize()
        {
            JObject imageSizeObject = getImgSize();
            if (null == imageSizeObject) return null;
            JArray size3d =(JArray)imageSizeObject[Service.size3d];
            int[] res = { (int)size3d[0], (int)size3d[1] };
            return res;
        }
    }
}
