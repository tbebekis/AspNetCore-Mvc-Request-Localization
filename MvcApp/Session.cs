using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

 
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MvcApp
{
    /// <summary>
    /// Provides access to session variables (entries)
    /// </summary>
    static public class Session
    {
        /* private */
        /// <summary>
        /// Returns a value stored in session, found under a specified key or a default value if not found.
        /// </summary>
        static T Get<T>(this ISession Instance, string Key)
        {
            Key = Key.ToLowerInvariant();
            string JsonText = Instance.GetString(Key);
            if (JsonText == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(JsonText);
        }
        /// <summary>
        /// Returns a value stored in session, found under a specified key or a default value if not found.
        /// </summary>
        static T Get<T>(this ISession Instance, string Key, T Default)
        {
            Key = Key.ToLowerInvariant();
            string JsonText = Instance.GetString(Key);
            if (JsonText == null)
                return Default;

            return JsonConvert.DeserializeObject<T>(JsonText);
        }
        /// <summary>
        /// Stores a value in session under a specified key.
        /// </summary>
        static void Set<T>(this ISession Instance, string Key, T Value)
        {
            Key = Key.ToLowerInvariant();
            string JsonText = JsonConvert.SerializeObject(Value);
            Instance.SetString(Key, JsonText);
        }

        /* public */
        /// <summary>
        /// Returns a value stored in session, found under a specified key or a default value if not found.
        /// </summary>
        static public T Get<T>(string Key)
        {
            return HttpContext.Session.Get<T>(Key);
        }
        /// <summary>
        /// Returns a value stored in session, found under a specified key or a default value if not found.
        /// </summary>
        static public T Get<T>(string Key, T Default)
        {
            return HttpContext.Session.Get(Key, Default);
        }
        /// <summary>
        /// Stores a value in session under a specified key.
        /// </summary>
        static public void Set<T>(string Key, T Value)
        {
            HttpContext.Session.Set(Key, Value);
        }

        /// <summary>
        /// Returns a string stored in session, found under a specified key or null if not found.
        /// </summary>
        static public string GetString(string Key)
        {
            return Get<string>(Key, null);
        }
        /// <summary>
        /// Stores a string value in session under a specified key.
        /// </summary>
        static public void SetString(string Key, string Value)
        {
            Set(Key, Value);
        }

        /// <summary>
        /// Clears all session variables
        /// </summary>
        static public void Clear()
        {
            HttpContext.Session.Clear();
        }
        /// <summary>
        /// Removes a session variable under a specified key.
        /// </summary>
        static public void Remove(string Key)
        {
            Key = Key.ToLowerInvariant();
            HttpContext.Session.Remove(Key);
        }
        /// <summary>
        /// Returns true if a variable exists in session under a specified key.
        /// </summary>
        static public bool ContainsKey(string Key)
        {
            Key = Key.ToLowerInvariant();
            byte[] Buffer = null;
            return HttpContext.Session.TryGetValue(Key, out Buffer);
        }

        /* properties */
        /// <summary>
        /// The context accessor
        /// </summary>
        static public IHttpContextAccessor HttpContextAccessor { get; set; }
        /// <summary>
        /// Returns the HttpContext
        /// </summary>
        static public HttpContext HttpContext { get { return HttpContextAccessor.HttpContext; } }
        /// <summary>
        /// Provides acces to request variables.
        /// <para>This dictionary is used to store data while processing a single request. The dictionary's contents are discarded after a request is processed.</para>
        /// </summary>
        static public IDictionary<object, object> Request { get { return HttpContext.Items; } }


        /// <summary>
        /// Gets or sets the current language of the session.
        /// <para>Represents a language this application supports, i.e. provides localized resources for.</para>
        /// </summary>
        static public LanguageItem Language
        {
            get
            {
                LanguageItem Result = Get<LanguageItem>("Language", null);
                return Result != null ? Result : Languages.DefaultLanguage;
            }
            set
            {
                //LanguageItem Lang = Languages.DefaultLanguage;

                if (value != null)
                {
                    Set("Language", value);
                }
            }
        }

    }
}
