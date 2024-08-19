using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Camera : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] float speed = 350f;
    [SerializeField] float maxTiltAngle = 85f;
    [SerializeField] RectTransform ignoreInputFrom;
    [SerializeField] float zoomFactor = 7f;

    Vector3 prev;

    private void Start() {
        prev = Input.mousePosition;    
    }

    
    void Update()
    {
        

        // GET MOVEDIR IN PIXEL COORDINATES
        Vector3 movedir = Vector3.zero;
        if (Input.GetMouseButton(0)) {
            if (RectTransformUtility.RectangleContainsScreenPoint(ignoreInputFrom, Input.mousePosition))
                return;
            movedir = Input.mousePosition - prev;
            movedir = Vector3.Normalize(movedir) * Time.deltaTime;
        }
        
        // ROTATE AROUND Y AXIS
        transform.position = Quaternion.Euler(0, movedir.x * speed, 0) * transform.position;

        // CALCULATE ROTATION AXIS ON XZ PLANE 
        Vector3 positionXZplane = new Vector3(transform.position.x, 0, transform.position.z);
        var angleHor = Vector3.Angle(positionXZplane, new Vector3(0, 0, 1));
        if (positionXZplane.x >= 0)
            angleHor = 360 - angleHor;
        var rotAxis = Quaternion.Euler(0, -angleHor, 0) * new Vector3(1, 0, 0);

        // ROTATE AROUND CALCULATED AXIS ON XZ, BUT DONT APPLY ROTATION TO TRANSFORM YET
        var position = Quaternion.AngleAxis(speed * movedir.y, rotAxis) * transform.position;

        // ONLY APPLY ROTATION IF VERTICAL ANGLE WOULD BE SMALLER THAN MAXTILTANGLE
        positionXZplane = new Vector3(position.x, 0, position.z);
        float angleVert = Vector3.Angle(positionXZplane, position);
        if (Mathf.Abs(angleVert) < maxTiltAngle)
            transform.position = position;

        // zooming functionality
        if (Input.mouseScrollDelta.y != 0) {
            var lPos = transform.localPosition;
            lPos += transform.forward * zoomFactor * Time.deltaTime * Input.mouseScrollDelta.y;
            transform.localPosition = lPos;
        }


        prev = Input.mousePosition;
        
        transform.LookAt(target);
    }
}
