using System.IO;
using UnityEngine;

namespace VPackage.Editors.GameCapture
{
    public static class GameCaptureCore
    {
        #region Handle

        //pColorWhenBlack!=Color.clear
        static Color GetColor(Color pColorWhenBlack,Color pColorWhenWhite)
        {
            float lAlpha = GetAlpha(pColorWhenBlack.r, pColorWhenWhite.r);
            return new Color(
                pColorWhenBlack.r / lAlpha,
                pColorWhenBlack.g / lAlpha,
                pColorWhenBlack.b / lAlpha,
                lAlpha);
        }


        //           Color*Alpha      Color   Color+(1-Color)*(1-Alpha)=1+Color*Alpha-Alpha
        //0----------ColorWhenZero----Color---ColorWhenOne------------1
        static float GetAlpha(float pColorWhenZero, float pColorWhenOne)
        {
            //pColorWhenOne-pColorWhenZero=1-Alpha
            return 1 + pColorWhenZero - pColorWhenOne;
        }

        static Texture2D CaptureView(Rect rect)
        {
            Texture2D textureOut = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            textureOut.ReadPixels(rect, 0, 0, false);
            return textureOut;
        }

        #endregion

        #region Capture Transparent

        public static Texture2D CaptureTransparentInRect(Camera camera, Rect rect)
        {
            Texture2D textureOut;
            var preClearFlags = camera.clearFlags;
            var preBackgroundColor = camera.backgroundColor;
            {
                camera.clearFlags = CameraClearFlags.Color;

                //make two captures with black and white background
                camera.backgroundColor = Color.black;
                camera.Render();
                var blackBackgroundCapture = CaptureView(rect);

                camera.backgroundColor = Color.white;
                camera.Render();
                var whiteBackgroundCapture = CaptureView(rect);

                for (int x = 0; x < whiteBackgroundCapture.width; ++x)
                {
                    for (int y = 0; y < whiteBackgroundCapture.height; ++y)
                    {
                        Color colorWhenBlack = blackBackgroundCapture.GetPixel(x, y);
                        Color colorWhenWhite = whiteBackgroundCapture.GetPixel(x, y);
                        if (colorWhenBlack != Color.clear)
                        {
                            //set real color
                            whiteBackgroundCapture.SetPixel(x, y,
                                GetColor(colorWhenBlack, colorWhenWhite));
                        }
                    }
                }
                whiteBackgroundCapture.Apply();
                textureOut = whiteBackgroundCapture;
                Object.DestroyImmediate(blackBackgroundCapture);
            }
            camera.backgroundColor = preBackgroundColor;
            camera.clearFlags = preClearFlags;
            return textureOut;
        }
        
        /// <summary>
        /// Capture a screenshot(not include GUI)
        /// </summary>
        public static Texture2D CaptureTransparent(Camera camera)
        {
            return CaptureTransparentInRect(camera, new Rect(0f, 0f, Screen.width, Screen.height));
        }
        
        public static void CaptureTransparentAndSave(Camera camera, string filePath)
        {
            var texture = CaptureTransparent(camera);
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(fileStream);
                    writer.Write(texture.EncodeToPNG());
                }
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        #endregion
        
        #region Capture

        public static Texture2D CaptureInRect(Camera camera, Rect rect)
        {
            Color oldBgColor = camera.backgroundColor;

            Color bgColor = camera.backgroundColor;
            bgColor.a = 1;
            camera.backgroundColor = bgColor;
            camera.Render();
            Texture2D texture = CaptureView(rect);
            camera.backgroundColor = oldBgColor;

            return texture;
        }
        
        public static Texture2D Capture(Camera camera)
        {
            return CaptureInRect(camera, new Rect(0f, 0f, Screen.width, Screen.height));
        }

        public static void CaptureAndSave(Camera camera, string filePath)
        {
            var texture = Capture(camera);
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(fileStream);
                    writer.Write(texture.EncodeToPNG());
                }
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        #endregion
    }
}