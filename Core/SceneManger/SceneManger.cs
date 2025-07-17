using System.Collections;
using UnityEngine.SceneManagement;
using Xuf.Common;
using System;

namespace Xuf.Core
{
    public class CSceneManager : CMonoSingleton<CSceneManager>
    {
        public void LoadSecen(string sceneName, IEnumerator beforeLoad = null,
            IEnumerator afterLoad = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            StartCoroutine(LoadSecenInternal(sceneName, mode, beforeLoad, afterLoad));
        }

        private IEnumerator LoadSecenInternal(string sceneName, LoadSceneMode mode,
            IEnumerator beforeLoad, IEnumerator afterLoad)
        {
            yield return beforeLoad;
            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            while (!op.isDone)
            {
                yield return null;
            }
            // Important: the scene is running now
            yield return afterLoad;
        }
    }
}
