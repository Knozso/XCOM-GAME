using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GrenadeMovingScript : NetworkBehaviour
{

    protected float time;

    public Cell startCell;
    public Cell endCell;

    private bool canMove = false;

    // Update is called once per frame
    void Update()
    {
        if(canMove)
        {
            time += Time.deltaTime;

            Vector3 start = new Vector3(startCell.WorldPosition.x, 1f, startCell.WorldPosition.z);
            Vector3 end = new Vector3(endCell.WorldPosition.x, 1f, endCell.WorldPosition.z);

            Vector3 pos = Parabola(start, end, 7f, time/2);
            transform.position = pos;
            if ((pos-end).magnitude < new Vector3(1f, 1f, 1f).magnitude)
            {
                GameObject timer = gameObject.transform.Find("Timer").gameObject;
                timer.SetActive(false);

                GameObject explosion = gameObject.transform.Find("Explosion").gameObject;
                explosion.SetActive(true);
                ExplosionRpc();

                for(int i = endCell.GetGridX() - 1; i <= endCell.GetGridX() + 1; i++)
                {
                    for(int j = endCell.GetGridY() - 1; j<= endCell.GetGridY() + 1; j++)
                    {
                        if (i>=0 && j>=0 && i<Grid.grid.GetLength(0) && j<Grid.grid.GetLength(1))
                        {
                            Cell targetCell = Grid.grid[i, j];
                            foreach (Player player in Stepper.Instance().Players)
                            {
                                foreach (Unit unit in player.Units)
                                {
                                    if (unit.CurrentCell.Equals(targetCell))
                                    {
                                        unit.HealthLost(3);
                                        unit.Hit();
                                    }
                                }
                            }
                            foreach(GameObject go in targetCell.MapElements)
                            {
                                Destroy(go);
                                Debug.Log(go);
                            }
                        }
                    }
                }

                canMove = false;
            }
        }
    }

    [ClientRpc]
    private void ExplosionRpc()
    {
        GameObject timer = gameObject.transform.Find("Timer").gameObject;
        timer.SetActive(false);

        GameObject explosion = gameObject.transform.Find("Explosion").gameObject;
        explosion.SetActive(true);
    }

    public void StartMovement(Cell startCell, Cell endCell)
    {
        this.startCell = startCell;
        this.endCell = endCell;
        this.canMove = true;
    }

    public Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }
}
