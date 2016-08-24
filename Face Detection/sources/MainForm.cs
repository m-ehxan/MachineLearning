using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Accord.Imaging.Filters;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using FaceDetection.Properties;
using System.Net;

namespace FaceDetection
{
    public partial class MainForm : Form
    {
        Bitmap picture = null; 

        HaarObjectDetector detector;

        public MainForm()
        {
            InitializeComponent();

           

            cbMode.DataSource = Enum.GetValues(typeof(ObjectDetectorSearchMode));
            cbScaling.DataSource = Enum.GetValues(typeof(ObjectDetectorScalingMode));

            cbMode.SelectedItem = ObjectDetectorSearchMode.NoOverlap;
            cbScaling.SelectedItem = ObjectDetectorScalingMode.SmallerToGreater;

            toolStripStatusLabel1.Text = "Please select the detector options and click Detect to begin.";

            HaarCascade cascade = new FaceHaarCascade();
            detector = new HaarObjectDetector(cascade, 30);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            picture = new Bitmap(pictureBox1.Image);
            detector.SearchMode = (ObjectDetectorSearchMode)cbMode.SelectedValue;
            detector.ScalingMode = (ObjectDetectorScalingMode)cbScaling.SelectedValue;
            detector.ScalingFactor = 1.5f;
            detector.UseParallelProcessing = cbParallel.Checked;
            detector.Suppression = 2;

            Stopwatch sw = Stopwatch.StartNew();


            // Process frame to detect objects
            Rectangle[] objects = detector.ProcessFrame(picture);


            sw.Stop();


            if (objects.Length > 0)
            {
                RectanglesMarker marker = new RectanglesMarker(objects, Color.Fuchsia);
                pictureBox1.Image = marker.Apply(picture);
            }

            toolStripStatusLabel1.Text = string.Format("Completed detection of {0} objects in {1}.",
                objects.Length, sw.Elapsed);
        }

        private void loadImage_Click(object sender , EventArgs e)
        {
            var request = WebRequest.Create("http://169.254.120.154/seepicture");

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                Bitmap picture = new Bitmap(stream);
                pictureBox1.Image = picture;
            }

        }

        private void clearImage_Click(object sender , EventArgs e)
        {
            pictureBox1.Image = null;
        }

        private void close_Click(object sender , EventArgs e)
        {
            this.Close();
        }



    }
}
