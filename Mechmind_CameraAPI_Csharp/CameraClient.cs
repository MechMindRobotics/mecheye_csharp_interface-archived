using Mmind;
using System;
using System.Text;
using OpenCvSharp;
using Google.Protobuf;
using System.Linq;


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
    class CameraClient:ZmqClient
    {
        const int DEPTH = 1;
        const int COLOR = 2;
        const int COLOR_DEPTH = COLOR | DEPTH;
        const int CaptureImage = 0;
        const int GetCameraIntri = 11;
        const int GetCameraStatus = 19;
        const int SetCameraParameter = 25;
        const int GetCameraParameter = 26;
        const int Encode32FBias = 32768;
        const int CaptureGratingImage = 8;
        public CameraClient() : base()
        { }
        public int connect(string ip)
        {
            return setAddr(ip);
        }
        Mmind.Response sendRequest(int command, double value_num, string value_str)
        {
            Mmind.Request Request = new Mmind.Request();
            Request.Command = command;
            Request.ValueDouble = value_num;
            Request.ValueString = value_str;
            Mmind.Response reply = sendReq(Request);
            return reply;
        }
        Mmind.CameraStatus getCameraStatus()
        {
            Mmind.Response reply = sendRequest(GetCameraStatus, 0.0, "");
            string StatusUnicode = reply.CameraStatus;
            byte[] StatusBytes = Encoding.UTF8.GetBytes(StatusUnicode);
            Mmind.CameraStatus cmstatus = new CameraStatus();
            cmstatus = Mmind.CameraStatus.Parser.ParseFrom(StatusBytes);
            return cmstatus;
        }
        public string getCameraId()
        {
            return getCameraStatus().EyeId;
        }
        public string getCameraIp()
        {
            return getCameraStatus().Ip;
        }
        public string getCameraVersion()
        {
            return getCameraStatus().Version;
        }
        public double getParameter(string paraname)
        {
            Mmind.Response reply = sendRequest(GetCameraParameter, 0.0, paraname);
            return double.Parse(reply.ParameterValue);
        }
        public string setParameter(string paraname, double value)
        {
            Mmind.Response error = sendRequest(SetCameraParameter, value, paraname);
            return error.Error;
        }
        public double[] getCameraIntri()
        {
            Mmind.Response reply = sendRequest(GetCameraIntri, 0.0, "");
            string intri_original = reply.CamIntri;
            int start = intri_original.LastIndexOf('[');
            int end = intri_original.LastIndexOf(']');
            int length = intri_original.Length;
            if (start == -1 || end == -1 || end < start)
            {
                Console.WriteLine("Wrong camera intrinsics");
                return null;
            }   
            string intri_str = intri_original.Remove(0,start+1).Substring(0,end-start-1);
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
            Mmind.Response reply = sendRequest(CaptureImage, COLOR, "");
            if (reply.ImageRGB.Length == 0)
            {
                Console.WriteLine("Client depth image is empty!");
                return null;
            }
            Console.WriteLine("Color image captured!");
            Mat img = asMat(reply.ImageRGB);
            return Cv2.ImDecode(img,ImreadModes.Color);

        }
        public Mat captureDepthImg() 
        {
            Mmind.Response reply = sendRequest(CaptureImage, DEPTH,"");
            if (reply.ImageDepth.Length == 0)
            {
                Console.WriteLine("Client depth image is empty!");
                return null;
            }
            Console.WriteLine("Depth image captured!");
            return read32FC1Mat(reply.ImageDepth, 2);
        }
        Mat read32FC1Mat(ByteString data, int offset = 0)
        {
            if (data.Length == 0) return null;
            double scale = readDouble(data, offset);
            Mat bias16U = Cv2.ImDecode(asMat(data, sizeof(double) + offset), ImreadModes.AnyDepth);
            Mat bias32F = Mat.Zeros(bias16U.Size(), MatType.CV_32FC1);
            bias16U.ConvertTo(bias32F, MatType.CV_32FC1);
            Mat mat32F = bias32F + new Mat(bias32F.Size(), bias32F.Type(), Scalar.All(-Encode32FBias));

            if (scale == 0)
                return new Mat();
            else
                return mat32F / scale;
        }
        Mat asMat(ByteString imgRGB,int offset = 0)
        {
            int i = offset;
            Mat img = new Mat();
            for (; i < imgRGB.Length; i++)
            {
                img.Add((byte)imgRGB[i]);
            }
            return img;
        }
        double readDouble(ByteString data_bs, int pos)
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
            //Console.WriteLine(v.ToString());
            return v;
        }

        Mat read32FC3Mat(ByteString data)
        {
            if (data.Length == 0) return null;
            double scale = readDouble(data, 0);
            Mat matC1 = Cv2.ImDecode(asMat(data, sizeof(double)), ImreadModes.AnyDepth);
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
            Mmind.Response reply = sendRequest(CaptureGratingImage, 4, "");
            Mat depthC3 = read32FC3Mat(reply.ImageGrating);
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
        int find_last_char(string s,char c)
        {
            int last = s.Length - 1;
            for (int i = last; i >= 0; i--)
            {
                if (s[i] == c) return i;
            }
            return -1;
        }
    }
}
