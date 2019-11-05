﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class TimeController : MonoBehaviour
{

    public float slowDownFactor;
    public float slowDownDuration;

    public float vignetteIntensity;
    public float chromaticAberrationIntensity;
    public int motionBlurIntensity;

    private PostProcessVolume volume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private MotionBlur motionBlur;

    private float defaultTimeScale;
    private float defaultFixedDeltaTime;
    private float slowDownTimer;
    private bool slowedDown;

    // Start is called before the first frame update
    void Awake()
    {
        defaultTimeScale = Time.timeScale;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        slowDownTimer = slowDownDuration;
    }

    private void Start()
    {
        vignette = ScriptableObject.CreateInstance<Vignette>();
        vignette.enabled.Override(true);
        vignette.intensity.Override(vignetteIntensity);

        chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        chromaticAberration.enabled.Override(true);
        chromaticAberration.intensity.Override(chromaticAberrationIntensity);
        chromaticAberration.fastMode.Override(true);

        motionBlur = ScriptableObject.CreateInstance<MotionBlur>();
        motionBlur.enabled.Override(true);
        motionBlur.sampleCount.Override(motionBlurIntensity);

        volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, new PostProcessEffectSettings[] { vignette, chromaticAberration, motionBlur });
        volume.weight = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            StartSlowMotion();
        }

        if (slowedDown && slowDownTimer > 0)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, slowDownFactor, 0.1f);
            slowDownTimer -= Time.deltaTime;
        }
        else if (defaultTimeScale - Time.timeScale < 0.05f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, defaultTimeScale, 0.1f);
        }
        else
        {
            Time.timeScale = defaultTimeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }

    public void StartSlowMotion()
    {
        slowedDown = true;
        slowDownTimer = slowDownDuration;
        Time.fixedDeltaTime = slowDownFactor * .02f;
        TweenEffects();
    }

    public void TweenEffects()
    {
        DOTween.Sequence()
            .Append(DOTween.To(() => volume.weight, x => volume.weight = x, 1, 1f))
            .AppendInterval(slowDownDuration)
            .Append(DOTween.To(() => volume.weight, x => volume.weight = x, 0f, 1f))
            .OnComplete(() =>
            {
                    //RuntimeUtilities.DestroyVolume(volume, true, true);
                });
    }
}