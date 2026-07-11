using NAudio.Wave;
using System;

namespace WinFormsApp1
{
    public class BasicAudioPlayer
    {
        private AudioFileReader _audioFile;
        private WaveOutEvent _outputDevice;

        public void PlayAudioFile(string filePath)
        {
            try
            {
                // 1. 创建音频文件读取器
                _audioFile = new AudioFileReader(filePath);

                // 2. 创建输出设备（这里用最常见的WaveOutEvent）
                _outputDevice = new WaveOutEvent();

                // 3. 将读取器与输出设备连接
                _outputDevice.Init(_audioFile);

                // 4. 开始播放
                _outputDevice.Play();

                Console.WriteLine($"正在播放: {filePath}");

                // 5. （可选）播放完成后自动释放资源
                _outputDevice.PlaybackStopped += (sender, args) => DisposeResources();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放出错: {ex.Message}");
                DisposeResources();
            }
        }

        public void StopPlayback()
        {
            _outputDevice?.Stop();
        }

        private void DisposeResources()
        {
            _outputDevice?.Dispose();
            _audioFile?.Dispose();
        }
    }
}
