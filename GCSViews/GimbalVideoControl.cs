#if !LIB
// XXX: We need both the System.Drawing.Bitmap from System.Drawing and MissionPlanner.Drawing
extern alias Drawing;
using MPBitmap = Drawing::System.Drawing.Bitmap;
#else
using MPBitmap = System.Drawing.Bitmap;
#endif

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using MissionPlanner.Utilities;
using SkiaSharp;
using OpenTK.Input;
using log4net;

namespace MissionPlanner
{
    public partial class GimbalVideoControl : UserControl
    {
        // logger
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private GimbalControlPreferences preferences = new GimbalControlPreferences();

        private readonly GStreamer _stream = new GStreamer();

        public GimbalVideoControl()
        {
            InitializeComponent();

            _stream.OnNewImage += RenderFrame;

            loadPreferences();
        }

        private void loadPreferences()
        {
            var json = Settings.Instance["GimbalControlPreferences", ""];
            if (json != "")
            {
                try
                {
                    preferences = Newtonsoft.Json.JsonConvert.DeserializeObject<GimbalControlPreferences>(json);
                }
                catch (Exception ex)
                {
                    log.Error("Invalid GimbalControlPreferences, reverting to default", ex);
                }
            }

            setCameraControlPanelVisibility(preferences.ShowCameraControls);
        }

        private void setCameraControlPanelVisibility(bool visibility)
        {
            CameraLayoutPanel.Visible = visibility;
        }

        private void RenderFrame(object sender, MPBitmap image)
        {
            try
            {
                if (image == null)
                {
                    VideoBox.Image?.Dispose();
                    VideoBox.Image = null;
                    return;
                }

                var old = VideoBox.Image;
                VideoBox.Image = new Bitmap(
                    image.Width, image.Height, 4 * image.Width,
                    PixelFormat.Format32bppPArgb,
                    image.LockBits(Rectangle.Empty, null, SKColorType.Bgra8888).Scan0);

                old?.Dispose();
            }
            catch
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.OnNewImage -= RenderFrame;
                _stream.Stop();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void videoStreamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_stream.Start("rtspsrc location=rtsp://192.168.144.25:8554/main.264 latency=41 udp-reconnect=1 timeout=0 do-retransmission=false ! application/x-rtp ! decodebin3 ! queue leaky=2 ! videoconvert ! video/x-raw,format=BGRA ! appsink name=outsink sync=false");
            _stream.Start("videotestsrc ! video/x-raw, width=1280, height=720, framerate=30/1 ! videoconvert ! video/x-raw,format=BGRA ! appsink name=outsink");
        }
    }

    public class GimbalControlPreferences
    {
        // Keybindings for various actions
        public Keys SlewLeft { get; set; }
        public Keys SlewRight { get; set; }
        public Keys SlewUp { get; set; }
        public Keys SlewDown { get; set; }

        public Keys SlewFastModifier { get; set; }
        public Keys SlewSlowModifier { get; set; }

        public Keys ZoomIn { get; set; }
        public Keys ZoomOut { get; set; }

        public Keys TakePicture { get; set; }
        public Keys ToggleRecording { get; set; }
        public Keys StartRecording { get; set; }
        public Keys StopRecording { get; set; }

        public Keys ToggleLockFollow { get; set; }
        public Keys SetLock { get; set; }
        public Keys SetFollow { get; set; }

        public MouseButton MoveCameraToMouseLocation { get; set; }
        public MouseButton MoveCameraPOIToMouseLocation { get; set; }
        public MouseButton SlewCameraBasedOnMouse { get; set; }
        public MouseButton TrackObjectUnderMouse { get; set; }
        
        public Keys MoveCameraToMouseLocationModifier { get; set; }
        public Keys MoveCameraPOIToMouseLocationModifier { get; set; }
        public Keys SlewCameraBasedOnMouseModifier { get; set; }
        public Keys TrackObjectUnderMouseModifier { get; set; }

        // Speed settings
        public decimal SlewSpeedSlow { get; set; }
        public decimal SlewSpeedNormal { get; set; }
        public decimal SlewSpeedFast { get; set; }
        public int ZoomSpeed { get; set; }
        public decimal CameraFOV { get; set; }
        public decimal MouseSlewSpeed { get; set; }

        // Boolean options
        public bool UseScrollForZoom { get; set; }
        public bool DefaultLockedMode { get; set; }
        public bool UseFOVReportedByCamera { get; set; }
        public bool ShowCameraControls { get; set; }

        public GimbalControlPreferences()
        {
            SlewLeft = Keys.A;
            SlewRight = Keys.D;
            SlewUp = Keys.W;
            SlewDown = Keys.S;

            SlewSlowModifier = Keys.Control;
            SlewFastModifier = Keys.Shift;
            
            ZoomIn = Keys.E;
            ZoomOut = Keys.Q;
            
            TakePicture = Keys.F;
            ToggleRecording = Keys.R;
            StartRecording = Keys.None;
            StopRecording = Keys.None;

            ToggleLockFollow = Keys.L;
            SetLock = Keys.K;
            SetFollow = Keys.J;

            MoveCameraToMouseLocation = MouseButton.Left;
            MoveCameraPOIToMouseLocation = MouseButton.Left;
            SlewCameraBasedOnMouse = MouseButton.Left;
            TrackObjectUnderMouse = MouseButton.Left;

            MoveCameraToMouseLocationModifier = Keys.None;
            MoveCameraPOIToMouseLocationModifier = Keys.Shift;
            SlewCameraBasedOnMouseModifier = Keys.Alt;
            TrackObjectUnderMouseModifier = Keys.Control;

            // Default speed settings
            SlewSpeedSlow = 0.1m; // unitless [-1, 1]
            SlewSpeedNormal = 0.5m; // unitless [-1, 1]
            SlewSpeedFast = 1.0m; // unitless [-1, 1]
            ZoomSpeed = 5; // % per second
            CameraFOV = 50.0m; // degrees

            // Default boolean options
            UseScrollForZoom = true;
            DefaultLockedMode = false;
            UseFOVReportedByCamera = true;
            ShowCameraControls = true;
        }
    }
}