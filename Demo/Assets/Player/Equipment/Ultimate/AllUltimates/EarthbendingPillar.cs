using System.Collections;
using UnityEngine;

public class EarthbendingPillar : MonoBehaviour
{
    private Vector3 end_point_1;
    private Vector3 end_point_2;
    private float time;
    private float speed;
    private float height;
    private bool has_reached_target = false;
    private bool is_still = false;

    // make one spawn pillar that follows player

    public void NEWSpawnPillar(Vector3 point, float height, float time, float speed, float width, Quaternion rotation)
    {
        if (Physics.Raycast(point + new Vector3(0f, 10f, 0f), Vector3.down, out hit_data, 20f, layer_mask))
        {
            transform.position = hit_data.point - rotation * new Vector3(0f, 0.5f + height, 0f);
        }

        transform.localScale = new Vector3(width, height * 2f, width);
        transform.rotation = rotation;

        this.end_point_1 = hit_data.point;
        this.end_point_2 = transform.position;
        this.time = time;
        this.speed = speed;
        this.height = height;
    }

    public void NEWSpawnPillar(Vector3 point, float height, float time, float speed, float width)
    {
        if (Physics.Raycast(point + new Vector3(0f, 10f, 0f), Vector3.down, out hit_data, 20f, layer_mask))
        {
            transform.position = hit_data.point - new Vector3(0f, 0.5f + height, 0f);
        }

        transform.localScale = new Vector3(width, height * 2f, width);
        transform.rotation = Quaternion.Euler(0f, 45f, 0f);

        this.end_point_1 = hit_data.point;
        this.end_point_2 = transform.position;
        this.time = time;
        this.speed = speed;
        this.height = height;
    }


    private RaycastHit hit_data;
    private int layer_mask = 1 << 12;

    public void SpawnPillar(Vector3 point, float under_ground_dist, float over_ground_dist, float time, float speed)
    {
        /*
        EarthbendingPillar earthbending_pillar = gameObject.AddComponent<EarthbendingPillar>();

        if (Physics.Raycast(point + new Vector3(0f, 10f, 0f), Vector3.down, out hit_data, 20f, layer_mask))
        {
            transform.position = hit_data.point - new Vector3(0f, under_ground_dist, 0f);
        }
        else
        {
            transform.position = point - new Vector3(0f, under_ground_dist, 0f);
        }

        transform.localScale = new Vector3(2f, 20f, 2f);
        transform.rotation = Quaternion.Euler(0f, 45f, 0f);

        Vector3 end_point = transform.position + new Vector3(0f, over_ground_dist, 0f);
        earthbending_pillar.SetValues(end_point, time, speed);
        */
    }


    private void Update()
    {
        if (is_still)
        {
            return;
        }

        float step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, end_point_1, step);

        if ((transform.position - end_point_1).sqrMagnitude < 0.01f)
        {
            ReachedTarget();
        }
    }

    private void ReachedTarget()
    {
        if (has_reached_target)
        {
            Destroy(gameObject);
        }
        has_reached_target = true;
        end_point_1 = end_point_2;
        StartCoroutine(StandStill(time));
    }

    private IEnumerator StandStill(float time)
    {
        is_still = true;
        yield return new WaitForSeconds(time);
        is_still = false;
    }
}
