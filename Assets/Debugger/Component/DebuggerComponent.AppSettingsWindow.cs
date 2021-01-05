//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class AppSettingsWindow : ScrollableDebuggerWindowBase
        {
            private DebuggerComponent m_DebuggerComponent = null;
            private bool m_IsSDK = false;
            private bool m_LastIsSDK = false;
            //private bool m_CloseLimit = true;
            //private bool m_LastCloseLimit = false;
            private bool m_IsShowGuide = true;
            private bool m_LastShowGuide = false;
            private bool m_IsShowLog = true;
            private bool m_LastIsShowLog = false;

            GameObject AppMain;

            public override void Initialize(params object[] args)
            {
                m_DebuggerComponent = args[0] as DebuggerComponent;
                if (m_DebuggerComponent == null)
                {
                    throw new System.ArgumentException("Debugger component is invalid.");
                }

                m_IsSDK = m_LastIsSDK = PlayerPrefs.GetInt("Debugger.Console.IsSDK", 1) > 0;
                //m_CloseLimit = m_LastCloseLimit = PlayerPrefs.GetInt("Debugger.Console.CloseLimit", 1) > 0;
                m_IsShowGuide = m_LastShowGuide = PlayerPrefs.GetInt("Debugger.Console.ShowGuide", 1) > 0;
                m_IsShowLog = m_LastIsShowLog = PlayerPrefs.GetInt("Debugger.Console.IsShowLog", 1) > 0;
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>System Information</b>");
                GUILayout.BeginVertical("box");
                {
                    m_IsSDK = GUILayout.Toggle(m_IsSDK, "m_IsSDK", GUILayout.Width(200f));
                    GUILayout.Space(10);
                    //m_CloseLimit = GUILayout.Toggle(m_CloseLimit, "m_CloseLimit", GUILayout.Width(200f));
                    GUILayout.Space(10);
                    m_IsShowGuide = GUILayout.Toggle(m_IsShowGuide, "m_IsShowGuide", GUILayout.Width(200f));
                    GUILayout.Space(10);
                    m_IsShowLog = GUILayout.Toggle(m_IsShowLog, "m_IsShowLog", GUILayout.Width(200f));

                }
                GUILayout.EndVertical();
            }

             public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (m_LastIsSDK != m_IsSDK)
                {
                    m_LastIsSDK = m_IsSDK;
                    PlayerPrefs.SetInt("Debugger.Console.IsSDK", m_LastIsSDK ? 1 : 0);
                    SetSetting();
                }

                //if (m_LastCloseLimit != m_CloseLimit)
                //{
                //    m_LastCloseLimit = m_CloseLimit;
                //    PlayerPrefs.SetInt("Debugger.Console.CloseLimit", m_LastIsSDK ? 1 : 0);
                //    SetSetting();
                //}

                if (m_LastShowGuide != m_IsShowGuide)
                {
                    m_LastShowGuide = m_IsShowGuide;
                    PlayerPrefs.SetInt("Debugger.Console.ShowGuide", m_LastIsSDK ? 1 : 0);
                    SetSetting();
                }

                if (m_LastIsShowLog != m_IsShowLog)
                {
                    m_LastIsShowLog = m_IsShowLog;
                    PlayerPrefs.SetInt("Debugger.Console.IsShowLog", m_LastIsSDK ? 1 : 0);
                    SetSetting();
                }
            }

            public void SetSetting()
            {
                if (AppMain == null)
                {
                    AppMain = GameObject.Find("Main");
                }

                if (AppMain != null)
                {
                    AppMain.SendMessage("SetIsSDK", m_IsSDK);
                    AppMain.SendMessage("SetIsShowTeach", m_IsShowGuide);
                    //AppMain.SendMessage("SetIsShieldClose", m_CloseLimit);
                    AppMain.SendMessage("SetLogLevel", "0");
                }
            }
        }
    }
}
