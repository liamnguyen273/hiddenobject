using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace com.tinycastle.StickerBooker
{
    public class StreamingPlayingStemData : IPlayingStemData
    {
        public int PackNumber { get; }
        public int ClipCount { get; }

        public float DownloadProgress => _progresses.Sum() / _progresses.Length;

        private readonly StemPackCsv _csv;
        private readonly UnityWebRequest[] _handlers;
        private readonly AudioClip[] _clips;
        private readonly float[] _progresses;
        private Task<bool> _downloadTask;

        public bool LoadCompleted => _downloadTask == null;
        public bool Usable => _clips.All(x => x != null);

        public StreamingPlayingStemData(StemPackCsv csv)
        {
            _csv = csv;
            _handlers = new UnityWebRequest[_csv.Items.Count];
            _clips = new AudioClip[_csv.Items.Count];
            _progresses = new float[_csv.Items.Count];
        }
        
        public AudioClip GetClip(int i)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<AudioClip> GetAllClips()
        {
            return _clips;
        }

        public void LaunchDownloadTask(float timeout)
        {
            if (_downloadTask != null) return;
            
            _downloadTask = DownloadFilesAsync(timeout);
            _downloadTask.ContinueWith(x =>
            {
                _downloadTask = null;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task<bool> DownloadFilesAsync(float timeout)
        {
            if (_clips.All(x => x != null))
            {
                return true;
            }
            
            // Launch tasks
            var tasks = new List<Task<AudioClip>>();
            var indexes = new List<int>();
            for (int i = 0; i < _csv.Items.Count; ++i)
            {
                if (_clips[i] != null) continue;   
                
                var handle = _csv.Items[i];
                var filename = handle.FileName;
                var lower = filename.ToLower();
                var type = lower.Contains(".wav") ? AudioType.WAV :
                    lower.Contains(".mp3") ? AudioType.MPEG :
                    lower.Contains(".ogg") ? AudioType.OGGVORBIS : AudioType.UNKNOWN;
                
                if (_handlers[i] == null || 
                    _handlers[i].result is UnityWebRequest.Result.ConnectionError
                                        or UnityWebRequest.Result.ProtocolError 
                                        or UnityWebRequest.Result.DataProcessingError)
                {
                    var oldHandler = _handlers[i];
                    oldHandler?.Dispose();
                    var www = UnityWebRequestMultimedia.GetAudioClip(handle.Url, type);
                    www.timeout = (int)timeout * 2;
                    www.SendWebRequest();
                    LogObj.Default.Info($"Created new download handler for {filename} at {handle.Url}");
                    _handlers[i] = www;
                    _progresses[i] = 0f;
                }
                else if (_handlers[i] != null && _handlers[i].result == UnityWebRequest.Result.InProgress)
                {
                    _progresses[i] = _handlers[i].downloadProgress;
                }

                tasks.Add(WaitDownloadTaskAsync(i, filename, _handlers[i], timeout));
                indexes.Add(i);
            }

            await Task.WhenAll(tasks);
            
            for (int i = 0; i < tasks.Count; ++i)
            {
                var task = tasks[i];
                if (task.IsCompletedSuccessfully && task.Result != null)
                {
                    _clips[i] = task.Result;
                    
                    // Dispose handler?
                    // _handlers.
                }
            }

            return _clips.All(x => x != null);
        }

        private async Task<AudioClip> WaitDownloadTaskAsync(int pos, string name, UnityWebRequest handler, float timeout)
        {
            var checkCount = (int)(timeout * 1000) / 200;
            var i = 0;
            while (!handler.isDone && i < checkCount)
            {
                await Task.Delay(200);
                _progresses[pos] = handler.downloadProgress;
                ++i;
            }

            if (!handler.isDone)
            {
                LogObj.Default.Info(
                    $"Audio clip download handler for {name} failed to complete inside timeout period, will return null clip.");
                return null;
            }
            else
            {
                _progresses[pos] = 1f;

                if (handler.result == UnityWebRequest.Result.Success)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(handler);
                    
                    LogObj.Default.Info($"Download {name} completed.");
                    
                    return clip;
                }
                else
                {
                    LogObj.Default.Warn($"Download {name} failed with status {handler.result}.");
                }
            }

            return null;
        }
    }
}