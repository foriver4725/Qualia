namespace MyScripts.Runtime
{
    internal sealed class DisasterSoundPlayer : ASoundPlayerWithType<SDisasterSound, SDisasterSound.Disaster>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SDisasterSound.Disaster.Count;

        // フェードアウトのため、2つを交互に使う
        private bool[] areUsingFirst = null;

        internal sealed override void LetPlay(SDisasterSound.Disaster type)
        {
            AudioClip clip = Param.GetClip(type);
            if (clip == null)
            {
                "No valid clip exists to play.".Print(LogSettings.Warning);
                return;
            }

            // 再生とフェードアウトが重複して実行されることはない想定

            (byte usingIndex, byte notUsingIndex, byte smallerIndex, _) = GetIndices(type);
            areUsingFirst[smallerIndex] = !areUsingFirst[smallerIndex];

            AudioSources[notUsingIndex].LetPlay
            (
                clip,
                volume: Param.Volume
            );

            LetStopByIndex(usingIndex);
        }

        internal void LetStop(SDisasterSound.Disaster type)
        {
            (byte usingIndex, _, _, _) = GetIndices(type);
            // 新しく再生するわけではないので、areUsingFirst を変更する必要はない
            LetStopByIndex(usingIndex);
        }

        private void LetStopByIndex(byte index)
        {
            AudioSource audioSource = AudioSources[index];
            if (!audioSource.isPlaying) return;

            audioSource
                .DOFade(0.0f, Param.FadeOutDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    audioSource.LetStop();
                });
        }

        private (byte Using, byte NotUsing, byte Smaller, byte Larger) GetIndices(SDisasterSound.Disaster type)
        {
            byte smallIndex = type.ToInteger<SDisasterSound.Disaster, byte>();
            byte largeIndex = (byte)(smallIndex + GetTypeAmount());

            if (areUsingFirst[smallIndex]) return (smallIndex, largeIndex, smallIndex, largeIndex);
            else return (largeIndex, smallIndex, smallIndex, largeIndex);
        }

        private protected sealed override void Init()
        {
            // フェードアウトさせるため、i,i+soundAmount の2つのAudioSourceを用意する
            byte soundAmount = (byte)(GetTypeAmount() << 1);

            AudioSources = new AudioSource[soundAmount];

            for (int i = 0; i < soundAmount; i++)
            {
                AudioSource source = Root.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    Param.Group,
                    doLoop: true
                );

                AudioSources[i] = source;
            }

            areUsingFirst = new bool[soundAmount];
            Array.Fill(areUsingFirst, false);
        }
    }
}
