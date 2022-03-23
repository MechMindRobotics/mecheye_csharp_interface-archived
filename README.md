# This repository is archived.

## It's highly recommended to use the latest version of [Mech-Eye C# API](https://github.com/MechMindRobotics/mecheye_csharp_samples).


Please select the proper branch corresponding to the camera firmware version. 

If the camera firmware version is greater than or equal to 1.5.0, please use the latest version of [Mech-Eye C# API](https://github.com/MechMindRobotics/mecheye_csharp_samples).

## Features

By using interfaces , you can easily control your mech_eye cameras in .Net programs. The features of interfaces are as follows:

* Connect to your cameras in your LANS.
* Set and get camera parameters like exposure time, period and so on.
* Get color image and depth image as matrix.
* Get point cloud data in the format of a two-dimensional array.

## Dependency

These environments are needed :

* Visual Studio (2019 is recommanded)
* .Net Core (Version 3.1 is recommanded)

These packages are needed:

* Newtonsoft.Json
* NetMQ
* OpenCvSharp4 
* OpenCvSharp4.runtime.win

All these you can install with Nuget.

### Installation

1. We recommand VS 2019 to compile the project. Make sure .Net is also installed.

   If not, you can open VS and then go to: Tools ->Get Tools and features and then, in Workloads tag, choose ".Net Desktop Development" and install it.

2. Clone the repo and open .sln file with VS.

3. Right CIick on the "Mechmind_cameraAPI_Csharp" solution and click "Manage Nuget packages".

4. Then Click the "Browse" tag and search "NetMQ" and install it.

   The 3 other packages can also be installed in this way.

## Quick Start

After finish installation above, open code file **sample.cs** and modify the IP address in line 14 to your actual camera address.

Press Ctrl+F5 to run.

The sample.cs will be compiled and run. 

## Project hierarchy

The following shows the hierarchy of project files.

```
Mech-Eye_Csharp_interface
├─ Mechmind_CameraAPI_Csharp
│    ├─ CameraClient.cs
│    ├─ Mechmind_CameraAPI_Csharp.csproj
│    ├─ ZmqClient.cs
│    └─ sample.cs
├─ Mechmind_CameraAPI_Csharp.sln
└─ README.md
```

**Mechmind_CameraAPI_Csharp**  folder contains all code. **CameraClient.cs** and **ZmqClient.cs** contains essential code of interfaces. 

**sample.cs** provides a simple example to show the usage of interfaces.

## Brief Intro to interfaces

All interfaces and functions are in  **CameraClient.cpp**.

There are two main classes: CameraClient and ZmqClient. CameraClient is subclass of ZmqClient. You only need to focus on CameraClient.

* **CameraClient**

  * **connect()** : connect to the camera according to its IP address.

  * **captureDepthImg()** : capture a depth image and return it.

  * **captureColorImg()** : capture a color image and return it.

  * **getCameraIntri()**: get camera's intrinsic parameters.

  * **getCameraInfo()**: get camera's ip address.

  * **getCameraVersion()**: get camera's version number.

  * **getColorImgSize()** : get the height and width of the color image to be captured.

  * **getDepthImgSize()** : get the height and width of the depth image to be captured.

  * **getParameter()** : get the value of a specific parameter in camera. 

  * **setParameter()** : set the value of a specific parameter in camera.

    **Attention**: Please be sure to know the meaning of your setting of parameters, **wrong setting could cause some errors in the interfaces!**
  * **captureRgbCloud()** : get a point cloud as a double array.


### Intro to samples

The original project provides a sample to show how to use interfaces.

##### sample.cs

This sample mainly shows how to set camera's paramters like exposure time.

First, we need to know the actual IP address of camera and set it, and then connect:

```c#
CameraClient camera = new CameraClient();
//camera IP should be modified to actual IP address
//always set IP before do anything else
camera.connect("192.168.3.76");
```

Then, we can get some brief info about camera:

```c#
Console.WriteLine("Camera ID: " + camera.getCameraId());
Console.WriteLine("Version: " + camera.getCameraVersion());
int[] colorImgSize = camera.getColorImgSize();
int[] depthImgSize = camera.getDepthImgSize();
Console.WriteLine("Color Image Size: {0} * {1}", colorImgSize[0], colorImgSize[1]);
Console.WriteLine("Depth Image Size: {0} * {1}", depthImgSize[0], depthImgSize[1]);
```

Finally, we can set and get the value of a specific parameter, in this case, we choose exposure time for color image:

```c#
Console.WriteLine(camera.setParameter("scan2dExposureMode",0)); 
Console.WriteLine(camera.getParameter("scan2dExposureMode"));
Console.WriteLine(camera.setParameter("scan2dExposureTime",20)); 
Console.WriteLine(camera.getParameter("scan2dExposureTime"));
```

The program can capture color images and depth images by camera. And also point clouds will be captured as a double array:

```c#
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
double[,] rel = camera.captureRGBCloud();//point cloud data in xyzbgr

```

