using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Handle.HandleFile
{
    public class HandleUploadFile
    {

        public static async Task<string> WirteFileAvatar(IFormFile file)
        {
            string fileName = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = "MyBug_" + DateTime.Now.Ticks + extension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Avatar");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                var exactPath = Path.Combine(filePath, fileName);
                using (var stream = new FileStream(exactPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return fileName;
        }
        public static async Task<string> WirteFileProduct(IFormFile file)
        {
            string fileName = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = "ProductImage_" + DateTime.Now.Ticks + extension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Product");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                var exactPath = Path.Combine(filePath, fileName);
                using (var stream = new FileStream(exactPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return fileName;
        }
    }
}
