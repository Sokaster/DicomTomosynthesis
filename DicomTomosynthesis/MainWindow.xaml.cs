using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using Dicom;
using System.Windows.Threading;
namespace DicomTomosynthesis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int currentIndex = 0;
        private string[] dicomFiles;
        private DispatcherTimer timer;
        private double animationSpeed = 5; // начальная скорость
        //public int windowCenter = 0;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new Microsoft.Win32.OpenFileDialog();
            openFolderDialog.CheckFileExists = false;
            openFolderDialog.CheckPathExists = true;
            openFolderDialog.FileName = "[Get Folder]";
            openFolderDialog.Filter = "Folders|no.files";

            bool? userClickedOK = openFolderDialog.ShowDialog();

            if (userClickedOK == true)
            {
                string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(openFolderDialog.FileName), "*.dcm");
                StartAnimation(files);
            }
        }

        private void PreviousFrameButton_Click(object sender, RoutedEventArgs e)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = dicomFiles.Length - 1;
            ReadAndDisplayDicomFile(dicomFiles[currentIndex]);
        }

        private void NextFrameButton_Click(object sender, RoutedEventArgs e)
        {
            currentIndex++;
            if (currentIndex >= dicomFiles.Length) currentIndex = 0;
            ReadAndDisplayDicomFile(dicomFiles[currentIndex]);
        }

        private void PauseContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                PauseContinueButton.Content = "Start";
            }
            else
            {
                timer.Start();
                PauseContinueButton.Content = "Stop";
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            animationSpeed = SpeedSlider.Value;

            if (timer != null)
            {
                timer.Interval = TimeSpan.FromMilliseconds(1000 / animationSpeed);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentIndex >= dicomFiles.Length)
            {
                currentIndex = 0;
            }

            ReadAndDisplayDicomFile(dicomFiles[currentIndex]);
            currentIndex++;
        }

        public void StartAnimation(string[] filePaths)
        {
            dicomFiles = filePaths;
            currentIndex = 0;

            timer = new DispatcherTimer();

            // use the slider value to set the timer interval
            timer.Interval = TimeSpan.FromMilliseconds(1000 / animationSpeed);

            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void ReadAndDisplayDicomFile(string filePath)
        {
            DicomFile dicomFile = DicomFile.Open(filePath);
            var dicomImage = new DicomImage(dicomFile.Dataset);

            //dicomImage.WindowCenter = windowCenter;
            System.Drawing.Bitmap bmp = dicomImage.RenderImage(0).AsClonedBitmap();

            // Convert bitmap to bitmapsource
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                bmp.GetHbitmap(),
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());

            // set the image control source to display the image
            System.Windows.Controls.Image myImage = (System.Windows.Controls.Image)FindName("DicomImage");
            myImage.Source = bitmapSource;
        }
        //private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    // Set the new brightness level
        //    windowCenter = (int)BrightnessSlider.Value;

        //    // Re-render the current image with the new brightness
        //    ReadAndDisplayDicomFile(dicomFiles[currentIndex]);
        //}
    }
}
