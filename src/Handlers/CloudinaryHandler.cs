using api_infor_cell.src.Shared.Utils;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace api_infor_cell.src.Handlers
{
    public class CloudinaryHandler(Cloudinary cloudinary)
    {
        public async Task<string> UploadAttachment(string parent, IFormFile attachment)
        {
            string extension = Path.GetExtension(attachment.FileName).ToLower();
            bool isHeic = extension == ".heic" || extension == ".heif";
            string fileName = Guid.NewGuid().ToString();

            using var memoryStream = new MemoryStream();

            if (isHeic)
            {
                extension = ".jpg";
            }

            await attachment.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            RawUploadParams uploadParams = new()
            {
                File = new FileDescription(fileName + extension, memoryStream),
                Folder = $"telemovvi/{parent}",
                PublicId = fileName
            };

            RawUploadResult result = await cloudinary.UploadAsync(uploadParams);
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
