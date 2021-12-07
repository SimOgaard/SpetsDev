using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private Sprite health;
    [SerializeField] private Sprite health_missing;

    [SerializeField] private float distance_x_offset;
    [SerializeField] private float distance_x_offset_recursive;
    [SerializeField] private float distance_y_offset;
    [SerializeField] private float distance_y_offset_recursive;

    private Image[] images;
    private int max_health;

    /// <summary>
    /// Updates max health by creating new health ui elements with right width, height and position.
    /// </summary>
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
            GameObject health_border_game_object = new GameObject("HealthImageBorderUI_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            health_border_game_object.transform.SetParent(transform, false);
            GameObject health_game_object = new GameObject("HealthImageUI_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            health_game_object.transform.SetParent(health_border_game_object.transform.transform, false);

            RectTransform health_border_rect_transform = health_border_game_object.GetComponent<RectTransform>();
            RectTransform health_rect_transform = health_game_object.GetComponent<RectTransform>();
            health_border_rect_transform.sizeDelta = new Vector2(13f, 12f);
            health_border_rect_transform.localPosition = new Vector3(x, y, 0f);
            health_rect_transform.sizeDelta = new Vector2(13f, 12f);
            x += distance_x_offset_recursive;
            y += distance_y_offset_recursive;

            health_border_game_object.GetComponent<Image>().sprite = health_missing;
            images[i] = health_game_object.GetComponent<Image>();
            images[i].sprite = health;
        }

        this.max_health = max_health;
    }

    /// <summary>
    /// Changes opacity of fill in heart ui image.
    /// </summary>
    public void UpdateCurrentHealth(int current_health)
    {
        for (int i = 0; i < max_health; i++)
        {
            images[i].color = current_health <= i ? new Color(0f, 0f, 0f, 0f) : Color.white;
        }
    }

    /// <summary>
    /// Sets max health and current health on game start.
    /// </summary>
    private void Start()
    {
        UpdateMaxHealth(4);
        UpdateCurrentHealth(3);
    }
}
