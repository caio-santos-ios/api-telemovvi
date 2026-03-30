using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace api_infor_cell.src.Handlers
{
    public class CloudinaryHandler(Cloudinary cloudinary)
    {
        public async Task<string> UploadAttachment(string parent, IFormFile attachment)
        {
            string extension = Path.GetExtension(attachment.FileName).ToLower();
            // bool isHeic = extension == ".heic" || extension == ".heif";
            string fileName = Guid.NewGuid().ToString();

            using var memoryStream = new MemoryStream();

            // if (isHeic)
            // {
            //     using var inputStream = attachment.OpenReadStream();
            //     using var image = await Image.LoadAsync(inputStream);
            //     await image.SaveAsJpegAsync(memoryStream);
            //     memoryStream.Position = 0;
            //     extension = ".jpg";
            // }
            // else
            // {
            // }
            await attachment.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName + extension, memoryStream),
                Folder = $"projeto-modelo/{parent}",
                PublicId = fileName
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            return result.SecureUrl.ToString();
        }
        
        public async Task<bool> Delete(string publicId, string folderProject, string folderModel)
        {
            // Cloudinary cloudinary = new(CloudinaryUrl);
            // cloudinary.Api.Secure = true;

            // DeletionParams deletionParams = new ($"{folderProject}/{folderModel}/{publicId}");
            // DeletionResult result = await cloudinary.DestroyAsync(deletionParams);

            // return result.Result == "ok";
            return true;
        }
    }
}