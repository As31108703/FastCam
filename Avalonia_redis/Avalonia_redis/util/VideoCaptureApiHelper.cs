using System;
using Emgu.CV;

namespace Avalonia_redis
{
  public static class VideoCaptureApiHelper
  {
    public static VideoCapture.API GetVideoCaptureAPI(string apiName)
    {
      switch (apiName)
      {
        case "Any":
          return VideoCapture.API.Any;
        case "Vfw":
          return VideoCapture.API.Vfw;
        case "V4L":
          return VideoCapture.API.V4L;
        case "V4L2":
          return VideoCapture.API.V4L2;
        case "Firewire":
          return VideoCapture.API.Firewire;
        case "IEEE1394":
          return VideoCapture.API.IEEE1394;
        case "DC1394":
          return VideoCapture.API.DC1394;
        case "CMU1394":
          return VideoCapture.API.CMU1394;
        case "QT":
          return VideoCapture.API.QT;
        case "Unicap":
          return VideoCapture.API.Unicap;
        case "Pvapi":
          return VideoCapture.API.Pvapi;
        case "OpenNI":
          return VideoCapture.API.OpenNI;
        case "OpenNIAsus":
          return VideoCapture.API.OpenNIAsus;
        case "DShow":
          return VideoCapture.API.DShow;
        case "Android":
          return VideoCapture.API.Android;
        case "XiApi":
          return VideoCapture.API.XiApi;
        case "AVFoundation":
          return VideoCapture.API.AVFoundation;
        case "Msmf":
          return VideoCapture.API.Msmf;
        case "Giganetix":
          return VideoCapture.API.Giganetix;
        case "Winrt":
          return VideoCapture.API.Winrt;
        case "IntelPerc":
          return VideoCapture.API.IntelPerc;
        case "Openni2":
          return VideoCapture.API.Openni2;
        case "Openni2Asus":
          return VideoCapture.API.Openni2Asus;
        case "Gphoto2":
          return VideoCapture.API.Gphoto2;
        case "Gstreamer":
          return VideoCapture.API.Gstreamer;
        case "Ffmpeg":
          return VideoCapture.API.Ffmpeg;
        case "Images":
          return VideoCapture.API.Images;
        case "Aravis":
          return VideoCapture.API.Aravis;
        case "OpencvMjpeg":
          return VideoCapture.API.OpencvMjpeg;
        case "IntelMfx":
          return VideoCapture.API.IntelMfx;
        case "Xine":
          return VideoCapture.API.Xine;
        default:
          throw new ArgumentException("Invalid API name");
      }
    }
  }
}