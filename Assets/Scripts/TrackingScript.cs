using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class TrackingScript : MonoBehaviour
{
    [SerializeField] private Camera main;
    [SerializeField] private Transform model;

    private float lastX = 0;
    private float lastY = 0;

    private WebCamTexture cam;
    private Mat frame;
    private Mat mask;

    private OpenCvSharp.Point[][] contours;
    private HierarchyIndex[] hierarchy;

    void Start()
    {
        // Gathering all Active Camera Instances on the machine
        WebCamDevice[] devices = WebCamTexture.devices;

        cam = new WebCamTexture(devices[0].name);
        cam.Play();
    }

    void Update()
    {
        // Converting the WebCam Data into a Visible Frame on Screen
        frame = OpenCvSharp.Unity.TextureToMat(cam);
        detectColor();
        displayFeedback();
    }

    public void displayFeedback()
    {
        // Passing the Frame Generated as a Texture into GameObject <Plane>
        Texture feedback = OpenCvSharp.Unity.MatToTexture(frame);
        GetComponent<Renderer>().material.mainTexture = feedback;
    }

    public void detectColor()
    {
        Scalar lowerBound = new Scalar(110, 50, 50);
        Scalar upperBound = new Scalar(124, 256, 256);

        //Lower Bound: Blue [92, 0, 0] | Green [50, 20, 20] | Yellow [20, 80, 80]
        //Upper Bound: Blue [124, 256, 256] | Green [100, 255, 255] | Yellow [30, 255, 255]

        Mat hsv_image = frame.CvtColor(ColorConversionCodes.BGR2HSV);

        mask = hsv_image.InRange(lowerBound, upperBound);
        mask = mask.GaussianBlur(new Size(9, 9), 0);

        drawObjectBoundary();
    }

    public void drawObjectBoundary()
    {
        int index = 0;

        Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        
        foreach (OpenCvSharp.Point[] item in contours)
        {
            float area = (float) Cv2.ContourArea(item);

            if (area > 4000.0f)
            {
                Cv2.DrawContours(frame, contours, index, new Scalar(0, 0, 255), 2);
                OpenCvSharp.Moments select_item = Cv2.Moments(item);
                int cX = System.Convert.ToInt32(select_item.M10 / select_item.M00);
                int cY = System.Convert.ToInt32(select_item.M01 / select_item.M00);

                Cv2.Circle(frame, new Point(cX, cY), 10, new Scalar(0, 255, 0));
                spawnMovableGameObject(cX, cY);
            }
            else
            {
                index++;
            }
        }
    }

    public void spawnMovableGameObject(int x, int y)
    {
        Transform model_spawned;
        Vector3 pos = main.ScreenToWorldPoint(new Vector3(x, y, 15));

        float normX = Mathf.Clamp(pos.x - lastX, -1, 1);
        float normY = Mathf.Clamp(pos.y - lastY, -1, 1);

        if(GameObject.FindGameObjectWithTag("model") == null)
        {
            Vector3 initial_pos = new Vector3((model.position.x + normX), (model.position.y - normY), 1);
            Instantiate(model, initial_pos, model.rotation);
        }
        else
        {
            model_spawned = GameObject.FindGameObjectWithTag("model").gameObject.transform;
            model_spawned.position = new Vector3((model_spawned.position.x + normX), (model_spawned.position.y - normY), 1);
        }

        lastX = pos.x;
        lastY = pos.y;
    }

}
