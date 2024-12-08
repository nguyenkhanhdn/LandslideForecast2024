using LandslideForecast.Models;
using lobe;
using lobe.ImageSharp;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace LandslideForecast.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Cam()
        {
            return View();
        }
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Upload(IFormFile fileUpload)
        {
            try
            {
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + fileUpload.FileName;
                //Get url To Save
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                //string relativeImg2 = Path.Combine("uploads", fileName);

                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    fileUpload.CopyTo(stream);
                }
                var result = Predict(SavePath);
                
                ViewBag.Result = result;
            }
            catch (Exception ex)
            {
            }
            return View();
        }
        [HttpPost]
        public string UploadWebCamImage(string mydata)//Binary -> nội dung của bức ảnh
        {
            string[] dat = mydata.Split(';');
            //Full path of image

            var fileName = "hole" + DateTime.Now.ToString().Replace("/", "_").Replace(" ", "_").Replace(":", "") + ".png";
            //Get url To Save
            string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            string relativeImg2 = Path.Combine("uploads", fileName);

            using (FileStream fs = new FileStream(SavePath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] data = Convert.FromBase64String(dat[0]);
                    bw.Write(data);//Ghi xuống đĩa, lưu trong thư mục UploadedFiles
                    bw.Close(); //Mở file thì có đóng file -> lỗi
                }
            }
            //Sau khi upload gọi hàm nhận diện 
            string predictResult = Predict(SavePath);
            ViewBag.Result = predictResult;
            
            return predictResult; //Hàm trả về là nhãn: sạt lở, không sạt lở
        }
        //Hàm chẩn đoán sạt lở đất

        private string Predict(string fileName)
        {
            try
            {
                string signatureFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/model/signature.json");
                var imageToClassify = fileName;//File ảnh cần chẩn đoán

                lobe.ImageClassifier.Register("onnx", () => new OnnxImageClassifier());
                var classifier = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

                //Phân loại ảnh  
                var results = classifier.Classify(SixLabors.ImageSharp.Image.Load(imageToClassify).CloneAs<Rgb24>());
                return results.Prediction.Label;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult About() { 
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}