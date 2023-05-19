using System.Numerics;

namespace Petpet.Utils
{
    public class ImageTransform
    {
        public static Matrix4x4 CalcuteTransformMatrix(Size size, PointF leftTop, PointF rightTop, PointF rightBottom, PointF leftBottom)
        {
            // use decimal in the intermediate process to get higher precision
            decimal w = size.Width;
            decimal h = size.Height;
            decimal x1 = (decimal)leftTop.X;
            decimal y1 = (decimal)leftTop.Y;
            decimal x2 = (decimal)rightTop.X;
            decimal y2 = (decimal)rightTop.Y;
            decimal x3 = (decimal)rightBottom.X;
            decimal y3 = (decimal)rightBottom.Y;
            decimal x4 = (decimal)leftBottom.X;
            decimal y4 = (decimal)leftBottom.Y;

            var matrix = new Matrix4x4(
                0, 0, 0, leftTop.X,
                0, 0, 0, leftTop.Y,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            // https://jiafeimiao.top/2023/05/18/%E7%94%A8%E5%9B%9B%E8%A7%92%E5%9D%90%E6%A0%87%E5%8F%8D%E6%8E%A8%E6%8A%95%E5%BD%B1%E5%8F%98%E6%8D%A2%E7%9F%A9%E9%98%B5/
            decimal denominator = -x2 * (y3 - y4) + x3 * (y2 - y4) - x4 * (y2 - y3);
            matrix.M11 = (float)(-((x1 * x3 * (y2 - y4)) - (x1 * x4 * (y2 - y3)) - (x2 * x3 * (y1 - y4)) + (x2 * x4 * (y1 - y3))) / w / denominator);
            matrix.M12 = (float)(((x1 * x2 * (y3 - y4)) - (x1 * x3 * (y2 - y4)) + (x2 * x4 * (y1 - y3)) - (x3 * x4 * (y1 - y2))) / h / denominator);
            matrix.M21 = (float)((-(x1 * y2 - x2 * y1) * (y3 - y4) + (x3 * y4 - x4 * y3) * (y1 - y2)) / w / denominator);
            matrix.M22 = (float)(-((x1 * y4 - x4 * y1) * (y2 - y3) - (x2 * y3 - x3 * y2) * (y1 - y4)) / h / denominator);
            matrix.M41 = (float)((-(x1 - x2) * (y3 - y4) + (x3 - x4) * (y1 - y2)) / w / denominator);
            matrix.M42 = (float)((-(x1 - x4) * (y2 - y3) + (x2 - x3) * (y1 - y4)) / h / denominator);

            // ImageSharp use "vector x matrix" to do transform, so return Matrix.Transpose
            return Matrix4x4.Transpose(matrix);
        }
    }
}
