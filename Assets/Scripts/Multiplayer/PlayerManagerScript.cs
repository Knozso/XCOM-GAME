using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManagerScript : NetworkBehaviour
{
    public GameObject gridObject;

    public override void OnStartServer()
    {
        base.OnStartServer();
        CmdInitGrid();
        CmdPlaceUnits();
        Debug.Log("Server Started");
    }

    [Command]
    void CmdInitGrid()
    {
        GameObject gridObj = Instantiate(gridObject, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        //gridObj.GetComponent<Grid>().InitGrid();
        NetworkServer.Spawn(gridObj);
    }

    [Command]
    void CmdPlaceUnits()
    {
        //gridObject.GetComponent<Grid>().AddUnits();
    }

    /*
    public override void On()
    {
        GameObject gridObj = Instantiate(gridObject, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        gridObj.GetComponent<Grid>().InitGrid();
    }
    */
    /*
    [Server]
    public override void OnClien()
    {

        //NetworkServer.Spawn(gridObj);
        //gridObj.GetComponent<Grid>().InitGrid();
        //gridObject.GetComponent<Grid>().InitGrid();
        //Instantiate(gridObject);
    }
    */
}
