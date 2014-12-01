using UnityEngine;

namespace MG.Utils
{
    /// <summary>
    /// Base singleton class.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Protected static field

        protected static T instance;

        #endregion

        #region Public static property

        public static T Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                instance = GameObject.FindObjectOfType<T>();

                if (instance == null)
                    Debug.LogError(string.Format(
                        "An instance of {0} is needed in the scene, but there is none.",
                        typeof(T)));

                return instance;
            }
        }

        #endregion
    }
}
