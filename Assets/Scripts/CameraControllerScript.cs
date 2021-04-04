using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    private Camera _thisCamera;
    public float _speed = 10.0f;
    public float _rotateSpeed = 10.0f;
    public float _zoomInSpeed = 2.0f;
    public float _maxZoomIn = 5.0f;
    public float _maxZoomOut = 20.0f;

    void Start()
    {
        Stepper.Instance().TurnChanged += (p) => TurnChanged(p);
        Stepper.Instance().SelectedUnitChanged += (u) => UnitChanged(u);
        _thisCamera = GetComponent<Camera>();
    }

    void Update()
    {
        transform.parent.position = new Vector3(transform.parent.position.x, 0, transform.parent.position.z);
        
        //Camera movement
        if (Input.GetKey(KeyCode.D)) transform.parent.Translate(new Vector3(_speed * Time.deltaTime, 0, 0), Space.Self);
        if (Input.GetKey(KeyCode.A)) transform.parent.Translate(new Vector3(-_speed * Time.deltaTime, 0, 0), Space.Self);
        if (Input.GetKey(KeyCode.W)) transform.parent.Translate(new Vector3(0, 0, _speed * Time.deltaTime), Space.Self);
        if (Input.GetKey(KeyCode.S)) transform.parent.Translate(new Vector3(0, 0, _speed * -Time.deltaTime), Space.Self);

        //Camera rotation
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            transform.parent.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * _rotateSpeed, 0));
            var transformRotation = transform.parent.rotation;
            var y = transformRotation.eulerAngles.y;
            transform.parent.rotation = Quaternion.Euler(30, y, 0);
        }

        //Camera zoom in
        if ((Input.mouseScrollDelta.y<0 && transform.localPosition.z>-_maxZoomOut) || (Input.mouseScrollDelta.y>0 && transform.localPosition.z<-_maxZoomIn))
        {
            Vector3 cameraLocalPos = transform.localPosition;
            cameraLocalPos.z += Input.mouseScrollDelta.y * _zoomInSpeed;
            transform.localPosition = cameraLocalPos;
        }

    }

    private void TurnChanged(Player currentPlayer)
    {
        Unit unit = currentPlayer.Units[0];
        Vector3 pos = unit.CurrentCell.WorldPosition;
        Vector3 newPos = new Vector3(pos.x, 0, pos.z);
        transform.parent.position=new Vector3(0, 0, 0);
        _thisCamera.transform.parent.position = newPos;
        _thisCamera.transform.LookAt(unit.CurrentCell.WorldPosition);
    }

    private void UnitChanged(Unit unit)
    {
        Vector3 pos = unit.CurrentCell.WorldPosition;
        Vector3 newPos = new Vector3(pos.x, 0, pos.z);
        if (Camera.main != null)
        {
            Camera.main.transform.parent.position = newPos;
            Camera.main.transform.LookAt(transform);
        }
    }
}
