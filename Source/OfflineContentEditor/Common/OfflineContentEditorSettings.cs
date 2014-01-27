using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDreamCorps.OfflineContentEditor.Common
{
    public static class OfflineContentEditorSettings
    {

        public static string UploadFolder = "OfflineContentEditor.UploadFolder";

        public static string DownloadFolder = "OfflineContentEditor.DownloadFolder";            

    }


    public enum FileFormat 
    {
        None = 0,
        Csv = 1,
        Tab = 2
    }
}
