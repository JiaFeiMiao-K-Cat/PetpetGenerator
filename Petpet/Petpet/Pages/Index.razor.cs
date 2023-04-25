using AntDesign;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Options;
using Petpet.Utils;

namespace Petpet.Pages
{
    public partial class Index
    {

        int maxSize = 32;

        bool IsQQ = true;

        bool loading = false;

        string qqid;

        string name;

        string imgDataURL = "";

        byte[] localImageData;

        byte[] imageData;

        IBrowserFile? file = null;

        private async void UploadFiles(InputFileChangeEventArgs e)
        {
            loading = true;
            file = e.GetMultipleFiles().FirstOrDefault();
            if ("image/jpeg" != file?.ContentType && "image/png" != file?.ContentType)
            {
                await _notice.Open(new NotificationConfig()
                {
                    Message = "文件格式错误",
                    Description = $"仅支持jpeg与png图像",
                    NotificationType = NotificationType.Error
                });
                file = null;
                loading = false;
                InvokeAsync(StateHasChanged);
                return;
            }
            if (maxSize * 1024 * 1024 < file?.Size)
            {
                await _notice.Open(new NotificationConfig()
                {
                    Message = "文件过大",
                    Description = $"仅支持不大于{maxSize}MiB的文件",
                    NotificationType = NotificationType.Error
                });
                file = null;
                loading = false;
                InvokeAsync(StateHasChanged);
                return;
            }
            var fs = file?.OpenReadStream(maxSize * 1024 * 1024);
            var ms = new MemoryStream();
            await fs?.CopyToAsync(ms);
            localImageData = ms.ToArray();
            loading = false;
            InvokeAsync(StateHasChanged);
        }

        private async void GenerateImageAsync (){
            loading = true;
            if (IsQQ)
            {
                if (string.IsNullOrWhiteSpace(qqid) || !qqid.All(char.IsAsciiDigit))
                {
                    await _notice.Open(new NotificationConfig()
                    {
                        Message = "这个QQ号似乎不太正常",
                        NotificationType = NotificationType.Error
                    });
                    qqid = string.Empty;
                    loading = false;
                    InvokeAsync(StateHasChanged);
                    return;
                }
                imageData = await Utils.Petpet.MakePetpet(qqid, $"{_myOptions.Value.BasePath}/{name}");
            }
            else
            {
                if (localImageData == null)
                {
                    await _notice.Open(new NotificationConfig()
                    {
                        Message = "还没有上传文件哦",
                        NotificationType = NotificationType.Error
                    });
                    loading = false;
                    InvokeAsync(StateHasChanged);
                    return;
                }
                imageData = await Utils.Petpet.MakePetpet(localImageData, $"{_myOptions.Value.BasePath}/{name}");
            }
            string imreBase64Data = Convert.ToBase64String(imageData);
            imgDataURL = string.Format("data:image/gif;base64,{0}", imreBase64Data);
            loading = false;
            InvokeAsync(StateHasChanged);
        }
    }
}
