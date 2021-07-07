using Chess.LODGroupIJob.JobSystem;
using Chess.LODGroupIJob.Slider;
using Chess.LODGroupIJob.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Chess.LODGroupIJob
{
    [CustomEditor(typeof(LODGroup))]
    [CanEditMultipleObjects]
    public class LODGroupEditor : LODEditorWindow
    {
        SceneCameraManager m_SceneCameraManager;

        LODSlider m_LODSlider;

        LODGroup m_LODGroup;

        GameObject m_SelectObj;

        Event m_LastEvent;

        //��Ҫɾ����renderer
        List<Renderer> m_WillDelete;

        public LODGroup LODGroup { get => m_LODGroup; set => m_LODGroup = value; }

        //dir
        Object m_PathDir;
        private void OnEnable()
        {
            m_LODGroup = (LODGroup)target;
            m_LODSlider = new LODSlider(this, true, LODUtils.kLODCulled);
            m_SceneCameraManager = new SceneCameraManager(m_LODGroup, m_LODSlider.SlideCursor);
            m_WillDelete = new List<Renderer>();
            FirstAwake();
            RefreshLOD();

            EditorApplication.update -= Updata;
            EditorApplication.update += Updata;
        }
        private void OnDestroy()
        {
            EditorApplication.update -= Updata;
        }

        //��һ�δ�����ˢ��LOD����
        void FirstAwake()
        {
            if (m_LODGroup.lodCount == 0)
            {
                LOD[] lods = new LOD[3];
                lods[0] = new LOD(0.6f);
                lods[1] = new LOD(0.3f);
                lods[2] = new LOD(0.1f);
                lods[0].Priority = 0;
                lods[1].Priority = 1;
                lods[2].Priority = 2;
                m_LODGroup.SetLODs(lods);
                m_LODGroup.RecalculateBounds();
            }
        }
        //ˢ��lod��
        public void RefreshLOD()
        {
            m_LODSlider.ClearRange();
            LOD[] lods = m_LODGroup.GetLODs();
            for (int i = 0; i < lods.Length; i++)
            {
                m_LODSlider.InsertRange(LODUtils.kLODNames[i], lods[i]);
            }
        }
        void Updata()
        {
            m_LODSlider.Updata(m_LastEvent);
        }
        public override void OnInspectorGUI()
        {
            m_LastEvent = Event.current;
            m_SceneCameraManager.OnInspectorGUI();
            m_LODSlider.Draw();

            
            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.BeginVertical("box", GUILayout.Width(150));
            RefreshAndStreaming();
            LODStreamingOpreat();
            EditorGUILayout.EndVertical();
            LODDataOperat();
            
            EditorGUILayout.EndHorizontal();

        }
        //ˢ�°�Χ�ͺ���ʽ����
        void RefreshAndStreaming()
        {
            string nullSearch = null;
            DrawHeader("�ܲ���", ref nullSearch, 0, true);
            if (GUILayout.Button("ˢ�°�Χ��"))
            {
                m_LODGroup.RecalculateBounds();
                m_LastEvent.Use();
            }
            EditorGUILayout.BeginHorizontal();
            if (m_PathDir != null && GUILayout.Button("һ����ʽ����"))
            {
                LOD[] lods = m_LODGroup.GetLODs();
                for(int i = 0; i < lods.Length; i++)
                {
                    ExportLODAsset(i, lods[i]);
                }
                EditorUtility.SetDirty(m_LODGroup);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("һ���˻���ʽ"))
            {
                LOD[] lods = m_LODGroup.GetLODs();
                for (int i = 0; i < lods.Length; i++)
                {
                    ImportLODAsset(i, lods[i]);
                }
                EditorUtility.SetDirty(m_LODGroup);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndHorizontal();

            m_PathDir = EditorGUILayout.ObjectField("��ʽ·��:", m_PathDir, typeof(Object));
        }
        //lod������
        void LODDataOperat()
        {
            int index = m_LODSlider.SelectedShowIndex;
            if (index == -1)
                return;

            string nullSearch = null;
            
            //��Χ���б䣬��Ҫˢ��
            if (m_LODSlider.RefreshBounds)
            {
                m_LODGroup.RecalculateBounds();
            }
            //��򻬶��������䶯����Ҫˢ��job����
            if(m_LODSlider.RefreshDrag)
            {
                LODGroupManager.Instance.Dirty = true;
            }
            LOD[] lods = m_LODGroup.GetLODs();
            if (lods[index].Streaming)
            {
                EditorGUILayout.BeginVertical("box");
                DrawHeader("RenderersAddress", ref nullSearch, 0, true);
                GUILayout.Label(lods[index].Address);
                EditorGUILayout.EndVertical();
                return;
            }

            var renderers = new List<Renderer>();
            if (lods[index].Renderers == null)
                return;

            //��ÿ��LOD�ϵ�����
            renderers.AddRange(lods[index].Renderers);
            
            EditorGUILayout.BeginVertical("box");
            if (renderers.Count > 0)
            {
                DrawHeader("Renderers", ref nullSearch, 0, true);
                m_WillDelete.Clear();
                //��ʾ��LOD�ڵ�Renderer
                foreach (var renderer in renderers)
                {
                    var obj = EditorGUILayout.ObjectField(renderer, typeof(GameObject), GUILayout.Width(150));
                    if (obj == null)
                    {
                        m_WillDelete.Add(renderer);
                    }
                }
                bool change = m_WillDelete.Count > 0 ? true : false;
                foreach (var renderer in m_WillDelete)
                {
                    if (!renderer.Equals(null))
                        renderer.enabled = true;
                    renderers.Remove(renderer);
                }

                if (change)
                {
                    lods[index].Renderers = renderers.ToArray();
                    List<Collider> l = new List<Collider>();
                    foreach(var r in renderers)
                    {
                       var c = r.GetComponent<Collider>();
                        if (c)
                            l.Add(c);
                    }
                    lods[index].Colliers = l.ToArray();
                    m_LODGroup.RecalculateBounds();
                }

            }
            EditorGUILayout.EndVertical();   
            
        }
       
        void LODStreamingOpreat()
        {
            int index = m_LODSlider.SelectedShowIndex;
            if (index == -1)
                return;
            GUILayout.Space(10);
            string nullSearch = null;
            DrawHeader("LOD" + index, ref nullSearch, 0, true);

            LOD[] lods = m_LODGroup.GetLODs();
            var lod = lods[index];

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            lod.Priority = EditorGUILayout.IntField("��������Ȩ��:", lod.Priority);
            if (lod.Streaming)
            {
                if (GUILayout.Button("�˻���ʽ"))
                {
                    ImportLODAsset(index, lod);
                    EditorUtility.SetDirty(m_LODGroup);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                if (m_PathDir != null && GUILayout.Button("��ʽ����"))
                {
                    ExportLODAsset(index, lod);
                    EditorUtility.SetDirty(m_LODGroup);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            EditorGUILayout.EndVertical();
        }
        void ExportLODAsset(int index, LOD lod)
        {
            var renderers = lod.Renderers;
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogError("LOD:"+ index+ "   û��Renderers");
                return;
            }
            GameObject lodObj = new GameObject();
            var sid = SerialIdManager.Instance.GetSid();
            sid = sid.Replace('/', '_');
            sid = sid.Replace(':', '_');
            
            lodObj.name = LODGroup.name + sid;
            lodObj.transform.parent = LODGroup.transform;
            lodObj.transform.localPosition = Vector3.zero;
            foreach (var rd in renderers)
            {
                rd.transform.parent = lodObj.transform;
                rd.enabled = true;
            }
            string path  = AssetDatabase.GetAssetPath(m_PathDir);
            string savePath = Path.Combine(path, lodObj.name + ".prefab").Replace('\\','/');
            savePath = savePath.Replace("[\\]", "/");
            PrefabUtility.SaveAsPrefabAsset(lodObj, savePath);

            GameObject.DestroyImmediate(lodObj);
            lod.Renderers = null;
            lod.Colliers = null;
            lod.Address = savePath;
            lod.CurrentState = State.UnLoaded;
            lod.Streaming = true;
        }
        void ImportLODAsset(int index, LOD lod)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(lod.Address);
            if (obj)
            {
                obj = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                obj.transform.parent = LODGroup.transform;
                obj.transform.localPosition= Vector3.zero;
                lod.Renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var o in lod.Renderers)
                {
                    o.transform.parent = LODGroup.transform;
                }
                GameObject.DestroyImmediate(obj);
                AssetDatabase.DeleteAsset(lod.Address);

            }

            lod.Address = null;
            lod.Streaming = false;
        }
    }
}