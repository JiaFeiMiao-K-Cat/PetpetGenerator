using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Image = SixLabors.ImageSharp.Image;
using Path = System.IO.Path;

namespace Petpet.Utils
{
    public class Petpet
    {
        public static async Task<byte[]?> MakePetpet(string qqid, string baseDirectory)
        {
            string json = File.ReadAllText(Path.Combine(baseDirectory, "data.json"));
            PetpetSettings? Settings = JsonSerializer.Deserialize<PetpetSettings>(json);
            switch (Settings?.type)
            {
                case "GIF":
                    {
                        return await MakeGIF(await QQHelper.GetAvatar(qqid), baseDirectory, Settings!);
                    }
                case "IMG":
                    {
                        return await MakeIMG(await QQHelper.GetAvatar(qqid), baseDirectory, Settings!);
                    }
                case "EXTENDED_IMG":
                    {
                        return await MakeExtendedIMG(await QQHelper.GetAvatar(qqid), baseDirectory, Settings!);
                    }
                case "EXTENDED_GIF":
                    {
                        return await MakeExtendedGIF(await QQHelper.GetAvatar(qqid), baseDirectory, Settings!);
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
            throw new NullReferenceException();
        }
        public static async Task<byte[]?> MakePetpet(byte[] avatar, string baseDirectory)
        {
            string json = File.ReadAllText(Path.Combine(baseDirectory, "data.json"));
            PetpetSettings? Settings = JsonSerializer.Deserialize<PetpetSettings>(json);
            switch (Settings?.type)
            {
                case "GIF":
                    {
                        return await MakeGIF(avatar, baseDirectory, Settings!);
                    }
                case "IMG":
                    {
                        return await MakeIMG(avatar, baseDirectory, Settings!);
                    }
                case "EXTENDED_IMG":
                    {
                        return await MakeExtendedIMG(avatar, baseDirectory, Settings!);
                    }
                case "EXTENDED_GIF":
                    {
                        return await MakeExtendedGIF(avatar, baseDirectory, Settings!);
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
            throw new NullReferenceException();
        }

        public static async Task<byte[]?> MakeGIF(byte[] avatar, string baseDirectory, PetpetSettings settings)
        {
            using var image = Image.Load(avatar);
            if (image != null)
            {
                var images = new List<Image>();
                for (int i = 0; i < settings.needProcess[0]; i++)
                {
                    images.Add(Image.Load(Path.Combine(baseDirectory, $"{i}.png")));
                }
                for (int i = settings.needProcess[0]; i <= settings.needProcess[1]; i++)
                {
                    var index = i - settings.needProcess[0];
                    images.Add(ProcessPetPetImage(image, i,
                        settings.pos[index][0],
                        settings.pos[index][1],
                        settings.pos[index][2],
                        settings.pos[index][3],
                        baseDirectory
                    ));
                }
                for (int i = settings.needProcess[1] + 1; i < settings.imageCount; i++)
                {
                    images.Add(Image.Load(Path.Combine(baseDirectory, $"{i}.png")));
                }
                using (var gif = new Image<Rgba32>(images[0].Width, images[0].Height))
                {
                    gif.Metadata.GetGifMetadata().RepeatCount = 0;
                    GifEncoder encoder = new GifEncoder()
                    {
                        Quantizer = new OctreeQuantizer(new QuantizerOptions
                        {
                            Dither = null,
                            DitherScale = 0,
                            MaxColors = 256
                        })
                    }; // 消除petpet出现的杂点
                    for (int i = 0; i < images.Count; i++)
                    {
                        var frame = images[i].Frames[0];
                        frame.Metadata.GetGifMetadata().DisposalMethod = GifDisposalMethod.RestoreToBackground; // 无拖影
                        gif.Frames.InsertFrame(i, frame);
                    }
                    gif.Frames.RemoveFrame(gif.Frames.Count - 1);
                    using var mem = new MemoryStream();
                    await gif.SaveAsGifAsync(mem, encoder);
                    return mem.ToArray();
                }
            }
            return null;
        }
        public static async Task<byte[]?> MakeIMG(byte[] avatar, string baseDirectory, PetpetSettings settings)
        {
            using var image = Image.Load(avatar);
            if (image != null)
            {
                var result = ProcessPetPetImage(
                    image, 
                    0, 
                    settings.pos[0][0], settings.pos[0][1], 
                    settings.pos[0][2], settings.pos[0][3], 
                    baseDirectory,
                    PixelAlphaCompositionMode.SrcOver
                );
                using var mem = new MemoryStream();
                await result.SaveAsPngAsync(mem);
                return mem.ToArray();
            }
            return null;
        }
        public static async Task<byte[]?> MakeExtendedIMG(byte[] avatar, string baseDirectory, PetpetSettings settings)
        {
            using var image = Image.Load(avatar);
            if (image != null)
            {
                var result = ProcessExtendedPetpetImage(image, settings.pos[0][0], settings.pos[0][1],
                    new PointF(settings.pos[0][2], settings.pos[0][3]),
                    new PointF(settings.pos[0][4], settings.pos[0][5]),
                    new PointF(settings.pos[0][6], settings.pos[0][7]),
                    new PointF(settings.pos[0][8], settings.pos[0][9]),
                    baseDirectory
                );
                using var mem = new MemoryStream();
                await result.SaveAsPngAsync(mem);
                return mem.ToArray();
            }
            return null;
        }

        public static async Task<byte[]?> MakeExtendedGIF(byte[] avatar, string baseDirectory, PetpetSettings settings)
        {
            using var image = Image.Load(avatar);
            if (image != null)
            {
                var images = new List<Image>();
                for (int i = 0; i < settings.imageCount; i++)
                {
                    images.Add(Image.Load(Path.Combine(baseDirectory, $"{i}.png")));
                }
                foreach (var pos in settings.pos)
                {
                    var index = pos[0];
                    images[index] = ProcessExtendedPetpetImage(image, index, pos[1],
                        new PointF(pos[2], pos[3]),
                        new PointF(pos[4], pos[5]),
                        new PointF(pos[6], pos[7]),
                        new PointF(pos[8], pos[9]),
                        baseDirectory
                    );
                }
                using (var gif = new Image<Rgba32>(images[0].Width, images[0].Height))
                {
                    gif.Metadata.GetGifMetadata().RepeatCount = 0;
                    GifEncoder encoder = new GifEncoder()
                    {
                        Quantizer = new OctreeQuantizer(new QuantizerOptions
                        {
                            Dither = null,
                            DitherScale = 0,
                            MaxColors = 256
                        })
                    }; // 消除petpet出现的杂点
                    for (int i = 0; i < images.Count; i++)
                    {
                        var frame = images[i].Frames[0];
                        frame.Metadata.GetGifMetadata().DisposalMethod = GifDisposalMethod.RestoreToBackground; // 无拖影
                        gif.Frames.InsertFrame(i, frame);
                    }
                    gif.Frames.RemoveFrame(gif.Frames.Count - 1);
                    using var mem = new MemoryStream();
                    await gif.SaveAsGifAsync(mem, encoder);
                    return mem.ToArray();
                }
            }
            return null;
        }

        private static Image ProcessExtendedPetpetImage(Image avatarImage, int index, int layoutPriority,
            PointF leftTop, PointF rightTop, PointF rightBottom, PointF leftBottom,
            string BaseDirectory)
        {
            Image avatar = avatarImage.CloneAs<Rgba32>();
            PixelAlphaCompositionMode mode = PixelAlphaCompositionMode.SrcOver;
            switch (layoutPriority)
            {
                case 1:
                    {
                        mode = PixelAlphaCompositionMode.DestOver;
                        break;

                    }
                default:
                    {
                        break;
                    }
            }
            var matrix = ImageTransform.CalcuteTransformMatrix(avatar.Size, leftTop, rightTop, rightBottom, leftBottom);
            avatar.Mutate(e => e.Transform(new ProjectiveTransformBuilder().AppendMatrix(matrix)));
            Image background = Image.Load(Path.Combine(BaseDirectory, $"{index}.png"));
            background.Mutate(e => e.DrawImage(avatar, new GraphicsOptions
            {
                AlphaCompositionMode = mode
            }));
            return background;
        }
        private static Image ProcessPetPetImage(Image avatarImage, int index, int x, int y, int width, int height, string baseDirectory, 
            PixelAlphaCompositionMode alphaCompositionMode = PixelAlphaCompositionMode.DestOver)
        {
            Image avatar = avatarImage.Clone(e => e.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Stretch,
                Size = new Size(width, height)
            }));
            Image background = Image.Load(Path.Combine(baseDirectory, $"{index}.png"));
            background.Mutate(e => e.DrawImage(
                avatar, 
                new Point(x, y), 
                new GraphicsOptions
                {
                    AlphaCompositionMode = alphaCompositionMode
                }
            ));
            return background;
        }
    }

    public class PetpetSettings
    {
        public string name { get; set; }
        public string type { get; set; }
        public int imageCount { get; set; }
        public int[] needProcess { get; set; }

        public int[][] pos { get; set; }
    }
}
