using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private Sprite health;
    [SerializeField] private Sprite healthMissing;

    [SerializeField] private float distanceXOffset;
    [SerializeField] private float distanceXOffsetRecursive;
    [SerializeField] private float distanceYOffset;
    [SerializeField] private float distanceYOffsetRecursive;

    private Image[] images;
    private int maxHealth;

    /// <summary>
    /// Updates max health by creating new health ui elements with right width, height and position.
    /// </summary>
    public void UpdateMaxHealth(int maxHealth)
    {
        images = new Image[maxHealth];

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float x = distanceXOffset;
        float y = distanceYOffset;

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject healthBorderGameObject = new GameObject("HealthImageBorderUI_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            healthBorderGameObject.transform.SetParent(transform, false);
            GameObject healthGameObject = new GameObject("HealthImageUI_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            healthGameObject.transform.SetParent(healthBorderGameObject.transform.transform, false);

            RectTransform healthBorderRectTransform = healthBorderGameObject.GetComponent<RectTransform>();
            RectTransform healthRectTransform = healthGameObject.GetComponent<RectTransform>();
            healthBorderRectTransform.sizeDelta = new Vector2(13f, 12f);
            healthBorderRectTransform.localPosition = new Vector3(x, y, 0f);
            healthRectTransform.sizeDelta = new Vector2(13f, 12f);
            x += distanceXOffsetRecursive;
            y += distanceYOffsetRecursive;

            healthBorderGameObject.GetComponent<Image>().sprite = healthMissing;
            images[i] = healthGameObject.GetComponent<Image>();
            images[i].sprite = health;
        }

        this.maxHealth = maxHealth;
    }

    /// <summary>
    /// Changes opacity of fill in heart ui image.
    /// </summary>
    public void UpdateCurrentHealth(int currentHealth)
    {
        for (int i = 0; i < maxHealth; i++)
        {
            images[i].color = currentHealth <= i ? new Color(0f, 0f, 0f, 0f) : Color.white;
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
