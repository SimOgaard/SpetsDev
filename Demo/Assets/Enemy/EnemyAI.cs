using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Holds general functionality that all enemies should inherrent from.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    public interface IAIBehaviour
    {
        void Die();
    }

    [HideInInspector] public Agent agent;
    public Transform chase_transform;
    public Vector3 old_chase_position = Vector3.zero;

    [SerializeField] private float health_remove_speed = 0.5f;
    [SerializeField] private float starting_health;
    private float _current_health;
    public float current_health
    {
        get { return _current_health; }
        private set
        {

            if (!health_bar_game_object.activeSelf)
            {
                health_bar_game_object.SetActive(true);
            }

            if (value <= 0f)
            {
                _current_health = 0f;
                UpdateHealthBar();
                //Die();
                return;
            }
            else if (value > starting_health)
            {
                _current_health = starting_health;
            }
            else
            {
                _current_health = value;
            }
            UpdateHealthBar();
        }
    }

    public float damage;

    private bool coroutine_is_started = false;
    private IEnumerator ActivateExclamationMark()
    {
        coroutine_is_started = true;
        question_mark_material.SetFloat("_Show", 0f);
        exclamation_point_material.SetFloat("_Show", 1f);
        yield return new WaitForSeconds(exclamation_point_time);
        exclamation_point_material.SetFloat("_Show", 0f);
    }

    public float hearing_amplification = 1f;
    [SerializeField] private float hearing_threshold = 0.001f;
    [SerializeField] private float attention_decrease_factor = 0.1f;
    public static float exclamation_point_time = 2f;
    [SerializeField] private float max_attentiveness;

    public bool eyes_open = false;
    public float fov = 1f;
    public float vision_distance = 1750f;
    public float vision_amplification = 1f;
    [SerializeField] private float vision_threshold = 0.001f;

    [SerializeField] private float change_chase_transform_threshold = 0.01f;

    public float current_attention = 0f;

    private void UpdateAttention(Transform origin, float attention_level_change)
    {
        current_attention += attention_level_change;
        if (current_attention <= 0f)
        {
            coroutine_is_started = false;
            question_mark_material.SetFloat("_Show", 0f);
            current_attention = 0f;

            if (chase_transform != null)
            {
                chase_transform = null;
            }
        }
        else if (current_attention < 1f)
        {
            coroutine_is_started = false;
            question_mark_material.SetFloat("_Show", 1f);
            question_mark_material.SetFloat("_CutoffY", current_attention);

            if (chase_transform != null)
            {
                old_chase_position = chase_transform.position;
                chase_transform = null;
            }
        }
        else
        {
            if (!coroutine_is_started && origin == Global.player_transform)
            {
                chase_transform = origin;
                StartCoroutine(ActivateExclamationMark());
            }
            else if (chase_transform != Global.player_transform && attention_level_change >= change_chase_transform_threshold)
            {
                question_mark_material.SetFloat("_Show", 1f);
                question_mark_material.SetFloat("_CutoffY", 1f);
                chase_transform = origin;
            }
            if (attention_level_change >= 0)
            {
                current_attention = max_attentiveness;
            }
        }
    }

    public void AttendToSound(Transform sound_origin, float sound_level, float time_span = 1f)
    {
        sound_level *= time_span;
        if (Mathf.Abs(sound_level) >= hearing_threshold)
        {
            UpdateAttention(sound_origin, sound_level);
        }
    }

    public void AttendToVision(Transform vision_origin, float vision_level, float time_span = 1f)
    {
        vision_level *= time_span;
        if (vision_level >= vision_threshold)
        {
            UpdateAttention(vision_origin, vision_level);
        }
    }

    private void Awake()
    {
        agent = GetComponent<Agent>();
        InitHealthBar();
        _current_health = starting_health;
    }

    private void Update()
    {
        current_animated_float_value -= Time.deltaTime * health_remove_speed;
    }

    private void FixedUpdate()
    {
        UpdateAttention(null, -Time.fixedDeltaTime * attention_decrease_factor);
    }

    [SerializeField] private GameObject canvas_game_object;
    private GameObject question_mark_game_object;
    private GameObject exclamation_point_game_object;
    private Material question_mark_material;
    private Material exclamation_point_material;
    [SerializeField] private GameObject health_bar_game_object;
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
        float parrent_scale_offset = 1f / transform.localScale.x;

        SpriteRenderer health_sprite_renderer = health_bar_game_object.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer removed_sprite_renderer = health_bar_game_object.transform.GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer no_health_sprite_renderer = health_bar_game_object.transform.GetChild(2).GetComponent<SpriteRenderer>();

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

        health_bar_game_object.transform.localScale = new Vector3(health_bar_pixel_size.x, SpriteInitializer.y_scale * health_bar_pixel_size.y, health_bar_pixel_size.x) * parrent_scale_offset;

        health_bar_game_object.SetActive(false);

        question_mark_game_object = canvas_game_object.transform.GetChild(0).gameObject;
        exclamation_point_game_object = canvas_game_object.transform.GetChild(1).gameObject;

        TextMeshProUGUI question_mark_text = question_mark_game_object.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI exclamation_point_text = exclamation_point_game_object.GetComponent<TextMeshProUGUI>();

        var font = question_mark_text.fontMaterial.GetTexture("_MainTex");

        question_mark_text.fontMaterial = new Material(Shader.Find("Custom/Font Color Shader"));
        exclamation_point_text.fontMaterial = new Material(Shader.Find("Custom/Font Color Shader"));

        question_mark_text.fontMaterial.SetColor("_Color1", health_sprite_renderer.color);
        exclamation_point_text.fontMaterial.SetColor("_Color2", removed_sprite_renderer.color);

        question_mark_text.fontMaterial.renderQueue = 4500;
        exclamation_point_text.fontMaterial.renderQueue = 4500;

        question_mark_text.fontMaterial.SetTexture("_MainTex", font);
        exclamation_point_text.fontMaterial.SetTexture("_MainTex", font);

        question_mark_material = question_mark_text.fontMaterial;
        exclamation_point_material = exclamation_point_text.fontMaterial;

        //canvas_game_object.SetActive(false);
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
        if (!damage_id_list.Contains(damage_id) && damage != 0f)
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
        if (damage != 0f)
        {
            current_health -= damage;
        }
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
        return;

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
    /// Kills this unit by disabeling scripts
    /// </summary>
    public void Die()
    {
        Debug.Log("died");
        SetColor(Color.black);

        agent.enabled = false;
        IAIBehaviour script = (IAIBehaviour)GetComponent(typeof(IAIBehaviour));
        script.Die();

        /*
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
        */
    }
}
