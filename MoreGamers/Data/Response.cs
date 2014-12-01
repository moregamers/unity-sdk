using System.Collections.Generic;

namespace MG.Data
{
    public class Response
    {
        #region Private fields

        private readonly string m_imageURL;
        private readonly string m_landscapeImageURL;
        private readonly string m_portraitImageURL;
        private readonly string m_clickURL;
        private readonly string m_trackingURL;

        #endregion

        #region Public properties

        public string ImageURL
        {
            get
            {
                return this.m_imageURL;
            }
        }

        public string LandscapeImageURL
        {
            get
            {
                return this.m_landscapeImageURL;
            }
        }

        public string PortraitImageURL
        {
            get
            {
                return this.m_portraitImageURL;
            }
        }

        public string ClickURL
        {
            get
            {
                return this.m_clickURL;
            }
        }

        public string TrackingURL
        {
            get
            {
                return this.m_trackingURL;
            }
        }

        #endregion

        #region Constructors

        public Response(Dictionary<string, object> json)
        {
            this.m_imageURL          = (string)json["image"];
            this.m_landscapeImageURL = (string)json["landscape_image"];
            this.m_portraitImageURL  = (string)json["portrait_image"];
            this.m_clickURL          = (string)json["click"];
            this.m_trackingURL       = (string)json["tracking"];
        }

        #endregion
    }
}
