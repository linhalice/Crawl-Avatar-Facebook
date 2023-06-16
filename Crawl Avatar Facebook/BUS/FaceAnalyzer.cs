using Leaf.xNet;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crawl_Avatar_Facebook.BUS
{
    public class FaceAnalyzer
    {
        private readonly string endpoint;
        private readonly string subscriptionKey;

        public FaceAnalyzer(string endpoint, string subscriptionKey)
        {
            this.endpoint = endpoint;
            this.subscriptionKey = subscriptionKey;
        }

        public void AnalyzeFace(string imagePath)
        {
            string outputFolderPath = "Avatar"; 
            // Tạo thư mục đầu ra nếu chưa tồn tại
            Directory.CreateDirectory(outputFolderPath + "/Lọc kỹ");

            #region Khai báo request
            HttpRequest xRequest = new HttpRequest();
            xRequest.KeepAlive = true;
            xRequest.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            xRequest.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            xRequest.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
            try
            {

                byte[] imageData = File.ReadAllBytes(imagePath);
                #endregion
                string uri = $"{endpoint}face/v1.0/detect?returnFaceAttributes=glasses,accessories,occlusion&recognitionModel=recognition_04";

                string jsonResponse = xRequest.Post(uri, imageData, "application/octet-stream").ToString();
                bool isImageValid = IsImageValid(jsonResponse);

                if (isImageValid)
                {
                    // Di chuyển ảnh hợp lệ vào thư mục OK
                    File.Move(imagePath, Path.Combine(outputFolderPath, "Lọc kỹ", Path.GetFileName(imagePath)));
                   
                }
                else
                {
                    // Di chuyển ảnh không hợp lệ vào thư mục Not OK
                    File.Delete(imagePath);
                    Console.WriteLine("Invalid image. Moved to Not OK folder.");
                }

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                var a = xRequest.Response.ToString();
            }
        }

        public bool IsImageValid(string jsonResponse)
        {
            try
            {
                var faceResults = JsonConvert.DeserializeObject<FaceResult[]>(jsonResponse);

                if (faceResults.Length != 1)
                    return false;

                var face = faceResults[0];
                var occlusion = face.FaceAttributes.Occlusion;
                var glasses = face.FaceAttributes.Glasses;

                if (occlusion.ForeheadOccluded || occlusion.EyeOccluded || occlusion.MouthOccluded)
                    return false;

                if (glasses != "NoGlasses")
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return false;
            }
        }

        public class FaceResult
        {
            [JsonProperty("faceRectangle")]
            public FaceRectangle FaceRectangle { get; set; }

            [JsonProperty("faceAttributes")]
            public FaceAttributes FaceAttributes { get; set; }
        }

        public class FaceRectangle
        {
            [JsonProperty("top")]
            public int Top { get; set; }

            [JsonProperty("left")]
            public int Left { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }
        }

        public class FaceAttributes
        {
            [JsonProperty("glasses")]
            public string Glasses { get; set; }

            [JsonProperty("occlusion")]
            public Occlusion Occlusion { get; set; }
        }

        public class Occlusion
        {
            [JsonProperty("foreheadOccluded")]
            public bool ForeheadOccluded { get; set; }

            [JsonProperty("eyeOccluded")]
            public bool EyeOccluded { get; set; }

            [JsonProperty("mouthOccluded")]
            public bool MouthOccluded { get; set; }
        }
    }
}
