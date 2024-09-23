using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VPackage.Editors.GameCapture
{
    public class GameCaptureWindow : EditorWindow
    {
        private Camera camera;
        private string folderPath;
        private string fileName = "image";
        private bool autoIncrement = true;
        private int currentIndex = 1;
        
        Vector2 scrollPos = Vector2.zero;

        [MenuItem("Tools/VPackage/Game Capture")]
        static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            GameCaptureWindow window = (GameCaptureWindow)GetWindow(typeof(GameCaptureWindow));
            window.titleContent = new GUIContent("Game Capture");
            window.Show();
        }

        private void OnEnable()
        {
            camera = camera = Camera.main;

            if (string.IsNullOrWhiteSpace(folderPath) == false && string.IsNullOrWhiteSpace(fileName) == false)
            {
                if (autoIncrement)
                {
                    CheckAndIncreaseCurrentIndex();
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            
            DrawTitle();
            DrawCamera();
            GUILayout.Space(10);
            DrawFolderPath();
            GUILayout.Space(10);
            DrawFileName();
            GUILayout.Space(10);
            DrawButtonCaptureTransparent();
            DrawButtonCapture();

            EditorGUILayout.EndScrollView();
        }

        void DrawTitle()
        {
            GUILayout.Space(10);
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            GUILayout.Label("Game Capture", style);
            GUILayout.Space(15);
        }

        void DrawCamera()
        {
            GUILayout.BeginHorizontal();
            camera = (Camera)EditorGUILayout.ObjectField("Camera", camera, typeof(Camera), true);
            if(GUILayout.Button("Auto Pick", GUILayout.Width(120)))
            {
                camera = Camera.main;
            }
            GUILayout.EndHorizontal();
        }

        void DrawFolderPath()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 15;
            GUILayout.Label("Folder", style);
            
            folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
            
            GUILayout.BeginHorizontal();
            
            //Draw Choose folder
            if (GUILayout.Button("Choose Folder"))
            {
                string lastFolder = "";
                if (string.IsNullOrWhiteSpace(folderPath) == false)
                {
                    if (Directory.Exists(folderPath))
                        lastFolder = folderPath;
                }
                
                string newFolderPath = EditorUtility.OpenFolderPanel("Choose Folder", lastFolder, "");
                if (string.IsNullOrEmpty(newFolderPath) == false && newFolderPath != folderPath)
                {
                    folderPath = newFolderPath;
                }
            }

            //Draw Go to folder
            bool oldEnable = GUI.enabled;
            GUI.enabled = string.IsNullOrWhiteSpace(folderPath) == false && Directory.Exists(folderPath);
            if(GUILayout.Button("Go To Folder"))
                EditorUtility.RevealInFinder(folderPath);
            GUI.enabled = oldEnable;
            
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void DrawFileName()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 15;
            GUILayout.Label("File", style);
            
            
            fileName = EditorGUILayout.TextField("File Name", fileName);

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                GUILayout.EndVertical();
                return;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                GUILayout.EndVertical();
                return;
            }
            

            bool newAutoIncrement = EditorGUILayout.Toggle("Auto Increment", autoIncrement);
            if (newAutoIncrement != autoIncrement)
            {
                if (newAutoIncrement == true)
                    CheckAndIncreaseCurrentIndex();
                
                autoIncrement = newAutoIncrement;
            }
            
            if(autoIncrement)
                currentIndex = EditorGUILayout.IntField("Index", currentIndex);


            string filePath = GenerateFilePath();
            if (File.Exists(filePath))
                EditorGUILayout.HelpBox("File exist -> will replace", MessageType.Warning);
            
            GUILayout.EndVertical();
        }

        bool CheckCondition()
        {
            if (camera == null)
            {
                EditorUtility.DisplayDialog("Camera empty", "Please pick a camera to capture", "Ok");
                return false;
            }

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                EditorUtility.DisplayDialog("Folder path empty", "Please pick a folder to capture", "Ok");
                return false;
            }
            
            if (Directory.Exists(folderPath) == false)
            {
                EditorUtility.DisplayDialog("Folder path not exist", "Please pick a folder exist to capture", "Ok");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(fileName))
            {
                EditorUtility.DisplayDialog("File name empty", "Please input a file name to capture", "Ok");
                return false;
            }

            return true;
        }

        void CheckAndIncreaseCurrentIndex()
        {
            string filePath = Path.Combine(folderPath, fileName + "_" + currentIndex + ".png");
            while (File.Exists(filePath))
            {
                currentIndex++;
                filePath = Path.Combine(folderPath, fileName + "_" + currentIndex + ".png");
            }
        }
        
        string GenerateFilePath()
        {
            if (autoIncrement)
            {
                return Path.Combine(folderPath, fileName + "_" + currentIndex + ".png");
            }
            else
            {
                return Path.Combine(folderPath, fileName + ".png");
            }
        }
        
        void DrawButtonCaptureTransparent()
        {
            if(GUILayout.Button("Capture Transparent", GUILayout.Height(25)) == false)
                return;
            
            if(CheckCondition() == false)
                return;

            GUI.FocusControl(null);

            var coroutineRunner = camera.gameObject.AddComponent<CoroutineRunner>();
            coroutineRunner.StartCoroutine(CaptureTransparentTimeline());
            
            
            
            IEnumerator CaptureTransparentTimeline()
            {
                yield return new WaitForEndOfFrame();
                string filePath = GenerateFilePath();
                GameCaptureCore.CaptureTransparentAndSave(camera, filePath);
                DestroyImmediate(coroutineRunner);

                if (autoIncrement)
                    CheckAndIncreaseCurrentIndex();
            }
        }

        
        void DrawButtonCapture()
        {
            if(GUILayout.Button("Capture", GUILayout.Height(25)) == false)
                return;
            
            if(CheckCondition() == false)
                return;
            
            GUI.FocusControl(null);
            
            var coroutineRunner = camera.gameObject.AddComponent<CoroutineRunner>();
            coroutineRunner.StartCoroutine(CaptureTimeline());
            
            
            
            IEnumerator CaptureTimeline()
            {
                yield return new WaitForEndOfFrame();
                string filePath = GenerateFilePath();
                GameCaptureCore.CaptureAndSave(camera, filePath);
                DestroyImmediate(coroutineRunner);

                if (autoIncrement)
                    CheckAndIncreaseCurrentIndex();
            }
        }
    }
}