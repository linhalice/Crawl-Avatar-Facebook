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

                    foreach (var face in faces)
                    {
                        if (face.Width > 200 || face.Height > 200)
                        {
                            listFaces.Add(face);
                        }
                    }

                    // Count the number of faces detected
                    int faceCount = listFaces.Count;

                    if (faceCount > 1 || faceCount == 0)
                    {
                          image.Dispose();
                        File.Delete(imageFile);
                        continue;
                    }

                    // Create the destination folder if it does not exist
                    string destinationFolder = Path.Combine(folderPath, $"{faceCount} Face");

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
                catch(Exception ex)
                {
                    File.Delete(imageFile);
                }
            }
        }

    }
}
