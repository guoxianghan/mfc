using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using SNTON.Constants;
using log4net;
using VI.MFC;
using VI.MFC.Components;
using VI.MFC.Logging;
using VI.MFC.Utils.ConfigBinder;

namespace SNTON.Components.Textkeys
{
    public class TextKeysReadIn : VIRuntimeComponent, IViSupportingComponent, ITextKeysReadIn
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        // This will contain the path to the language folders
        List<string> listOfLanguages = new List<string>();
        public Dictionary<string, string> textKeyDictionary = new Dictionary<string, string>();

#pragma warning disable 649

        [ConfigBoundProperty("TextkeyPathEN")]
        private string textkeyPathEN;

        [ConfigBoundProperty("TextkeyPathZH")]
        private string textkeyPathZH;

#pragma warning restore 649
        
        public override string GetInfo()
        {
            return "Translates a given Textkey.";
        }

        /// <summary>
        /// Creates an instance of the TextkeysReadIn component.
        /// </summary>
        /// <param name="theConfigNode">XML configuration Node used.</param>
        /// <returns>reference to newly created module.</returns>
        public static TextKeysReadIn Create(XmlNode theConfigNode)
        {
            TextKeysReadIn module = new TextKeysReadIn();
            module.Init(theConfigNode);
            return module;
        }

        ///// <summary>
        ///// Start the class
        ///// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();
            logger.InfoMethod("TextKey broker started.");
            ReadTextkeys(listOfLanguages);
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="configNode">the configuration</param>
        public override void Init(XmlNode configNode)
        {
            base.Init(configNode);
            // Read the Textkeys on init            
            textKeyDictionary =  ReadTextkeys(listOfLanguages);

        }

        /// <summary>
        /// Validate the parameters we just read in.
        /// The Housekeeping parameters will be called in the CleanUpBroker
        /// </summary>
        protected override void ValidateParameters()
        {
            if (!string.IsNullOrWhiteSpace(textkeyPathEN))
                {
                    try
                    {
                        // Check Path for english textkeys
                        textkeyPathEN = Path.GetFullPath(textkeyPathEN);
                    }
                    catch (Exception)
                    {
                        ThrowArgumenException(string.Format("Please specify a valid English text key path ({0}).", textkeyPathEN));
                    }
                listOfLanguages.Add(textkeyPathEN);
                }

            if (!string.IsNullOrWhiteSpace(textkeyPathZH))
            {
                try
                {
                    // Check Path for chinese textkeys
                    textkeyPathZH = Path.GetFullPath(textkeyPathZH);
                }
                catch (Exception)
                {
                    ThrowArgumenException(string.Format("Please specify a valid Chinese text key path ({0}).", textkeyPathZH));
                }
                listOfLanguages.Add(textkeyPathZH);
            }
        }

        /// <summary>
        /// This method will read all textkey xml files in the paths given in the List listOflanguages and add it to the dictionary
        /// </summary>
        /// <param name="listOfLanguages">List of folder path with xml textkey files, according the given path of the mfc.xml </param>
        /// <returns>Dictonary of all textkeys</returns>
        private Dictionary<string, string> ReadTextkeys(List<string> listOfLanguages )
        {
            //foreach language -> each language has a folder with files
            foreach (string language in listOfLanguages)
            {
                //foreach file in this folder
                List<string> allFiles = GetAllFiles(language);
                if (allFiles.Count > 0)
                {
                    foreach (string file in allFiles)
                    {
                        // read in all keys 
                        textKeyDictionary = ReadFileToDict(file, textKeyDictionary);
                    }
                }
            }
            return textKeyDictionary;
        }

