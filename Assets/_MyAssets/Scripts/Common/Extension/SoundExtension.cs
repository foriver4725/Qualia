namespace MyScripts.Common.Extension;

internal static class SoundExtension
{
    internal static void LetInit
    (
        this AudioSource source,
        AudioMixerGroup group,
        bool doPlayOnAwake = false,
        bool doLoop = false,
        float volume = 1.0f,
        float pitch = 1.0f
    )
    {
        source.outputAudioMixerGroup = group;
        source.playOnAwake = doPlayOnAwake;
        source.loop = doLoop;
        source.volume = volume;
        source.pitch = pitch;
        source.clip = null;
    }

    internal static void LetPlay
    (
        this AudioSource source,
        AudioClip clip,
        float volume = 1.0f,
        float pitch = 1.0f
    )
    {
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }

    internal static void LetStop
    (
        this AudioSource source,
        bool doClearClip = true,
        bool doResetVolume = true,
        bool doResetPitch = true
    )
    {
        source.Stop();
        if (doClearClip) source.clip = null;
        if (doResetVolume) source.volume = 1.0f;
        if (doResetPitch) source.pitch = 1.0f;
    }
}
