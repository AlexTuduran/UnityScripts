using UnityEngine;
using UnityEditor;

using UnityStandardAssets.ImageEffects;

[InitializeOnLoad]
public class EditorCamera : MonoBehaviour {
    static public bool aaEnabled = true;
    static public bool moreAAEnabled = true;
    static public bool insaneAAEnabled = false;

    static EditorCamera() {
        SceneView.onSceneGUIDelegate += Delegate;
    }

    static void Delegate(SceneView sv) {
        if (!aaEnabled) {
            return;
        }

        if (Event.current.type != EventType.Layout) {
            return;
        }
            
        if (!Camera.main) {
            return;
        }

        Camera cam = sv.camera;

        if (cam.GetComponent<Antialiasing>()) {
        }
        else {
            Antialiasing aa = cam.gameObject.AddComponent<Antialiasing>();
            aa.mode = AAMode.FXAA2;

            if (moreAAEnabled) {
                Antialiasing aa2 = cam.gameObject.AddComponent<Antialiasing>();
                aa2.mode = AAMode.FXAA2;

                if (insaneAAEnabled) {
                    Antialiasing aa3 = cam.gameObject.AddComponent<Antialiasing>();
                    aa3.mode = AAMode.FXAA2;
                }
            }
        }
    }
}