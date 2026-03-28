using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        Instance = this;

        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.3f;

        // SFX source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        GenerateSounds();
    }

    void GenerateSounds()
    {
        // Generate all sounds procedurally (no external files needed)
        clips["pistol_shoot"] = GenerateShot(0.03f, 900, 500, 0.4f);
        clips["rifle_shoot"] = GenerateShot(0.04f, 600, 250, 0.5f);
        clips["sniper_shoot"] = GenerateShot(0.06f, 350, 120, 0.6f);
        clips["smg_shoot"] = GenerateShot(0.025f, 1000, 600, 0.35f);
        clips["shotgun_shoot"] = GenerateShot(0.05f, 250, 100, 0.55f);
        clips["heavy_shoot"] = GenerateShot(0.04f, 400, 180, 0.5f);

        clips["reload"] = GenerateReload();
        clips["empty_click"] = GenerateClick(0.02f, 1500);

        clips["footstep1"] = GenerateFootstep(0.05f, 200);
        clips["footstep2"] = GenerateFootstep(0.05f, 250);
        clips["footstep3"] = GenerateFootstep(0.05f, 180);

        clips["hit_body"] = GenerateHit(0.06f, 400, 0.7f);
        clips["hit_headshot"] = GenerateHit(0.08f, 600, 1.0f);
        clips["hit_wall"] = GenerateHit(0.03f, 1200, 0.4f);

        clips["death"] = GenerateDeath();
        clips["kill_confirm"] = GenerateKillConfirm();

        clips["round_start"] = GenerateBeep(0.3f, 880, 0.5f);
        clips["round_win"] = GenerateWinSound();
        clips["round_lose"] = GenerateLoseSound();

        clips["buy_weapon"] = GenerateBeep(0.1f, 1200, 0.3f);
        clips["no_money"] = GenerateBeep(0.15f, 300, 0.4f);

        clips["bomb_plant"] = GenerateBombPlant();
        clips["bomb_tick"] = GenerateBeep(0.05f, 1000, 0.6f);
        clips["bomb_explode"] = GenerateExplosion();

        clips["jump"] = GenerateJump();
        clips["land"] = GenerateFootstep(0.08f, 150);
    }

    // === Sound Generators ===

    AudioClip GenerateShot(float duration, float freqStart, float freqEnd, float noiseAmount)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(freqStart, freqEnd, t);
            float wave = Mathf.Sin(2 * Mathf.PI * freq * t * duration) * (1 - t);
            float noise = (Random.value * 2 - 1) * noiseAmount * (1 - t);
            data[i] = Mathf.Clamp((wave + noise) * (1 - t * 0.8f), -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("shot", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateReload()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            // Click at start
            if (t < 0.05f) data[i] = Mathf.Sin(2 * Mathf.PI * 2000 * t) * (1 - t / 0.05f) * 0.5f;
            // Slide sound in middle
            else if (t > 0.3f && t < 0.5f)
            {
                float lt = (t - 0.3f) / 0.2f;
                data[i] = (Random.value * 2 - 1) * 0.3f * (1 - Mathf.Abs(lt - 0.5f) * 2);
            }
            // Click at end
            else if (t > 0.7f && t < 0.75f)
            {
                float lt = (t - 0.7f) / 0.05f;
                data[i] = Mathf.Sin(2 * Mathf.PI * 1800 * t) * (1 - lt) * 0.5f;
            }
        }

        AudioClip clip = AudioClip.Create("reload", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateClick(float duration, float freq)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t * duration) * (1 - t) * 0.3f;
        }
        AudioClip clip = AudioClip.Create("click", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateFootstep(float duration, float freq)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = (Random.value * 2 - 1) * 0.4f;
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t * duration) * 0.3f;
            data[i] = (noise + tone) * (1 - t) * 0.5f;
        }
        AudioClip clip = AudioClip.Create("step", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateHit(float duration, float freq, float vol)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t * duration) * (1 - t) * vol;
            data[i] += (Random.value * 2 - 1) * 0.2f * (1 - t);
        }
        AudioClip clip = AudioClip.Create("hit", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateDeath()
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.3f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(500, 150, t) * t * 0.3f) * (1 - t) * 0.6f;
        }
        AudioClip clip = AudioClip.Create("death", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateKillConfirm()
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.15f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * 1200 * t * 0.15f) * (1 - t) * 0.4f;
            if (t > 0.5f) data[i] += Mathf.Sin(2 * Mathf.PI * 1600 * t * 0.15f) * (1 - t) * 0.3f;
        }
        AudioClip clip = AudioClip.Create("killconfirm", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateBeep(float duration, float freq, float vol)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t * duration) * vol * (1 - t * 0.5f);
        }
        AudioClip clip = AudioClip.Create("beep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateWinSound()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { 523, 659, 784 };
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            int noteIdx = Mathf.Min((int)(t * 3), 2);
            data[i] = Mathf.Sin(2 * Mathf.PI * notes[noteIdx] * t * duration) * 0.4f * (1 - t * 0.3f);
        }
        AudioClip clip = AudioClip.Create("win", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateLoseSound()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { 400, 300, 200 };
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            int noteIdx = Mathf.Min((int)(t * 3), 2);
            data[i] = Mathf.Sin(2 * Mathf.PI * notes[noteIdx] * t * duration) * 0.4f * (1 - t * 0.3f);
        }
        AudioClip clip = AudioClip.Create("lose", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateBombPlant()
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(440, 880, t) * t * duration) * 0.5f * (1 - t * 0.5f);
        }
        AudioClip clip = AudioClip.Create("bombplant", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateExplosion()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = (Random.value * 2 - 1) * (1 - t);
            float bass = Mathf.Sin(2 * Mathf.PI * 60 * t * duration) * (1 - t) * 0.8f;
            data[i] = Mathf.Clamp((noise * 0.6f + bass) * (1 - t * 0.7f), -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("explosion", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateJump()
    {
        int sampleRate = 44100;
        float duration = 0.1f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            data[i] = Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(300, 500, t) * t * duration) * (1 - t) * 0.3f;
        }
        AudioClip clip = AudioClip.Create("jump", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // === Public Play Methods ===

    public void PlayShoot(string weaponType)
    {
        string key = weaponType + "_shoot";
        AudioClip clip = null;
        if (clips.ContainsKey(key)) clip = clips[key];
        else if (clips.ContainsKey("rifle_shoot")) clip = clips["rifle_shoot"];
        if (clip != null)
        {
            sfxSource.Stop();
            sfxSource.clip = clip;
            sfxSource.Play();
        }
    }

    public void PlayReload() { if (clips.ContainsKey("reload")) sfxSource.PlayOneShot(clips["reload"]); }
    public void PlayEmptyClick() { if (clips.ContainsKey("empty_click")) sfxSource.PlayOneShot(clips["empty_click"]); }

    public void PlayFootstep()
    {
        string key = "footstep" + Random.Range(1, 4);
        if (clips.ContainsKey(key)) sfxSource.PlayOneShot(clips[key], 0.3f);
    }

    public void PlayHitBody() { if (clips.ContainsKey("hit_body")) sfxSource.PlayOneShot(clips["hit_body"]); }
    public void PlayHeadshot() { if (clips.ContainsKey("hit_headshot")) sfxSource.PlayOneShot(clips["hit_headshot"]); }
    public void PlayHitWall() { if (clips.ContainsKey("hit_wall")) sfxSource.PlayOneShot(clips["hit_wall"]); }
    public void PlayDeath() { if (clips.ContainsKey("death")) sfxSource.PlayOneShot(clips["death"]); }
    public void PlayKillConfirm() { if (clips.ContainsKey("kill_confirm")) sfxSource.PlayOneShot(clips["kill_confirm"]); }
    public void PlayRoundStart() { if (clips.ContainsKey("round_start")) sfxSource.PlayOneShot(clips["round_start"]); }
    public void PlayRoundWin() { if (clips.ContainsKey("round_win")) sfxSource.PlayOneShot(clips["round_win"]); }
    public void PlayRoundLose() { if (clips.ContainsKey("round_lose")) sfxSource.PlayOneShot(clips["round_lose"]); }
    public void PlayBuyWeapon() { if (clips.ContainsKey("buy_weapon")) sfxSource.PlayOneShot(clips["buy_weapon"]); }
    public void PlayNoMoney() { if (clips.ContainsKey("no_money")) sfxSource.PlayOneShot(clips["no_money"]); }
    public void PlayBombPlant() { if (clips.ContainsKey("bomb_plant")) sfxSource.PlayOneShot(clips["bomb_plant"]); }
    public void PlayBombTick() { if (clips.ContainsKey("bomb_tick")) sfxSource.PlayOneShot(clips["bomb_tick"]); }
    public void PlayBombExplode() { if (clips.ContainsKey("bomb_explode")) sfxSource.PlayOneShot(clips["bomb_explode"]); }
    public void PlayJump() { if (clips.ContainsKey("jump")) sfxSource.PlayOneShot(clips["jump"]); }
    public void PlayLand() { if (clips.ContainsKey("land")) sfxSource.PlayOneShot(clips["land"]); }
}
