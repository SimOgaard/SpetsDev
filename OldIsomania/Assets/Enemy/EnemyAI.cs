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
    public Transform chaseTransform;
    public Vector3 oldChasePosition = Vector3.zero;

    [SerializeField] private float healthRemoveSpeed = 0.5f;
    [SerializeField] private float startingHealth;
    private float _currentHealth;
    public float currentHealth
    {
        get { return _currentHealth; }
        private set
        {

            if (!healthBarGameObject.activeSelf)
            {
                healthBarGameObject.SetActive(true);
            }

            if (value <= 0f)
            {
                _currentHealth = 0f;
                UpdateHealthBar();
                //Die();
                return;
            }
            else if (value > startingHealth)
            {
                _currentHealth = startingHealth;
            }
            else
            {
                _currentHealth = value;
            }
            UpdateHealthBar();
        }
    }

    public float damage;

    private bool coroutineIsStarted = false;
    private IEnumerator ActivateExclamationMark()
    {
        coroutineIsStarted = true;
        questionMarkMaterial.SetFloat("_Show", 0f);
        exclamationPointMaterial.SetFloat("_Show", 1f);
        yield return new WaitForSeconds(exclamationPointTime);
        exclamationPointMaterial.SetFloat("_Show", 0f);
    }

    public float hearingAmplification = 1f;
    [SerializeField] private float hearingThreshold = 0.001f;
    [SerializeField] private float attentionDecreaseFactor = 0.1f;
    public static float exclamationPointTime = 2f;
    [SerializeField] private float maxAttentiveness;

    public bool eyesOpen = false;
    public float fov = 1f;
    public float visionDistance = 1750f;
    public float visionAmplification = 1f;
    [SerializeField] private float visionThreshold = 0.001f;

    [SerializeField] private float changeChaseTransformThreshold = 0.01f;

    public float currentAttention = 0f;

    private void UpdateAttention(Transform origin, float attentionLevelChange)
    {
        currentAttention += attentionLevelChange;
        if (currentAttention <= 0f)
        {
            coroutineIsStarted = false;
            questionMarkMaterial.SetFloat("_Show", 0f);
            currentAttention = 0f;

            if (chaseTransform != null)
            {
                chaseTransform = null;
            }
        }
        else if (currentAttention < 1f)
        {
            coroutineIsStarted = false;
            questionMarkMaterial.SetFloat("_Show", 1f);
            questionMarkMaterial.SetFloat("_CutoffY", currentAttention);

            if (chaseTransform != null)
            {
                oldChasePosition = chaseTransform.position;
                chaseTransform = null;
            }
        }
        else
        {
            if (!coroutineIsStarted && origin == Global.playerTransform)
            {
                chaseTransform = origin;
                StartCoroutine(ActivateExclamationMark());
            }
            else if (chaseTransform != Global.playerTransform && attentionLevelChange >= changeChaseTransformThreshold)
            {
                questionMarkMaterial.SetFloat("_Show", 1f);
                questionMarkMaterial.SetFloat("_CutoffY", 1f);
                chaseTransform = origin;
            }
            if (attentionLevelChange >= 0)
            {
                currentAttention = maxAttentiveness;
            }
        }
    }

    public void AttendToSound(Transform soundOrigin, float soundLevel, float timeSpan = 1f)
    {
        soundLevel *= timeSpan;
        if (Mathf.Abs(soundLevel) >= hearingThreshold)
        {
            UpdateAttention(soundOrigin, soundLevel);
        }
    }

    public void AttendToVision(Transform visionOrigin, float visionLevel, float timeSpan = 1f)
    {
        visionLevel *= timeSpan;
        if (visionLevel >= visionThreshold)
        {
            UpdateAttention(visionOrigin, visionLevel);
        }
    }

    private void Awake()
    {
        agent = GetComponent<Agent>();
        InitHealthBar();
        _currentHealth = startingHealth;
    }

    private void Update()
    {
        currentAnimatedFloatValue -= Time.deltaTime * healthRemoveSpeed;
    }

    private void FixedUpdate()
    {
        UpdateAttention(null, -Time.fixedDeltaTime * attentionDecreaseFactor);
    }

    [SerializeField] private GameObject canvasGameObject;
    private GameObject questionMarkGameObject;
    private GameObject exclamationPointGameObject;
    private Material questionMarkMaterial;
    private Material exclamationPointMaterial;
    [SerializeField] private GameObject healthBarGameObject;
    private Material healthMaterial;
    private Material removedHealthMaterial;
    private Material noHealthMaterial;

    private float healthBarSliderValue = 1;

    [SerializeField] private Vector2 healthBarPixelSize = Vector2.one;

    /// <summary>
    /// Inits health bar to correct size.
    /// </summary>
    private void InitHealthBar()
    {
        float parrentScaleOffset = 1f / transform.localScale.x;

        SpriteRenderer healthSpriteRenderer = healthBarGameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer removedSpriteRenderer = healthBarGameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer noHealthSpriteRenderer = healthBarGameObject.transform.GetChild(2).GetComponent<SpriteRenderer>();

        healthSpriteRenderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));
        removedSpriteRenderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));
        noHealthSpriteRenderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));

        healthSpriteRenderer.material.SetColor("_Color", healthSpriteRenderer.color);
        removedSpriteRenderer.material.SetColor("_Color", removedSpriteRenderer.color);
        noHealthSpriteRenderer.material.SetColor("_Color", noHealthSpriteRenderer.color);

        healthSpriteRenderer.material.renderQueue = 4003;
        removedSpriteRenderer.material.renderQueue = 4002;
        noHealthSpriteRenderer.material.renderQueue = 4001;

        healthMaterial = healthSpriteRenderer.material;
        removedHealthMaterial = removedSpriteRenderer.material;
        noHealthMaterial = noHealthSpriteRenderer.material;

        healthBarGameObject.transform.localScale = new Vector3(healthBarPixelSize.x, SpriteInitializer.yScale * healthBarPixelSize.y, healthBarPixelSize.x) * parrentScaleOffset;

        healthBarGameObject.SetActive(false);

        questionMarkGameObject = canvasGameObject.transform.GetChild(0).gameObject;
        exclamationPointGameObject = canvasGameObject.transform.GetChild(1).gameObject;

        TextMeshProUGUI questionMarkText = questionMarkGameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI exclamationPointText = exclamationPointGameObject.GetComponent<TextMeshProUGUI>();

        var font = questionMarkText.fontMaterial.GetTexture("_MainTex");

        questionMarkText.fontMaterial = new Material(Shader.Find("Custom/Font Color Shader"));
        exclamationPointText.fontMaterial = new Material(Shader.Find("Custom/Font Color Shader"));

        questionMarkText.fontMaterial.SetColor("_Color1", healthSpriteRenderer.color);
        exclamationPointText.fontMaterial.SetColor("_Color2", removedSpriteRenderer.color);

        questionMarkText.fontMaterial.renderQueue = 4500;
        exclamationPointText.fontMaterial.renderQueue = 4500;

        questionMarkText.fontMaterial.SetTexture("_MainTex", font);
        exclamationPointText.fontMaterial.SetTexture("_MainTex", font);

        questionMarkMaterial = questionMarkText.fontMaterial;
        exclamationPointMaterial = exclamationPointText.fontMaterial;

        //canvasGameObject.SetActive(false);
    }

    /// <summary>
    /// Uppdates the visuals of our health bar
    /// </summary>
    private void UpdateHealthBar()
    {
        float barValue = currentHealth / startingHealth;
        healthBarSliderValue = Mathf.CeilToInt(barValue * healthBarPixelSize.x) / healthBarPixelSize.x;
        healthMaterial.SetFloat("_CutoffX", healthBarSliderValue);
    }

    private float _currentAnimatedFloatValue;
    private float currentAnimatedFloatValue
    {
        get { return _currentAnimatedFloatValue; }
        set { _currentAnimatedFloatValue = Mathf.Clamp(value, healthBarSliderValue, 1f); AnimatedHealthBar(); }
    }

    /// <summary>
    /// Lerps removed/red health to black
    /// </summary>
    private void AnimatedHealthBar()
    {
        removedHealthMaterial.SetFloat("_CutoffX" , Mathf.CeilToInt(currentAnimatedFloatValue * healthBarPixelSize.x) / healthBarPixelSize.x);
    }

    private List<string> damageIdList = new List<string>();
    /// <summary>
    /// Remembers passed in damage id and discards further attempts of damaging ai with the same damage id. Damage id gets removed after given time.
    /// </summary>
    public bool Damage(float damage, string damageId, float invulnerabilityTime = 0.25f)
    {
        if (!damageIdList.Contains(damageId) && damage != 0f)
        {
            currentHealth -= damage;
            damageIdList.Add(damageId);
            StartCoroutine(RemoveIdFromList(invulnerabilityTime, damageId));
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
            currentHealth -= damage;
        }
        return true;
    }

    /// <summary>
    /// Removes damage id from list
    /// </summary>
    private IEnumerator RemoveIdFromList(float time, string item)
    {
        yield return new WaitForSeconds(time);
        damageIdList.Remove(item);
    }

    /// <summary>
    /// For debug purposes
    /// </summary>
    public void SetColor(Color color)
    {
        return;

        if (transform.TryGetComponent<MeshRenderer>(out MeshRenderer meshRendererParrent))
        {
            meshRendererParrent.material.SetColor("_Color", color);
        }
        foreach (Transform child in transform)
        {
            if(child.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
            {
                meshRenderer.material.SetColor("_Color", color);
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
