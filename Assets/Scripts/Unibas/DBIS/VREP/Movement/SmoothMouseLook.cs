using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     by http://wiki.unity3d.com/index.php/SmoothMouseLook
/// </summary>
[AddComponentMenu("Camera-Control/Smooth Mouse Look")]
public class SmoothMouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float frameCounter = 20;
    public float maximumX = 360F;
    public float maximumY = 60F;

    public float minimumX = -360F;

    public float minimumY = -60F;

    private Quaternion originalRotation;

    private readonly List<float> rotArrayX = new List<float>();

    private readonly List<float> rotArrayY = new List<float>();

    private float rotationX;
    private float rotationY;
    private float rotAverageX;
    private float rotAverageY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    private void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            rotAverageY = 0f;
            rotAverageX = 0f;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            rotArrayY.Add(rotationY);
            rotArrayX.Add(rotationX);

            if (rotArrayY.Count >= frameCounter) rotArrayY.RemoveAt(0);
            if (rotArrayX.Count >= frameCounter) rotArrayX.RemoveAt(0);

            for (var j = 0; j < rotArrayY.Count; j++) rotAverageY += rotArrayY[j];
            for (var i = 0; i < rotArrayX.Count; i++) rotAverageX += rotArrayX[i];

            rotAverageY /= rotArrayY.Count;
            rotAverageX /= rotArrayX.Count;

            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            var yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            var xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotAverageX = 0f;

            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            rotArrayX.Add(rotationX);

            if (rotArrayX.Count >= frameCounter) rotArrayX.RemoveAt(0);
            for (var i = 0; i < rotArrayX.Count; i++) rotAverageX += rotArrayX[i];
            rotAverageX /= rotArrayX.Count;

            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            var xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {
            rotAverageY = 0f;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

            rotArrayY.Add(rotationY);

            if (rotArrayY.Count >= frameCounter) rotArrayY.RemoveAt(0);
            for (var j = 0; j < rotArrayY.Count; j++) rotAverageY += rotArrayY[j];
            rotAverageY /= rotArrayY.Count;

            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

            var yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
        }
    }

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
        originalRotation = transform.localRotation;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if (angle >= -360F && angle <= 360F)
        {
            if (angle < -360F) angle += 360F;
            if (angle > 360F) angle -= 360F;
        }

        return Mathf.Clamp(angle, min, max);
    }
}