//
// Frame Capturer
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OmniSARTechnologies.Helper.Graphics {
    public enum ImageFormat {
        JPG,
        PNG,
        EXR
    }

    public class FrameCapturer : MonoBehaviour {
        [Header("Capturing")]
        public bool continuousCapture = false;
        public string path = "captures";
        public string tag = "final";
        public int frameRate = 30;
        public ImageFormat outputFormat = ImageFormat.PNG;

        [Header("Off-Screen Capturing")]
        public bool captureOffScreen = false;
        public int offScreenWidth = 3840;
        public int offScreenHeight = 2160;

        private Camera m_currentCamera;
        public Camera currentCamera {
            get {
                if (!m_currentCamera) {
                    m_currentCamera = GetComponent<Camera>();
                }
                return m_currentCamera;
            }
        }

        private string GetCapturePath() {
#if UNITY_EDITOR
            return path;
#else
            return Path.Combine(Application.persistentDataPath, path);
#endif
        }

        private string GetCurrentFileName(int width, int height, string ext, string tag) {
            return Path.Combine(
                GetCapturePath(),
                string.Format(
                    "P[{0}]-S[{1}]{2}{3}-[W{4}xH{5}]-[F{6:D06}]-[T{7:D12}]{8}",
                    Application.productName,
                    SceneManager.GetActiveScene().name,
                    this.tag.Length > 0 ? "-t[" + this.tag + "]" : "",
                    tag.Length > 0 ? "-t[" + tag + "]" : "",
                    width,
                    height,
                    Time.frameCount,
                    (int)(Time.fixedUnscaledTime * 1000.0f),
                    ext
                )
            );
        }

        private string GetCurrentFileName(int width, int height, string ext, bool offScreen) {
            return GetCurrentFileName(width, height, ext, offScreen ? "offscreen" : "onscreen");
        }

        private static bool ForcePath(string path) {
            if (Directory.Exists(path)) {
                return true;
            }

            DirectoryInfo info = Directory.CreateDirectory(path);
            if (null == info) {
                return false;
            }

            return info.Exists;
        }

        private bool EnsureCapturePathAndReport() {
            string capturePath = GetCapturePath();

            if (!ForcePath(capturePath)) {
                Debug.LogError("Path '" + capturePath + "' does not exist and could not be created.");
                return false;
            }

            return true;
        }

    	private void Start() {
            EnsureCapturePathAndReport();
    	}
    	
        private static string ImageFormatToString(ImageFormat imageFormat) {
            return imageFormat.ToString();
        }

        private static string ImageFormatToExtension(ImageFormat imageFormat) {
            return "." + ImageFormatToString(imageFormat).ToUpperInvariant();
        }

        private static bool SaveTexture(Texture2D texture, string fileName, ImageFormat imageFormat) {
            if (!texture) {
                return false;
            }

            int maxFilex = 8;
            while (File.Exists(fileName) && (maxFilex --> 0)) {
                fileName += ImageFormatToExtension(imageFormat);
            }

            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }

            byte[] bytes = null;
            switch (imageFormat) { 
                case ImageFormat.JPG:
                    bytes = texture.EncodeToJPG();
                    break;

                case ImageFormat.PNG:
                    bytes = texture.EncodeToPNG();
                    break;

                case ImageFormat.EXR:
                    bytes = texture.EncodeToEXR();
                    break;

                default:
                    break;
            }

            if (null == bytes) {
                return false;
            }

            System.IO.File.WriteAllBytes(fileName, bytes);

            return File.Exists(fileName);
        }

        private static bool SaveTexture(RenderTexture texture, string fileName, ImageFormat imageFormat) {
            if (!texture) {
                return false;
            }

            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            if (!texture2D) {
                return false;
            }

            texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture2D.Apply();

            return SaveTexture(texture2D, fileName, imageFormat);
        }

        private bool CaptureOnScreenFrame() {
            string failureMsg = "On-screen frame capture failed.";

            if (!EnsureCapturePathAndReport()) {
                Debug.Log(failureMsg);
                return false;
            }

            string fileName = GetCurrentFileName(
                Screen.width,
                Screen.height,
                ImageFormatToExtension(outputFormat),
                offScreen: false
            );

            ScreenCapture.CaptureScreenshot(fileName);

            Debug.Log("Captured on-screen frame to '" + fileName + "'.");
            return true;
        }

        private bool CaptureOffScreenFrame(int width, int height) {
            string failureMsg = "Off-screen frame capture failed.";

            if (!EnsureCapturePathAndReport()) {
                Debug.Log(failureMsg);
                return false;
            }

            Camera cam = currentCamera;
            if (!cam) {
                Debug.Log(failureMsg);
                return false;
            }

            RenderTexture temp = cam.targetTexture;

            cam.targetTexture = RenderTexture.GetTemporary(width, height, 24);
            if (!cam.targetTexture) {
                Debug.Log(failureMsg);
                return false;
            }

            RenderTexture.active = cam.targetTexture;
            cam.Render();

            string fileName = GetCurrentFileName(
                                  width,
                                  height,
                                  ImageFormatToExtension(outputFormat),
                                  offScreen: true
                              );
            
            if (!SaveTexture(cam.targetTexture, fileName, outputFormat)) {
                Debug.Log(failureMsg);
                return false;
            }

            RenderTexture.ReleaseTemporary(cam.targetTexture);
            cam.targetTexture = temp;
            RenderTexture.active = temp;

            Debug.Log("Captured off-screen frame to '" + fileName + "'.");
            return true;
        }
    
        public void CaptureFrame(bool offScreen) {
            if (offScreen) {
                CaptureOffScreenFrame(offScreenWidth, offScreenHeight);
            } else {
                CaptureOnScreenFrame();
            }
        }

        public void CaptureFrame() {
            CaptureFrame(captureOffScreen);
        }

        public void CaptureFrameTagged(string tag) {
            this.tag = tag;
            CaptureFrame();
        }

        public void CaptureOffScreenFrameTagged(string tag) {
            this.tag = tag;
            CaptureOffScreenFrame(offScreenWidth, offScreenHeight);
        }

        public void CaptureOnScreenFrameTagged(string tag) {
            this.tag = tag;
            CaptureOnScreenFrame();
        }

        private void Capture() {
            Time.captureFramerate = continuousCapture ? frameRate : 0;

            if (!continuousCapture) {
                return;
            }

            CaptureFrame();
        }

        private void LateUpdate() {
            Capture();
        }
    }
}
