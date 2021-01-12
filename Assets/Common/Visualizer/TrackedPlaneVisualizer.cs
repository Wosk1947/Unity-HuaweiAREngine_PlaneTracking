namespace Common
{
    using System.Collections.Generic;
    using HuaweiARUnitySDK;
    using UnityEngine;
    using HuaweiARInternal;

    public class TrackedPlaneVisualizer : MonoBehaviour
    {

        private float V = 200.0f;
        private float timer = 0.0f;
        private int startgame=-1;
        private GameObject Lshoulder; 
        private float directionL = 1.0f;
        private GameObject Larm;
        private float directionL1 = -1.8f;
        private int hit = 0;
        private int endhit = 0;
        private GameObject Rshoulder;
        private GameObject Rarm;
        private GameObject temp1;
        private GameObject temp2;
        private string arm;
        
        
        
        private GameObject m_hwCube;
        private ARAnchor anchor;
        private List<ARHitResult> newPoints = new List<ARHitResult>();
        private Pose enemyPose;



        private static int s_planeCount = 0;

        private readonly Color[] k_planeColors = new Color[]
        {
            new Color(1.0f, 0.0f, 0.0f),
            new Color(0.5f,0.3f,0.9f),
            new Color(0.8f,0.4f,0.8f),
            new Color(0.5f,0.8f,0.4f),
            new Color(0.5f,0.9f,0.8f)
        };

        private ARPlane m_trackedPlane;

        // Keep previous frame's mesh polygon to avoid mesh update every frame.
        private List<Vector3> m_previousFrameMeshVertices = new List<Vector3>();
        private List<Vector3> m_meshVertices3D = new List<Vector3>();
        private List<Vector2> m_meshVertices2D = new List<Vector2>();

        private List<Color> m_meshColors = new List<Color>();

        private Mesh m_mesh;

        private MeshRenderer m_meshRenderer;


        public void Start()
        {
            startgame = 0;
        }
        
        public void Awake()
        {
            m_mesh = GetComponent<MeshFilter>().mesh;
            m_meshRenderer = GetComponent<MeshRenderer>();
            //m_meshRenderer.material.color=new Color(0,0,0, 0.0f );
        }

        public void Update()
        {
            if (startgame == 1)
            {
                timer+=Time.deltaTime;
                
                if (timer % 3.0f < 0.1)
                {
                    hit = 1;
                    endhit = 0;

                    if (((int)timer / 3) % 2 > 0.5f)
                    {
                        temp1 = Lshoulder;
                        temp2 = Larm;
                        arm = "Left";
                    }
                    else
                    {
                        temp1 = Rshoulder;
                        temp2 = Rarm;
                        arm = "Right";
                    }
                }

                if (hit == 1)
                {


                    if (temp1.transform.localRotation.eulerAngles.z > 77)
                    {
                        directionL = -1.0f;
                        directionL1 = 1.8f;
                        endhit = 1;
                    }

                    if (temp1.transform.localRotation.eulerAngles.z < 10)
                    {
                        directionL = 1.0f;
                        directionL1 = -1.8f;
                        if (endhit == 1)
                        {
                            hit = 0;
                        }
                    }

                    temp1.transform.Rotate(new Vector3(0, 0, Time.deltaTime * V * directionL), Space.Self);
                    temp2.transform.Rotate(new Vector3(0, 0, Time.deltaTime * V * directionL1), Space.Self);
                }
            }
            
            if (Input.touchCount > 0 && startgame==0) {
                newPoints=ARFrame.HitTest(Screen.width/2,Screen.height/2);
                anchor=newPoints[0].CreateAnchor();
                enemyPose=anchor.GetPose();
                m_hwCube.transform.position = new Vector3(enemyPose.position.x,enemyPose.position.y,enemyPose.position.z);
                startgame = 1;
            }
            
            
            if (m_trackedPlane == null)
            {
                return;
            }
            else if (m_trackedPlane.GetSubsumedBy() != null
                || m_trackedPlane.GetTrackingState() == ARTrackable.TrackingState.STOPPED)
            {
                Destroy(gameObject);
                return;
            }
            else if (m_trackedPlane.GetTrackingState()==ARTrackable.TrackingState.PAUSED) // whether to destory gameobject if not tracking
            {
                m_meshRenderer.enabled = false;
                return;
            }

            m_meshRenderer.enabled = true;
            _UpdateMeshIfNeeded();
        }

		public void Initialize(ARPlane plane)
        {
            m_hwCube= GameObject.Find("Cube");
            m_hwCube.SetActive(true);
            
            
            Lshoulder = GameObject.Find("Sphere");
            Lshoulder.SetActive(true);
            Larm = GameObject.Find("Sphere1");
            Larm.SetActive(true);
            
            Rshoulder = GameObject.Find("Sphere2");
            Rshoulder.SetActive(true);
            Rarm = GameObject.Find("Sphere3");
            Rarm.SetActive(true);
            
            
            
            
            m_trackedPlane = plane;
            m_meshRenderer.material.SetColor("_GridColor", k_planeColors[s_planeCount++ % k_planeColors.Length]);
            Update();
        }

        private void _UpdateMeshIfNeeded()
        {
            m_meshVertices3D.Clear();
            m_trackedPlane.GetPlanePolygon(m_meshVertices3D);

            if (_AreVerticesListsEqual(m_previousFrameMeshVertices, m_meshVertices3D))
            {
                return;
            }

            Pose centerPose = m_trackedPlane.GetCenterPose();
            for(int i = 0; i < m_meshVertices3D.Count; i++)
            {
                m_meshVertices3D[i] = centerPose.rotation * m_meshVertices3D[i] + centerPose.position;
            }

            Vector3 planeNormal = centerPose.rotation * Vector3.up;
            m_meshRenderer.material.SetVector("_PlaneNormal", planeNormal);

            m_previousFrameMeshVertices.Clear();
            m_previousFrameMeshVertices.AddRange(m_meshVertices3D);

            m_meshVertices2D.Clear();
            m_trackedPlane.GetPlanePolygon(ref m_meshVertices2D);

            Triangulator tr = new Triangulator(m_meshVertices2D);

            m_mesh.Clear();
            m_mesh.SetVertices(m_meshVertices3D);
            m_mesh.SetIndices(tr.Triangulate(), MeshTopology.Triangles, 0);
            m_mesh.SetColors(m_meshColors);

        }

        private bool _AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
        {
            if (firstList.Count != secondList.Count)
            {
                return false;
            }

            for (int i = 0; i < firstList.Count; i++)
            {
                if (firstList[i] != secondList[i])
                {
                    return false;
                }
            }

            return true;
        }
        
        /*
        public void OnGUI()
        {
            GUIStyle bb = new GUIStyle();
            bb.normal.background = null;
            bb.normal.textColor = new Color(34f / 255f, 139f / 255f, 34f / 255f);
            bb.fontSize = 100;

            
                GUI.Label(new Rect(0, 0, 200, 200), string.Format(arm+" {0}",
                    timer), bb);
            
        }
        */
        
    }
}
