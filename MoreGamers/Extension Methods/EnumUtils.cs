using System;
using System.Reflection;
using System.ComponentModel;

using UnityEngine;

namespace MG.EnumsExtensions
{
    internal static class EnumUtils
    {
        #region Public static methods

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <returns>The description.</returns>
        /// <param name="en">enumValue.</param>
        public static string GetDescription(this Enum enumValue)
        {
            try
            {
                MemberInfo[] info = enumValue.GetType().GetMember(enumValue.ToString());

                if (info != null && info.Length > 0)
                {
                    object[] attributes = info[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                    if (attributes != null && attributes.Length > 0)
                    {
                        return ((DescriptionAttribute)attributes[0]).Description;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            return enumValue.ToString();
        }

        #endregion
    }
}

