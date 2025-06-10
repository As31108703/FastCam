using Emgu.CV;
using Emgu.CV.Structure;

public static class ImageConverter
{
  public static byte[] ConvertFrameToBytes(Mat frame)
  {
    using Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
    byte[] jpegBytes = image.ToJpegData();
    return jpegBytes;
  }
}