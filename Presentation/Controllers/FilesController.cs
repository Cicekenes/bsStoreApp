using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        [HttpPost("upload")]
        //formfile ile dosya işlemleri yapıyoruz,dosyalara erişmek,yüklemek, dosyalarla ilgili her türlü işlem.
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            //folder,combine birden fazla parametreyi birleştirir.Media klasörünü yakaladık
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "Media");

            //Dosya yoksa oluştur
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //path
            var path = Path.Combine(folder, file?.FileName);

            //stream
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //responseBody
            return Ok(new
            {
                file = file.FileName,
                path = path,
                size = file.Length
            });
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            //filePath
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Media",fileName);
            //ContentType:Jpeg,pdf,vs....
            var provider = new FileExtensionContentTypeProvider();
            if(!provider.TryGetContentType(fileName,out var contentType))
            {
                contentType = "application/octet-stream";
            }

            //Read
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes,contentType,Path.GetFileName(filePath));
        }
    }
}
