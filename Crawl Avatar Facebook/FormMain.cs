using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Net;
using Emgu.CV.Face;

namespace Crawl_Avatar_Facebook
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                int i = 0;
                Thread threadMain = new Thread(() =>
                {
                    var list = File.ReadAllLines("Data.txt").ToList();
                    foreach (var line in list)
                    {
                        i++;
                        Thread.Sleep(200);
                        string uid = line.Split('|')[0];
                        Thread thread = new Thread(() =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                label1.Text = $"Trạng thái: Đang tải {i}/{list.Count}";
                            }));
                            BUS.Facebook.AvatarDownload(uid);
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                });
                threadMain.IsBackground = true;
                threadMain.Start();
            }

            {

                Thread threadMain = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            GetImagesWithFaces("Avatar");
                            Thread.Sleep(10000);
                        }
                        catch (Exception)
                        {


                        }
                    }
                });
                threadMain.IsBackground = true;
                threadMain.Start();
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filePath = @"Data.txt";

            // Kiểm tra xem tệp có tồn tại hay không
            if (System.IO.File.Exists(filePath))
            {
                // Mở tệp với ứng dụng mặc định
                Process.Start(filePath);
            }
            else
            {
                Console.WriteLine("File does not exist!");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = @"Avatar";
            if (Directory.Exists(path))
            {
                Process.Start("explorer.exe", path);
            }
            else
            {
                Console.WriteLine("Directory does not exist!");
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            GetImagesWithFaces("Avatar");
        }

        public void GetImagesWithFaces(string folderPath)
        {
            // Load the Haar cascade classifier for face detection
            CascadeClassifier faceCascade = new CascadeClassifier("haarcascade_frontalface_alt2.xml");

            // Get a list of all image files in the specified folder
            string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

            // Loop through each image file and check if it contains a face
            foreach (string imageFile in imageFiles)
            {
                Thread.Sleep(100);
                try
                {
                    // Load the image file
                    Image<Bgr, byte> image = new Image<Bgr, byte>(imageFile);

                    if (image.Width < 500 || image.Height < 500)
                    {
                        image.Dispose();
                        File.Delete(imageFile);
                        continue;
                    }

                    // Detect the faces in the image
                    Rectangle[] faces = faceCascade.DetectMultiScale(image.Convert<Gray, byte>());

                    List<Rectangle> listFaces = new List<Rectangle>();
                    List<Rectangle> listFacesInvail = new List<Rectangle>();

                    foreach (var face in faces)
                    {
                        if (face.Width > 200 || face.Height > 200)
                        {
                            float checkWidth = (float)face.Width / image.Width;
                            float checkHeight = (float)face.Height / image.Height;
                            if (checkWidth >= 0.3 && checkHeight >= 0.22)
                            {
                                if (face.X > image.Width / 10 && face.Y > image.Height / 10 && face.X < image.Width / 2 && face.Y < image.Height / 3)
                                {
                                    if (face.X + face.Width < image.Width - (image.Width / 10) && face.Y + face.Height < image.Height - (image.Height / 10))
                                    {
                                        listFaces.Add(face);
                                    }

                                }

                            }
                        }
                    }

                    foreach (var face in faces)
                    {
                        if (face.Width > 200 || face.Height > 200)
                        {
                            listFacesInvail.Add(face);
                        }
                    }




                    // Count the number of faces detected
                    int faceCount = listFaces.Count;

                    if (faceCount > 1 || faceCount == 0|| listFacesInvail.Count>1)
                    {
                        image.Dispose();
                        File.Delete(imageFile);
                        continue;
                    }

                    // Create the destination folder if it does not exist
                    string destinationFolder = Path.Combine(folderPath, $"Level 1");

                    if (!Directory.Exists(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    // Move the image to the appropriate folder based on the face count
                    string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(imageFile));

                    //File.Move(imageFile, destinationFile);

                    // Loop through each face and draw a rectangle
                    //foreach (Rectangle face in listFaces)
                    //{
                    //    //image.Draw(face, new Bgr(0, 255, 0), 2);
                    //}

                    // Save the image with the face rectangles
                    image.Save(destinationFile);

                    image.Dispose();

                    File.Delete(imageFile);
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    File.Delete(imageFile);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string folderPath = "Avatar\\Lọc tương đối"; // Thay đường dẫn thư mục của bạn vào đây
            string outputFolderPath = "Avatar"; // Thay đường dẫn thư mục đầu ra của bạn vào đây


            string subscriptionKey = "b0c89dfd41f24fc4b45081b543e3526d";
            string endpoint = "https://thanhsonface.cognitiveservices.azure.com/";
            BUS.FaceAnalyzer faceAnalyzer = new BUS.FaceAnalyzer(endpoint, subscriptionKey);


            // Lấy danh sách tệp tin ảnh trong thư mục
            string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

            // Tạo thư mục đầu ra nếu chưa tồn tại
            Directory.CreateDirectory(outputFolderPath + "/Lọc kỹ");

            // Lọc ảnh
            foreach (string imageFile in imageFiles)
            {


                new Thread(() =>
                {
                    Console.WriteLine("Processing: " + imageFile);
                    faceAnalyzer.AnalyzeFace(imageFile);
                }).Start();
                Thread.Sleep(300);

            }
        }


        private void hahaha()
        {
            string folderPath = "Avatar"; // Thay đường dẫn thư mục của bạn vào đây
            string outputFolderPath = "Avatar"; // Thay đường dẫn thư mục đầu ra của bạn vào đây

            // Khởi tạo ComputerVisionClient
            ComputerVisionClient computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials("508b24c8b9984215a3a94aaeff3877d8"));

            // Thiết lập endpoint cho ComputerVisionClient
            computerVisionClient.Endpoint = "https://thanhsonvision.cognitiveservices.azure.com/";

            // Lấy danh sách tệp tin ảnh trong thư mục
            string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

            // Tạo thư mục đầu ra nếu chưa tồn tại
            Directory.CreateDirectory(outputFolderPath + "/OK");
            Directory.CreateDirectory(outputFolderPath + "/Not OK");

            // Lọc ảnh
            foreach (string imageFile in imageFiles)
            {
                Console.WriteLine("Processing: " + imageFile);

                // Đọc dữ liệu từ tệp tin ảnh
                using (Stream imageFileStream = File.OpenRead(imageFile))
                {
                    // Gọi API nhận diện khuôn mặt
                    var faceResults = computerVisionClient.AnalyzeImageInStreamAsync(imageFileStream, new List<VisualFeatureTypes?> { VisualFeatureTypes.Faces }).Result;

                    // Kiểm tra số lượng khuôn mặt
                    if (faceResults.Faces.Count == 1)
                    {
                        // Kiểm tra kích thước khuôn mặt
                        var faceWidth = faceResults.Faces[0].FaceRectangle.Width;
                        var imageWidth = faceResults.Metadata.Width;

                        // In ra các thông số kiểm tra
                        foreach (var face in faceResults.Faces)
                        {
                            Console.WriteLine("Face attributes:");
                            Console.WriteLine($"Age: {face.Age}");
                            Console.WriteLine($"Gender: {face.Gender}");

                        }


                        if ((double)faceWidth / imageWidth >= 0.25)
                        {
                            // Di chuyển ảnh hợp lệ vào thư mục OK
                            File.Move(imageFile, Path.Combine(outputFolderPath, "OK", Path.GetFileName(imageFile)));
                            Console.WriteLine("Valid image. Moved to OK folder.");
                        }
                        else
                        {
                            // Di chuyển ảnh không hợp lệ vào thư mục Not OK
                            File.Move(imageFile, Path.Combine(outputFolderPath, "Not OK", Path.GetFileName(imageFile)));
                            Console.WriteLine("Invalid image. Moved to Not OK folder.");
                        }
                    }
                    else
                    {
                        // Di chuyển ảnh không hợp lệ vào thư mục Not OK
                        File.Move(imageFile, Path.Combine(outputFolderPath, "Not OK", Path.GetFileName(imageFile)));
                        Console.WriteLine("Invalid image. Moved to Not OK folder.");
                    }
                }
            }
        }
    }
}
