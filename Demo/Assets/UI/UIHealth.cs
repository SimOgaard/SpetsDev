using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField]
    private Sprite health;
    [SerializeField]
    private Sprite health_missing;

    [SerializeField]
    private float distance_x_offset;
    [SerializeField]
    private float distance_x_offset_recursive;
    [SerializeField]
    private float distance_y_offset;

    private Image[] images;

    private int max_health;
    public void UpdateMaxHealth(int max_health)
    {
        images = new Image[max_health];

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float x = distance_x_offset;
        float y = distance_y_offset;

        for (int i = 0; i < max_health; i++)
        {
            GameObject health_game_object = new GameObject("HealthImageUI_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            health_game_object.transform.SetParent(transform, false);

            RectTransform rect_transform = health_game_object.GetComponent<RectTransform>();
            rect_transform.sizeDelta = new Vector2(64f, 64f);
            rect_transform.localPosition = new Vector3(x, y, 0f);
            x += distance_x_offset_recursive;

            images[i] = health_game_object.GetComponent<Image>();
            images[i].sprite = health;
        }

        this.max_health = max_health;
    }

    public void UpdateCurrentHealth(int current_health)
    {
        for (int i = 0; i < max_health; i++)
        {
            images[i].sprite = current_health <= i ? health_missing : health;
        }
    }

    private void Start()
    {
        UpdateMaxHealth(3);
        UpdateCurrentHealth(2);
    }
}
