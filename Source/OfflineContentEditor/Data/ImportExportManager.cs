using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using LumenWorks.Framework.IO.Csv;
using Sitecore.Shell.Feeds.Sections;
using TheDreamCorps.OfflineContentEditor.Common;
using Sitecore;
using Sitecore.Jobs;

namespace TheDreamCorps.OfflineContentEditor.Data
{
    public static class ImportExportManager
    {
        public static string _csvPathAndFileName = string.Empty;

        public static string seprator { get { return ", "; } }

        //Get the filename of the csv file
        public static string FileName
        {
            get
            {
                return "DreamCorps" + DateTime.Now.ToString("MM-DD-YYYY-mm-hh") + ".csv";
            }
        }
        /// <summary>
        /// Generates a csv file given a list of items and fields to export.
        /// 
        /// </summary>
        /// <param name="itemsToExport">Items to export</param>
        /// <param name="fieldsToExport">fields to export</param>
        /// <param name="format">format to generate file in</param>
        /// <returns></returns>
        public static bool GenerateFile(List<Item> itemsToExport, List<ID> fieldsToExport, string filename, FileFormat format)
        {
            
            //Starting with the StringBuilder to store the info.
            StringBuilder objsb = new StringBuilder();

            //If there are no items to export then return false
            if (itemsToExport == null || itemsToExport.Count <= 0) return false;

            //If the csv file does not exist, then create the csv file.
            if (!Utility.DoesFileExist(FileName))
            {
                Utility.CreateCSVFile(_csvPathAndFileName);
            }
            try
            {
                //create header
                objsb.Append("ItemID");
                objsb.Append(seprator);

                Item itemForFields = itemsToExport[0];

                List<ID> validFields = fieldsToExport;
                int i = 0;

                //Iterating through valid fields and appending it to the string builder.
                foreach (ID fieldID in validFields)
                {
                    objsb.Append(itemForFields.Fields[fieldID].Name);
                    objsb.Append(" - ");
                    objsb.Append(fieldID);
                    i++;
                    if (i != validFields.Count)
                    {
                        objsb.Append(", ");
                    }
                }

                int x = 0;

                //Iterating through the Items To Export parameters and appending it to the stringbuilder. 
                foreach (Item item in itemsToExport)
                {
                    if (item != null)
                    {
                        objsb.Append(System.Environment.NewLine);
                        objsb.Append(item.ID);
                        
                        //Iterating through each field through valid fields.
                        foreach (ID fieldID in validFields)
                        {
                            objsb.Append(seprator);
                            objsb.Append(item.Fields[fieldID].Value);
                        }
                    }
                }

                if (objsb.ToString() != null && objsb.ToString().Length > 0)
                {
                    System.IO.File.WriteAllText(filename, objsb.ToString());
                    return true;
                }
            }
            catch (System.IO.IOException ex)
            {
                //Write  apprpriate handling
            }
            catch (System.Exception ex)
            {
                //Write  apprpriate handling
            }
            return false;
        }


        /// <summary>
        /// Updates a list of items from a given csv file
        /// </summary>
        /// <param name="fileName">Name of the csv file to open</param>
        public static void UpdateItemsFromFile(string fileName)
        {
            try
            {
                //Get path and filename
                string csvPathFilename = Settings.GetSetting(OfflineContentEditorSettings.UploadFolder) + "\\" + fileName;

                //Creating the using statement to read the file.
                using (CsvReader csv = new CsvReader(new StreamReader(csvPathFilename), true))
                {
                    //Get header of csv file (first line)
                    List<string> headerValues = csv.GetFieldHeaders().ToList();
                    List<string> guids = new List<string>();

                    //Get all guids from header
                    foreach (var headerValue in headerValues)
                    {
                        var rx = new Regex("{");

                        if (headerValue.Contains("{"))
                        {
                            guids.Add("{" + rx.Split(headerValue)[1]);
                        }
                    }

                    //Iterate through each row of csv file and update items
                    while (csv.ReadNextRecord())
                    {
                        //Get the current item and update fields
                        Sitecore.Data.Database master = Sitecore.Configuration.Factory.GetDatabase("master");
                        Item item = master.GetItem(csv[0]);

                        for (int i = 1; i < csv.FieldCount; i++)
                        {
                            if (item != null)
                            {
                                item.Editing.BeginEdit();
                                item.Fields[new ID(guids[i - 1])].Value = csv[i];
                                item.Editing.EndEdit();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Write exception here.
            }
        }        
    }

    public class ExportRunner
    {
        public void RunExport(List<Item> itemsToExport, List<ID> fieldsToExport, string fileName, FileFormat format)
        {
            if (itemsToExport.Count > 0 && fieldsToExport.Count > 0)
            {
                ImportExportManager.GenerateFile(itemsToExport, fieldsToExport, fileName, format);

                JobStatus status = Sitecore.Context.Job.Status;

                status.State = JobState.Finished;
            }
            else
            {
                
            }
        }
    }

    /// <summary>
    /// Import Runner Class
    /// </summary>
    public class ImportRunner
    {
        public void RunImport(string file) 
        {

            ImportExportManager.UpdateItemsFromFile(file);

            JobStatus status = Sitecore.Context.Job.Status;

            status.State = JobState.Finished;
        }
    }
}
