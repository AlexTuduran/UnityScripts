//
// Frame Capturer Editor
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using UnityEngine;
using UnityEditor;
using OmniSARTechnologies.Helper.Graphics;

namespace OmniSARTechnologies.Helper.Graphics {
    [CustomEditor(typeof(FrameCapturer))]
    public class FrameCapturerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            FrameCapturer m_Component = target as FrameCapturer;
            if (!m_Component) {
                return;
            }

            GUILayout.Space(4);
            GUILayout.BeginHorizontal(); {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Capture Frame Now", GUILayout.MinHeight(24), GUILayout.MaxWidth(192))) {
                    m_Component.CaptureFrame();
                }
                GUILayout.FlexibleSpace();
            } GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
    }
}