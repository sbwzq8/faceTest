using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace FaceTutorial
{
    public partial class MainWindow : Window
    {
        // Replace the first parameter with your valid subscription key.
        //
        // Replace or verify the region in the second parameter.
        //
        // You must use the same region in your REST API call as you used to obtain your subscription keys.
        // For example, if you obtained your subscription keys from the westus region, replace
        // "westcentralus" in the URI below with "westus".
        //
        // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using
        // a free trial subscription key, you should not need to change this region.
        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("b17c9821ee20450f88f2f7e28107de9a", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

        Face[] faces;                   // The list of detected faces.
        double resizeFactor;            // The resize factor for the displayed image.

        public MainWindow()
        {
            InitializeComponent();
        }

        // Displays the image and calls Detect Faces.

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the image file to scan from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            // Return if canceled.
            if (!(bool)result)
            {
                return;
            }

            // Display the image file.
            string filePath = openDlg.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            FacePhoto.Source = bitmapSource;

            // Detect any faces in the image.
            Title = "Detecting...";
            faces = await UploadAndDetectFaces(filePath);
            Title = String.Format("Detection Finished. {0} face(s) detected", faces.Length);

            if (faces.Length > 0)
            {
                // Prepare to draw rectangles around the faces.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                resizeFactor = 96 / dpi;

                Image img;
                Bitmap bitmap2 = BitmapImage2Bitmap(bitmapSource);
                Image imageBackground = (Image)bitmap2;
                for (int i = 0; i < faces.Length; ++i)
                {
                    Face face = faces[i];


                  //  Bitmap bitmap2 = BitmapImage2Bitmap(bitmapSource);
                  //  Image imageBackground = (Image)bitmap2;
                    Image rawImageOverlay = Image.FromFile("glasses.png");
                    

                    double ratiox = rawImageOverlay.Width;
                    double ratioy = rawImageOverlay.Height;

                    double ratio = ratioy / ratiox;

                    double resize = face.FaceRectangle.Height * ratio;




                    // face.FaceRectangle.Width, face.FaceRectangle.Height * (int)ratio)
                    //Image imageOverlay = Image.FromFile("santa2.png");
                    Image imageOverlay = ResizeImage(rawImageOverlay, face.FaceRectangle.Width,(int)resize);
                    //Image imageOverlay = ResizeImage(rawImageOverlay, (int) (rawImageOverlay.Width * .4), (int) (rawImageOverlay.Height * .4));
                    
                    img = new Bitmap(imageBackground.Width, imageBackground.Height);

                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        gr.DrawImage(imageBackground, new System.Drawing.Point(0, 0));
                        //gr.DrawImage(imageOverlay, face.FaceRectangle.Left, face.FaceRectangle.Top);
                        //gr.DrawImage(imageOverlay, new System.Drawing.Point((face.FaceRectangle.Left - (face.FaceRectangle.Width / 2) ), face.FaceRectangle.Top - (face.FaceRectangle.Height /2) ));
                        //gr.DrawImage(imageOverlay, (face.FaceRectangle.Left + (face.FaceRectangle.Width/2) ) - (imageOverlay.Width/2), (face.FaceRectangle.Top + (face.FaceRectangle.Height/2) ) - (imageOverlay.Height/2) );
                        gr.DrawImage(imageOverlay, (face.FaceRectangle.Left - (face.FaceRectangle.Width/2) - 30 ) /*- (imageOverlay.Width/2)*/, (face.FaceRectangle.Top - (face.FaceRectangle.Height/2) ) - (imageOverlay.Height/2) + 20 );
                    }
                    img.Save("output1" /*+ i*/ + ".png", ImageFormat.Png);

                    

                    

                    /*Image imageBackground = bitmapSource;
                    Image imageOverlay = Image.FromFile("bitmap2.png");

                    Image img = new Bitmap(imageBackground.Width, imageBackground.Height);
                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        gr.DrawImage(imageBackground, new System.Drawing.Point(0, 0));
                        gr.DrawImage(imageOverlay, new System.Drawing.Point(0, 0));
                    }
                    img.Save("output.png", ImageFormat.Png);*/

                    // Draw a rectangle on the face.
                    drawingContext.DrawRectangle(
                        System.Windows.Media.Brushes.Transparent,
                        new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 2),
                        new Rect(
                            face.FaceRectangle.Left * resizeFactor,
                            face.FaceRectangle.Top * resizeFactor,
                            face.FaceRectangle.Width * resizeFactor,
                            face.FaceRectangle.Height * resizeFactor
                            )
                    );
                }

                drawingContext.Close();

                // Display the image with the rectangle around the face.
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                //FacePhoto.Source = bitmapSource;
                //FacePhoto.Source = faceWithRectBitmap;

                Uri outputUri = new Uri(@"C:\Users\MiniTron\source\repos\FaceTutorial\FaceTutorial\bin\Debug\output1.png");
                BitmapImage outputBitmap = new BitmapImage();

                outputBitmap.BeginInit();
                outputBitmap.CacheOption = BitmapCacheOption.None;
                outputBitmap.UriSource = outputUri;
                outputBitmap.EndInit();

                FacePhoto.Source = outputBitmap;
            }
        }

        // Displays the face description when the mouse is over a face rectangle.

        private void FacePhoto_MouseMove(object sender, MouseEventArgs e)
        {
            // If the REST call has not completed, return from this method.
            if (faces == null)
                return;

            // Find the mouse position relative to the image.
            System.Windows.Point mouseXY = e.GetPosition(FacePhoto);

            ImageSource imageSource = FacePhoto.Source;
            BitmapSource bitmapSource = (BitmapSource)imageSource;

            // Scale adjustment between the actual size and displayed size.
            var scale = FacePhoto.ActualWidth / (bitmapSource.PixelWidth / resizeFactor);

            // Check if this mouse position is over a face rectangle.
            bool mouseOverFace = false;

            for (int i = 0; i < faces.Length; ++i)
            {
                FaceRectangle fr = faces[i].FaceRectangle;
                double left = fr.Left * scale;
                double top = fr.Top * scale;
                double width = fr.Width * scale;
                double height = fr.Height * scale;

                // Display the face description for this face if the mouse is over this face rectangle.
                if (mouseXY.X >= left && mouseXY.X <= left + width && mouseXY.Y >= top && mouseXY.Y <= top + height)
                {
                    mouseOverFace = true;
                    break;
                }
            }
        }

        // Uploads the image file and calls Detect Faces.

        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                return new Face[0];
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
