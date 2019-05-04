using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BlobAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /*
                 * Getting configuration data for application
                 */
                string connectionString = ConfigurationManager.ConnectionStrings["AzureConnectionString"].ConnectionString;
                string localFolder = ConfigurationManager.AppSettings["sourceFolder"];
                string destConteiner = ConfigurationManager.AppSettings["destinationConteiner"];
                string downloadContainer = ConfigurationManager.AppSettings["containerForDownload"];

                Console.WriteLine(@"Connection to storage account");
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                Console.WriteLine(@"Getting reference to conteiner");
                CloudBlobContainer container = blobClient.GetContainerReference(downloadContainer);
                

                // If container not exist - create container in blob storage
                container.CreateIfNotExists();

                // Retrieving files for uploads
                string[] fileEntries = Directory.GetFiles(localFolder);
                if (fileEntries.Count() > 0)
                {
                    Console.WriteLine("Uploading files..." + Environment.NewLine);

                    foreach (string filePath in fileEntries)
                    {
                        //Create new file path for file
                        string key = DateTime.UtcNow.ToString("yyyy-MM-dd-HH_mm_ss") + "-" + Path.GetFileName(filePath);

                        UploadBlob(container, key, filePath, true);
                    }
                }
                
                DownloadBlob(container, localFolder);

            } catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Upload file to blob storage
        /// </summary>
        /// <param name="container">BLOB container</param>
        /// <param name="key">File path</param>
        /// <param name="fileName">File name in storage</param>
        /// <param name="deleteAfter">If need delete from folder after uploading</param>
        /// <param name="maxAge">If need cach controle set string like max-age=3600</param>
        private async static void UploadBlob(CloudBlobContainer container, string key, string fileName, bool deleteAfter, string maxAge = "")
        {
            // Retrieving files not for uploading
            string[] fExeptions = fileName.Split('\\').Where(item => item == ".gitkeep").ToArray();

            if (fExeptions.Count() == 0)
            {
                try
                {
                    //Get reference to blob object in BLOB container
                    CloudBlockBlob blob = container.GetBlockBlobReference($"{DateTime.UtcNow.ToString("yyyy-MM-dd")}/{key}");

                    //Saving file to blob
                    using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        await blob.UploadFromStreamAsync(fs);
                    }
;
                    ShowMessage("File Uri for Reading: " + Environment.NewLine + CreateFileUri(blob));

                    //Deleting file from local folder
                    if (deleteAfter)
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message, 0);
                }
            }
        }

        /// <summary>
        /// Download folders from BLOB storage 
        /// </summary>
        /// <param name="container">BLOB container</param>
        /// <param name="localFolder">Local main path where save downloaded files</param>
        private async static void DownloadBlob(CloudBlobContainer container, string localFolder)
        {
            BlobContinuationToken dirToken = null;
            do
            {
                var dirResult = await container.ListBlobsSegmentedAsync(dirToken);
                dirToken = dirResult.ContinuationToken;
                
                foreach (var dirItem in dirResult.Results)
                {
                    if (dirItem is CloudBlobDirectory)
                    {
                        var dir = dirItem as CloudBlobDirectory;
                        Console.WriteLine($"Start download directory:\n{dir.Prefix}");
                        Console.WriteLine(Environment.NewLine);
                        
                        BlobContinuationToken blobToken = null;
                        var blobResult = await dir.ListBlobsSegmentedAsync(blobToken);

                        foreach (var blobItem in blobResult.Results)
                        {
                            if (blobItem is CloudBlockBlob)
                            {
                                DownloadFileFromBlob(blobItem as CloudBlockBlob, localFolder);
                            }
                        }
                        ShowMessage("\tDownload succefull!", 1);
                    }
                }
            } while (dirToken != null);
        }

        /// <summary>
        /// Download file from Blob container
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="pathTo"></param>
        private async static void DownloadFileFromBlob(CloudBlockBlob blob, string pathTo)
        {
            try
            {
                blob.Properties.CacheControl = (3600 * 24 * 7).ToString();

                string fileName = blob.Name;
                fileName = fileName.Replace("/", "\\");

                string folderName = fileName.Split('\\')[0];
                Directory.CreateDirectory($"{pathTo}\\{folderName}");

                Console.WriteLine($"\tDownload file: {fileName}");

               await blob.DownloadToFileAsync($"{pathTo}/{fileName}", FileMode.Create);
            } catch(Exception ex)
            {
                ShowMessage("ERROR: " + ex.Message, 0);
            }
        }

        /// <summary>
        /// Create file uri for reading
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private static string CreateFileUri(CloudBlockBlob blob)
        {
            //Create contrains for uri
            SharedAccessBlobPolicy constraints = new SharedAccessBlobPolicy();
            
            //Time expired
            constraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(7);

            //Create permission for only reading
            constraints.Permissions = SharedAccessBlobPermissions.Read;

            //Generate token
            string token = blob.GetSharedAccessSignature(constraints);

            //Return uri with token
            return blob.Uri + token;
        }

        /// <summary>
        /// Show color message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color">0 - red, 1 - green, default - yellow</param>
        private static void ShowMessage(string message, short color = 2)
        {
            switch(color)
            {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.WriteLine(message);
            Console.WriteLine(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
