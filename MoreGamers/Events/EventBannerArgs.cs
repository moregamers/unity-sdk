using System;

using UnityEngine;

namespace MG.Events
{
    public class EventBannerArgs : EventArgs
    {
        #region Private fields

        private readonly string m_clickURL;

        private readonly Texture2D m_image;

        #endregion

        #region Public properties

        public string ClickURL
        {
            get
            {
                return this.m_clickURL;
            }
        }

        public Texture2D Image
        {
            get
            {
                return this.m_image;
            }
        }

        #endregion

        #region Constructors

        public EventBannerArgs(string clickURL, Texture2D image)
        {
            this.m_clickURL = clickURL;

            this.m_image = image;
        }

        #endregion
    }
}
