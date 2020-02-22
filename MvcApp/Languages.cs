using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcApp
{
    /// <summary>
    /// The languages this application supports, i.e. provides localized resources for.
    /// </summary>
    static public class Languages
    {

        /* private */
        static object syncLock = new object();
        static List<LanguageItem> fItems = new List<LanguageItem>();

        /* public */
        /// <summary>
        /// Returns true if a language, specified by a two letter code (en, el, it, fr, etc) is registered.
        /// </summary>
        static public bool Contains(string TwoLetterLanguageCode)
        {
            lock (syncLock)
            {
                return Find(TwoLetterLanguageCode) != null;
            }
        }
        /// <summary>
        /// Returns a language, specified by a two letter code (en, el, it, fr, etc), if registered, else null.
        /// </summary>
        static public LanguageItem Find(string TwoLetterLanguageCode)
        {
            lock (syncLock)
            {
                LanguageItem Item = fItems.FirstOrDefault(item => string.Compare(item.Code, TwoLetterLanguageCode, StringComparison.InvariantCultureIgnoreCase) == 0);
                return Item;
            }
        }
        /// <summary>
        /// Returns a language, specified by a two letter code (en, el, it, fr, etc), if registered, else returns the default language (en).
        /// </summary>
        static public LanguageItem FindOrDefault(string TwoLetterLanguageCode)
        {
            lock (syncLock)
            {
                LanguageItem Item = Find(TwoLetterLanguageCode);
                if (Item == null)
                    Item = DefaultLanguage;
                return Item;
            }
        }
        /// <summary>
        /// Returns a language, specified by a two letter code (en, el, it, fr, etc), if registered, else throws an exception.
        /// </summary>
        static public LanguageItem Get(string TwoLetterLanguageCode)
        {
            lock (syncLock)
            {
                LanguageItem Item = Find(TwoLetterLanguageCode);
                if (Item == null)
                    throw new ApplicationException($"Language not registered: {TwoLetterLanguageCode}");
                return Item;
            }
        }

        /// <summary>
        /// Registers a language
        /// </summary>
        static public void Add(LanguageItem Item)
        {
            lock (syncLock)
            {
                if (!Contains(Item.Code))
                    fItems.Add(Item);
            }
        }

        /* properties */
        /// <summary>
        /// The default language
        /// </summary>
        static public LanguageItem DefaultLanguage
        {
            get
            {
                LanguageItem Result = fItems.FirstOrDefault(item => string.Compare(item.Code, "en", StringComparison.InvariantCultureIgnoreCase) == 0);
                return Result != null ? Result : new LanguageItem() {
                    Id = "", 
                    Name = "English", 
                    Code = "en", 
                    CultureCode ="en-US"
                };
            }
        }
        /// <summary>
        /// The number of registered languages
        /// </summary>
        static public int Count { get { lock (syncLock) return fItems.Count; } }
        /// <summary>
        /// The list of registered languages
        /// </summary>
        static public LanguageItem[] Items { get { return fItems.ToArray(); } }
    }



}
