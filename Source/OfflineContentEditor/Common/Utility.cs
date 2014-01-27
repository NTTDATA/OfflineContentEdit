using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Configuration;

namespace TheDreamCorps.OfflineContentEditor.Common
{
    public static class Utility
    {
        /// <summary>
        /// Checking if the file exists or not. 
        /// </summary>
        /// <param name="fileName">Name of csv file</param>
        /// <returns></returns>
        public static bool DoesFileExist(string fileName)
        {
            bool result = false;
            string filepath = Settings.GetSetting(OfflineContentEditorSettings.DownloadFolder) + "\\" + fileName;

            if (filepath.Length > 0)
            {
                return System.IO.File.Exists(filepath);
            }
            return result;
        }

        /// <summary>
        /// Create csv file if it does not exist
        /// </summary>
        /// <param name="file">File name and path</param>
        /// <returns></returns>
        public static void CreateCSVFile(string file)
        {
            try
            {
                using (File.Create(file)) ;
            }
            catch (Exception e)
            {
            }
        }
    }
}
