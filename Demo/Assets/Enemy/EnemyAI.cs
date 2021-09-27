using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds general functionality that all enemies should inherrent from.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [HideInInspector] public Agent agent;
    [HideInInspector] public Transform player_transform;

    [SerializeField] private float health_remove_speed = 0.5f;
    [SerializeField] private float starting_health;
    private float _current_health;
    public float current_health
    {
        get { return _current_health; }
        private set { _current_health = Mathf.Clamp(value, 0f, starting_health); UpdateHealthBar(); }
    }

    private void Awake()
    {
        player_transform = GameObject.Find("Player").transform;
    }

    private void Start()
    {
        agent = GetComponent<Agent>();
        InitHealthBar();
        current_health = starting_health;
    }

    private void Update()
    {
        current_animated_float_value -= Time.deltaTime * health_remove_speed;
    }

    [SerializeField] private Transform health_bar_transform;
    private Material health_material;
    private Material removed_health_material;
    private Material no_health_material;

    private float health_bar_slider_value = 1;

    [SerializeField] private Vector2 health_bar_pixel_size = Vector2.one;

    /// <summary>
    /// Inits health bar to correct size.
    /// </summary>
    private void InitHealthBar()
    {
        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * 30f);
        float parrent_scale_offset = 1f / transform.localScale.x;

        SpriteRenderer health_sprite_renderer = health_bar_transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer removed_sprite_renderer = health_bar_transform.GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer no_health_sprite_renderer = health_bar_transform.GetChild(2).GetComponent<SpriteRenderer>();

        health_sprite_renderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));
        removed_sprite_renderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));
        no_health_sprite_renderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));

        health_sprite_renderer.material.SetColor("_Color", health_sprite_renderer.color);
        removed_sprite_renderer.material.SetColor("_Color", removed_sprite_renderer.color);
        no_health_sprite_renderer.material.SetColor("_Color", no_health_sprite_renderer.color);

        health_sprite_renderer.material.renderQueue = 4003;
        removed_sprite_renderer.material.renderQueue = 4002;
        no_health_sprite_renderer.material.renderQueue = 4001;

        health_material = health_sprite_renderer.material;
        removed_health_material = removed_sprite_renderer.material;
        no_health_material = no_health_sprite_renderer.material;

        health_bar_transform.localScale = new Vector3(health_bar_pixel_size.x, y_scale * health_bar_pixel_size.y, health_bar_pixel_size.x) * parrent_scale_offset;
    }

    /// <summary>
    /// Uppdates the visuals of our health bar
    /// </summary>
    private void UpdateHealthBar()
    {
        float bar_value = current_health / starting_health;
        health_bar_slider_value = Mathf.CeilToInt(bar_value * health_bar_pixel_size.x) / health_bar_pixel_size.x;
        health_material.SetFloat("_CutoffX", health_bar_slider_value);
    }

    private float _current_animated_float_value;
    private float current_animated_float_value
    {
        get { return _current_animated_float_value; }
        set { _current_animated_float_value = Mathf.Clamp(value, health_bar_slider_value, 1f); AnimatedHealthBar(); }
    }

    /// <summary>
    /// Lerps removed/red health to black
    /// </summary>
    private void AnimatedHealthBar()
    {
        removed_health_material.SetFloat("_CutoffX" , Mathf.CeilToInt(current_animated_float_value * health_bar_pixel_size.x) / health_bar_pixel_size.x);
    }

    private List<string> damage_id_list = new List<string>();
    /// <summary>
    /// Remembers passed in damage id and discards further attempts of damaging ai with the same damage id. Damage id gets removed after given time.
    /// </summary>
    public bool Damage(float damage, string damage_id, float invulnerability_time = 0.25f)
    {
        if (!damage_id_list.Contains(damage_id))
        {
            current_health -= damage;
            damage_id_list.Add(damage_id);
            StartCoroutine(RemoveIdFromList(invulnerability_time, damage_id));
            return true;
        }
        return false;
    }
    /// <summary>
    /// Damages enemy by damage
    /// </summary>
    public bool Damage(float damage)
    {
        current_health -= damage;
        return true;
    }

    /// <summary>
    /// Removes damage id from list
    /// </summary>
    private IEnumerator RemoveIdFromList(float time, string item)
    {
        yield return new WaitForSeconds(time);
        damage_id_list.Remove(item);
    }

    /// <summary>
    /// For debug purposes
    /// </summary>
    public void SetColor(Color color)
    {
        if (transform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh_renderer_parrent))
        {
            mesh_renderer_parrent.material.SetColor("_Color", color);
        }
        foreach (Transform child in transform)
        {
            if(child.TryGetComponent<MeshRenderer>(out MeshRenderer mesh_renderer))
            {
                mesh_renderer.material.SetColor("_Color", color);
            }
        }
    }

    /// <summary>
    /// Kills this unit by disabeling all scripts on it
    /// </summary>
    public void Die()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
    }
}