        /// <summary>
        /// Reads one textkey xml file and adds it to a temporary dictonary file for later merge with the old one
        /// The keys will get two additional characters according to the just reading textfile language
        /// "keyCN" or "keyEN", these textkeys files are not allowed to have identical textkeys with diffrent values,
        /// so there can be key="login" value="log in" in more than one file but not key="login" value="Please login" and in another file
        /// key="login" value="log in" - this will throw an exception 
        /// </summary>
        /// <param name="file">Path to the textkey xml file</param>
        /// <param name="dictTextkeys">The textkeydictonary</param>
        /// <returns></returns>
        public Dictionary<string,string> ReadFileToDict(string file, Dictionary<string,string> dictTextkeys )
        {
            XNamespace ns = "http://vanderlande.com/spec/TextDefinitions";
            //Get language for TextKey
            string languageTextKey = (file.StartsWith(textkeyPathEN) ? SNTONConstants.TextKeyLanguage.En : SNTONConstants.TextKeyLanguage.Cn);
            try
            {
                XElement xdoc = XElement.Load(file);
                Dictionary<string, string> nextDictTextkeys = xdoc.Descendants(ns + "text")
                                                                  .ToDictionary(d => (string)d.Attribute("key") + languageTextKey,
                                                                                d => (string)d.Value);

                // Append this new dictonary to the old one and check for duplicates
                dictTextkeys = MergeDictionary(dictTextkeys, nextDictTextkeys, file);
            }
            catch (Exception)
            {   // If we have an exception here, than there are 2 identical textkeys in the same file
//                ThrowArgumenException(String.Format("Duplicate textkey entry in file: {0}. {1}", file, e));
                logger.WarnMethod(String.Format("Duplicate textkey entry in file: {0}.", file));
            }

            return dictTextkeys;
        }

        /// <summary>
        /// Merging the temporary dictonary with the textkey dictonary and checking for duplicates
        /// </summary>
        /// <param name="oldDictTextkeys">Textkey dictonary</param>
        /// <param name="newDictTextkeys">temporary textkey dictonary</param>
        /// <param name="file">Path of the now merging textkeys</param>
        /// <returns>Textkey dictonary</returns>
        private Dictionary<string, string> MergeDictionary(Dictionary<string, string> oldDictTextkeys, Dictionary<string, string> newDictTextkeys, string file )
        {
            // This will throw and exception when there a duplicates
            try
            {
                foreach (var newDict in newDictTextkeys)
                {
                    // check first if the key is already in the dictonary
                    if (!oldDictTextkeys.ContainsKey(newDict.Key))
                    {
                        // If the key does not exist - > add
                        oldDictTextkeys.Add(newDict.Key, newDict.Value);
                    }
                    else
                    {
                        // check if value is also the same
                        // if value is the same, don't add because it is already in the dictonary
                        // if value is different -> warning, this entry should then have a different key
                        if (!ContainsKeyValuepair(oldDictTextkeys, newDict.Key, newDict.Value))
                        {
                            logger.InfoMethod(String.Format("Duplicate TextKey, but different value, please change the key of: File: {0} - Textkey: {1} - Value: {2}", file, newDict.Key, newDict.Value));
                        }
                    }
                    
                }
            }
            catch(Exception e)
            {
                ThrowArgumenException(String.Format("Duplicate textkey entry in file: {0}. {1}", file, e));
            }
            return oldDictTextkeys;
        }

        /// <summary>
        /// Checks if the given key value pair already exists in the dictonary
        /// </summary>
        /// <param name="dictionary">Textkey dictonary</param>
        /// <param name="expectedKey">key</param>
        /// <param name="expectedValue">value</param>
        /// <returns>True if the key value pair already exists</returns>
        public bool ContainsKeyValuepair(Dictionary<string, string> dictionary, string expectedKey, string expectedValue)
        {
            string actualValue;
            if (!dictionary.TryGetValue(expectedKey, out actualValue))
            {
                return false;
            }
            return actualValue == expectedValue;
        }
        /// <summary>
        /// Helper class which logs error and throws new ArgumentException
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <exception cref="ArgumentException">Throw the message</exception>
        protected void ThrowArgumenException(String message)
        {
            logger.ErrorMethod(message);
            throw new ArgumentException(message);
        }

        /// <summary>
        /// Puts all filenames with path from a given folder into a list
        /// the list will be sorted ascending
        /// </summary>
        /// <param name="directory">archiving directory</param>
        /// <returns>Path of all files</returns>
        static List<String> GetAllFiles(String directory)
        {
            return Directory.GetFiles(directory, "*.xml*", SearchOption.AllDirectories).ToList();
        }

        /// <summary>
        /// Translate the overgiven textkey to the value
        /// </summary>
        /// <param name="key">Textkey key</param>
        /// <returns>Value</returns>
        public string Translate(string key)
        {
            if (textKeyDictionary.ContainsKey(key))
            {
                return textKeyDictionary[key];
            }
            return string.Format("[key {0} not found]",key);
        }
    }
}
